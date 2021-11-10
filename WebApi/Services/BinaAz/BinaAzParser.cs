using HtmlAgilityPack;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WebApi.Models;
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
        public int maxRequest = 70;
        static string[] proxies = SingletonProxyServersIp.Instance;

        public BinaAzParser(EmlakBaza emlakBaza,
            HttpClientCreater clientCreater,
            UnitOfWork unitOfWork,
            HttpClient _httpClient)
        {
            this._emlakBaza = emlakBaza;
            this.clientCreater = clientCreater;
            this.unitOfWork = unitOfWork;
            this._httpClient = _httpClient;
            //_httpClient = clientCreater.Create(proxies[0]);
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
                            //if (count >= 10)
                            //{
                            //    x++;
                            //    if (x >= 350)
                            //        x = 0;

                            //    _httpClient = clientCreater.Create(proxies[x]);
                            //    count = 0;
                            //}
                            duration++;
                            if (duration >= 5)
                            {
                                duration = 0;
                                break;
                            }

                            try
                            {
                                Console.WriteLine(model.site);

                                Uri myUri = new Uri($"{model.site}/items/{++id}", UriKind.Absolute);
                                header = await _httpClient.GetAsync(myUri);
                                var url = header.RequestMessage.RequestUri.AbsoluteUri;
                                count++;
                                HtmlDocument doc = new HtmlDocument();

                                var response = await _httpClient.GetAsync(url);
                                //Console.WriteLine(response.StatusCode.ToString());
                                //Console.WriteLine(response.IsSuccessStatusCode);

                                

                                if (response.IsSuccessStatusCode)
                                {
                                    var html = await response.Content.ReadAsStringAsync();
                                    if (!string.IsNullOrEmpty(html))
                                    {
                                        doc.LoadHtml(html);
                                        Announce announce = new Announce();
                                        //Console.WriteLine(doc.DocumentNode.SelectSingleNode(".//div[@class='services-container']//h1").InnerText);
                                        announce.address = doc.DocumentNode.SelectSingleNode(".//div[@class='services-container']//h1").InnerText;

                                        Console.WriteLine(doc.DocumentNode.SelectSingleNode(".//div[@class='map_address']").InnerText.Split("Ünvan: ")[1]);
                                        Console.WriteLine($"price: {doc.DocumentNode.SelectSingleNode(".//span[@class='price-val']").InnerText.Replace(" ", "")}");
                                        var cities = unitOfWork.CitiesRepository.GetAll();
                                        foreach (var city in cities)
                                        {
                                            if (doc.DocumentNode.SelectSingleNode(".//div[@class='map_address']").InnerText.Split("Ünvan: ")[1].Contains(city.name))
                                            {
                                                Console.WriteLine(city.name);
                                                break;
                                            }
                                        }
                                        foreach (var item in doc.DocumentNode.SelectNodes(".//ul[@class='locations']//li"))
                                        {
                                            //// metro
                                            if (item.InnerText.Contains(" m."))
                                            {
                                                var metros = unitOfWork.MetrosRepository.GetAll();
                                                foreach (var metro in metros)
                                                {
                                                    if (item.InnerText.Contains(metro.name))
                                                    {
                                                        Console.WriteLine($"metro name: {metro.name}");
                                                        announce.metro_id = metro.id;
                                                        break;
                                                    }
                                                }
                                            }
                                            //////rayon
                                            if (item.InnerText.Contains(" r."))
                                            {
                                                var regions = unitOfWork.CitiesRepository.GetAllRegions();
                                                foreach (var region in regions)
                                                {
                                                    if (item.InnerText.Contains(region.name))
                                                    {
                                                        Console.WriteLine($"region name: {region.name}");
                                                    }
                                                }
                                            }
                                            //////qesebe
                                            if (item.InnerText.Contains(" q."))
                                            {
                                                var settlements = unitOfWork.CitiesRepository.GetAllSettlement();
                                                foreach (var settlement in settlements)
                                                {
                                                    if (item.InnerText.Contains(settlement.name))
                                                    {
                                                        Console.WriteLine($"settlement name: {settlement.name}");
                                                    }
                                                }
                                            }
                                            //Console.WriteLine($"----- {item.InnerText}");
                                        }


                                        ///////////// phone
                                        var phoneResponse = await _httpClient.GetAsync($"{url}/phones");
                                        StringBuilder numbers = new StringBuilder();
                                        if (phoneResponse != null)
                                        {
                                            var json = await phoneResponse.Content.ReadAsStringAsync();
                                            var result = JsonSerializer.Deserialize<PhonesModel>(json);
                                            for (int i = 0; i < result.phones.Length; i++)
                                            {
                                                var charsToRemove = new string[] { "(", ")", "-", ".", " " };
                                                foreach (var c in charsToRemove)
                                                {
                                                    result.phones[i] = result.phones[i].Replace(c, string.Empty);
                                                }
                                                if (i < result.phones.Length-1)
                                                    numbers.Append($"{result.phones[i]},");
                                                else
                                                    numbers.Append($"{result.phones[i]}");
                                            }
                                            announce.mobile = numbers.ToString();
                                        }
                                        ///////////////////////
                                        Console.WriteLine(numbers.ToString());
                                    }
                                }
                                else
                                {
                                    Console.WriteLine(response.StatusCode.ToString());     /// not found
                                    Console.WriteLine(response.IsSuccessStatusCode);       /// false
                                }
                            }
                            catch (Exception e)
                            {

                                Console.WriteLine(e.Message);
                            }
                            Thread.Sleep(10000);
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