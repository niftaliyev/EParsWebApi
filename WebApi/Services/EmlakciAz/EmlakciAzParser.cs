using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WebApi.Models;
using WebApi.Repository;

namespace WebApi.Services.EmlakciAz
{
    public class EmlakciAzParser
    {
        private readonly EmlakBaza _emlakBaza;
        //private readonly HttpClientCreater clientCreater;
        private static bool isActive = false;
        private readonly UnitOfWork unitOfWork;
       

        private HttpClient _httpClient;
        HttpResponseMessage header;
        public int maxRequest = 10;
        //static string[] proxies = SingletonProxyServersIp.Instance;

        public EmlakciAzParser(EmlakBaza emlakBaza,
            HttpClientCreater clientCreater,
            UnitOfWork unitOfWork,
            HttpClient httpClient)
        {
            this._emlakBaza = emlakBaza;
            //this.clientCreater = clientCreater;
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
                    var id = model.last_id;

                    isActive = true;
                    int x = 0;
                    int count = 0;
                    int duration = 0;

                    while (true)
                    {


                        try
                        {

                            Console.WriteLine(model.site);

                            Uri myUri = new Uri($"{model.site}/elanlar/view/{++id}", UriKind.Absolute);
                            header = await _httpClient.GetAsync(myUri);
                            var url = header.RequestMessage.RequestUri.AbsoluteUri;
                            HtmlDocument doc = new HtmlDocument();

                            if (!header.RequestMessage.RequestUri.ToString().StartsWith("https://emlakci.az/index/index"))
                            {
                                var response = await _httpClient.GetAsync(url);
                                Console.WriteLine(response.StatusCode.ToString());


                                if (response.IsSuccessStatusCode)
                                {
                                    var html = await response.Content.ReadAsStringAsync();
                                    if (!string.IsNullOrEmpty(html))
                                    {
                                        doc.LoadHtml(html);
                                        Announce announce = new Announce();

                                        if (doc.DocumentNode.SelectSingleNode(".//div[@class='elan_inner_info']") != null)
                                        {
                                            Console.WriteLine(doc.DocumentNode.SelectSingleNode(".//div[@class='elan_inner_info']").InnerText);
                                            announce.text = doc.DocumentNode.SelectSingleNode(".//div[@class='elan_inner_info']").InnerText;
                                        }

                                        if (doc.DocumentNode.SelectSingleNode(".//div[@class='elan_inner_left_basliq']") != null)
                                        {
                                            Console.WriteLine(doc.DocumentNode.SelectSingleNode(".//div[@class='elan_inner_left_basliq']//span").InnerText);
                                            Console.WriteLine($"ID: {doc.DocumentNode.SelectSingleNode(".//div[@class='elan_inner_left_basliq']//label").InnerText.Replace("#","")}");
                                        }

                                        Console.WriteLine(doc.DocumentNode.SelectNodes(".//div[@class='elan_inner_connect_phone_ad']")[0]?.InnerText);


                                        var charsToRemove = new string[] { "(", ")", "-", ".", " " };

                                        List<string> numbers = new List<string>();
                                        for (int i = 1; i < doc.DocumentNode.SelectNodes(".//div[@class='elan_inner_connect_phone_ad']").Count; i++)
                                        {
                                            var number = doc.DocumentNode.SelectNodes(".//div[@class='elan_inner_connect_phone_ad']")[i]?.InnerText;
                                            foreach (var c in charsToRemove)
                                            {
                                                number = number.Replace(c, string.Empty);

                                            }
                                            if (number != "&nbsp;" || number != " ")
                                                numbers.Add(number);

                                        }

                                        foreach (var number in numbers)
                                        {
                                            Console.WriteLine(number);
                                        }

                                        if (doc.DocumentNode.SelectSingleNode(".//div[@class='elan_inner_right']") != null)
                                        {
                                            int xIndex = 0;
                                            foreach (var item in doc.DocumentNode.SelectNodes(".//div[@class='elan_inner_info_param_left']"))
                                            {
                                                //if (item.SelectSingleNode(".//div[@class='elan_inner_info_param_left']").InnerText == "Qiyməti:")
                                                //    Console.WriteLine($"QIYMET {item.SelectSingleNode(".//div[@class='elan_inner_info_param_right']").InnerText}");
                                                if (item.InnerText == "Qiyməti:")
                                                    Console.WriteLine(Int32.Parse(doc.DocumentNode.SelectNodes(".//div[@class='elan_inner_info_param_right']")[xIndex].InnerText.Split(" ")[0]));
                                                else if (item.InnerText == "Əmlakın növü:")
                                                    Console.WriteLine(doc.DocumentNode.SelectNodes(".//div[@class='elan_inner_info_param_right']")[xIndex].InnerText);
                                                else if (item.InnerText == "Sahəsi:")
                                                    Console.WriteLine(Int32.Parse(doc.DocumentNode.SelectNodes(".//div[@class='elan_inner_info_param_right']")[xIndex].InnerText.Split(" ")[0]));
                                                else if (item.InnerText == "Otaq sayı:")
                                                    Console.WriteLine(Int32.Parse(doc.DocumentNode.SelectNodes(".//div[@class='elan_inner_info_param_right']")[xIndex].InnerText));
                                                else if (item.InnerText == "Ünvanı:")
                                                    Console.WriteLine(doc.DocumentNode.SelectNodes(".//div[@class='elan_inner_info_param_right']")[xIndex].InnerText);
                                                else if (item.InnerText == "Mərtəbə:")
                                                {
                                                    Console.WriteLine(doc.DocumentNode.SelectNodes(".//div[@class='elan_inner_info_param_right']")[xIndex].InnerText.Split("/")[0]);
                                                    Console.WriteLine(doc.DocumentNode.SelectNodes(".//div[@class='elan_inner_info_param_right']")[xIndex].InnerText.Split("/")[1]);

                                                }

                                                xIndex++;
                                                TelegramBotService.Sender($"{xIndex}");

                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine("Empty");
                                duration++;
                            }
                            if (duration >= maxRequest)
                            {
                                model.last_id = (id - maxRequest);
                                Console.WriteLine("******** END emlakci **********");
                                TelegramBotService.Sender("emlakci.az limited");

                                isActive = false;
                                //unitOfWork.ParserAnnounceRepository.Update(model);
                                duration = 0;
                                break;
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
}
