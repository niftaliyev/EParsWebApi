using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WebApi.Models;
using WebApi.Repository;
using WebApi.Services.EmlakAz;

namespace WebApi.Services.YeniEmlakAz
{
    public class YeniEmlakAzParser
    {
        private readonly HttpClient httpClient;
        private readonly EmlakBaza emlakBaza;
        private readonly UnitOfWork unitOfWork;
        HttpResponseMessage header;

        public YeniEmlakAzParser(HttpClient httpClient, EmlakBaza emlakBaza, UnitOfWork unitOfWork)
        {
            this.httpClient = httpClient;
            this.emlakBaza = emlakBaza;
            this.unitOfWork = unitOfWork;
        }

        public async Task YeniEmlakAzPars()
        {

            var model = unitOfWork.ParserAnnounceRepository.GetBySiteName("https://yeniemlak.az");
            var id = model.last_id;
            int counter = 0;


            while (true)
            {
                Announce announce = new Announce();
                try
                {

                    header = await httpClient.GetAsync($"{model.site}/elan/{++id}");
                    string url = header.RequestMessage.RequestUri.AbsoluteUri;
                    Console.WriteLine(id);
                        var response = await httpClient.GetAsync(url);
                        if (header.IsSuccessStatusCode)
                        {
                            var html = await response.Content.ReadAsStringAsync();
                            if (!string.IsNullOrEmpty(html))
                            {
                                HtmlDocument doc = new HtmlDocument();
                                doc.LoadHtml(html);
                                Console.WriteLine(doc.DocumentNode.SelectSingleNode(".//div[@class='text']") == null ? "not null" : doc.DocumentNode.SelectSingleNode(".//div[@class='text']").InnerText);
                            }
                        }
                }
                catch (Exception e)
                {
                    Console.WriteLine("No Connection");
                }
            }
        }
    }
}
