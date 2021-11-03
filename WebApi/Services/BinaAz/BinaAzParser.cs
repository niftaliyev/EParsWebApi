using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WebApi.Repository;

namespace WebApi.Services.BinaAz
{
    public class BinaAzParser
    {
        private readonly EmlakBaza _emlakBaza;
        private readonly HttpClientCreater clientCreater;
        private static bool isActive = false;
        private readonly UnitOfWork unitOfWork;
        private HttpClient _httpClient;
        HttpResponseMessage header;
        public int maxRequest = 200;
        static string[] proxies = SingletonProxyServersIp.Instance;

        public BinaAzParser(EmlakBaza emlakBaza,
            HttpClientCreater clientCreater,
            UnitOfWork unitOfWork)
        {
            this._emlakBaza = emlakBaza;
            this.clientCreater = clientCreater;
            this.unitOfWork = unitOfWork;
            _httpClient = clientCreater.Create(proxies[0]);
            Console.WriteLine(proxies[0]);
        }


        public async Task BinaAzPars()
        {
            var model = unitOfWork.ParserAnnounceRepository.GetBySiteName("https://bina.az");
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
                            if (count >= 10)
                            {
                                x++;
                                if (x >= 350)
                                    x = 0;

                                _httpClient = clientCreater.Create(proxies[x]);
                                count = 0;
                            }


                            try
                            {
                                Console.WriteLine(model.site);

                                Uri myUri = new Uri($"{model.site}/items/{++id}", UriKind.Absolute);
                                header = await _httpClient.GetAsync(myUri);
                                var url = header.RequestMessage.RequestUri.AbsoluteUri;
                                count++;
                                HtmlDocument doc = new HtmlDocument();

                                Console.WriteLine(header.IsSuccessStatusCode);
                            }
                            catch (Exception e)
                            {

                                Console.WriteLine(e.Message);
                            }
                        }
                    }
                    catch (Exception e)
                    {

                        Console.WriteLine(e.Message);
                    }
                }
            }
        }
    }
}