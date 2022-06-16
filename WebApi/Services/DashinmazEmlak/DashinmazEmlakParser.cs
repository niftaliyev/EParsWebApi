using HtmlAgilityPack;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using WebApi.Models;
using WebApi.Repository;

namespace WebApi.Services.DashinmazEmlak
{
    public class DashinmazEmlakParser
    {
        private readonly EmlakBaza _emlakBaza;
        private static bool isActive = false;
        private readonly UnitOfWork unitOfWork;

        private HttpClient _httpClient;
        HttpResponseMessage header;
        public int maxRequest = 50;
        static string[] proxies = SingletonProxyServersIp.Instance;

        public DashinmazEmlakParser(EmlakBaza emlakBaza,
            UnitOfWork unitOfWork,
            HttpClient httpClient)
        {
            this._emlakBaza = emlakBaza;
            this.unitOfWork = unitOfWork;
            _httpClient = httpClient;
        }

        public async Task DashinmazEmlakPars()
        {
            var model = unitOfWork.ParserAnnounceRepository.GetBySiteName("https://dashinmazemlak.az");

            if (!isActive)
            {
                if (model.isActive)
                {
                    try
                    {
                        var id = model.last_id;

                        isActive = true;
                        int x = 0;
                        int count = 0;
                        int duration = 0;


                        while (true)
                        {
                            try
                            {
                                Announce announce = new Announce();
                                header = await _httpClient.GetAsync($"{model.site}/az/satilir/{++id}.html");
                                string url = header.RequestMessage.RequestUri.AbsoluteUri;

                                var response = await _httpClient.GetAsync(url);
                                if (response.IsSuccessStatusCode)
                                {
                                    var html = await response.Content.ReadAsStringAsync();
                                    if (!string.IsNullOrEmpty(html))
                                    {
                                        HtmlDocument doc = new HtmlDocument();
                                        doc.LoadHtml(html);


                                        if (doc.DocumentNode.SelectSingleNode(".//div[@class='elan_read_content_text']") != null)
                                        {
                                            Console.WriteLine(doc.DocumentNode.SelectSingleNode(".//div[@class='elan_read_content_text']").InnerText);
                                        }
                                    }
                                }
                            }
                            catch (System.Exception)
                            {

                                throw;
                            }
                        }
                    }
                    catch
                    {
                    }
                }

            }


        }
    }
}
