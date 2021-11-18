using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WebApi.Repository;

namespace WebApi.Services.EmlakciAz
{
    public class EmlakciAzParser
    {
        private readonly EmlakBaza _emlakBaza;
        private readonly HttpClientCreater clientCreater;
        private static bool isActive = false;
        private readonly UnitOfWork unitOfWork;
       

        private HttpClient _httpClient;
        HttpResponseMessage header;
        public int maxRequest = 200;
        static string[] proxies = SingletonProxyServersIp.Instance;

        public EmlakciAzParser(EmlakBaza emlakBaza,
            HttpClientCreater clientCreater,
            UnitOfWork unitOfWork,
            HttpClient httpClient)
        {
            this._emlakBaza = emlakBaza;
            this.clientCreater = clientCreater;
            this.unitOfWork = unitOfWork;

            // _httpClient = clientCreater.Create(proxies[0]);
            _httpClient = httpClient;
        }
        public async Task EmlakciAzPars()
        {
            var model = unitOfWork.ParserAnnounceRepository.GetBySiteName("https://emlakci.az");
            if (!isActive)
            {
                if (model.isActive)
                {

                    Console.WriteLine("TEST");
                }
            }
        }
    }
}
