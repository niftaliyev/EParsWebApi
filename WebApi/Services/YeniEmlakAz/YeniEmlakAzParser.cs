using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
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
        private HttpClient httpClient;
        private readonly EmlakBaza emlakBaza;
        private static bool isActive = false;
        private readonly UnitOfWork unitOfWork;
        private readonly HttpClientCreater clientCreater;
        HttpResponseMessage header;
        static string[] proxies; // лучше добавить ентер

        public YeniEmlakAzParser(EmlakBaza emlakBaza, UnitOfWork unitOfWork , HttpClientCreater clientCreater )
        {
            proxies = File.ReadAllLines("proxies.txt");
            httpClient = clientCreater.Create(proxies[0]);
            this.emlakBaza = emlakBaza;
            this.unitOfWork = unitOfWork;
            this.clientCreater = clientCreater;
        }

        public async Task YeniEmlakAzPars()
        {

            var model = unitOfWork.ParserAnnounceRepository.GetBySiteName("https://yeniemlak.az");
            var id = model.last_id;
            int counter = 0;


            if (!isActive)
            {
                if (model.isActive)
                {
                    int x = 0;
                    int count = 0;
                    while (true)
                    {
                        if (count >= 10)
                        {
                            x++;
                            if (x >= 350)
                                x = 0;

                            httpClient = clientCreater.Create(proxies[x]);
                            count = 0;
                        }

                        Announce announce = new Announce();
                        try
                        {

                            header = await httpClient.GetAsync($"{model.site}/elan/{++id}");
                            string url = header.RequestMessage.RequestUri.AbsoluteUri;
                            count++;
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
    }
}
