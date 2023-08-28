using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WebApi.Models;
using WebApi.Repository;
using WebApi.ViewModels;

namespace WebApi.Services
{
    public class ArendaAzEmlakBazaWithProxy
    {
        private readonly UnitOfWork unitOfWork;
        private readonly HttpClientCreater clientCreater;
        HttpClient httpClient;
        static string[] proxies = SingletonProxyServersIp.Instance;

        public ArendaAzEmlakBazaWithProxy(UnitOfWork unitOfWork,
                                          HttpClientCreater clientCreater
                                            )
        {
            this.unitOfWork = unitOfWork;
            this.clientCreater = clientCreater;
            //proxies = File.ReadAllLines("proxies.txt");

        }
        public async Task CheckAsync(int id, params string[] numbers)
        {
            try
            {

                Random rnd = new Random();
                httpClient = clientCreater.Create(proxies[rnd.Next(0, 99)]);
                bool turn = false;
                for (int i = 0; i < numbers.Length; i++)
                {
                    httpClient = clientCreater.Create(proxies[rnd.Next(0, 99)]);
                    httpClient = new HttpClient();
                    var values = new Dictionary<string, string>();

                    values.Add("number", numbers[i]);

                    var content = new FormUrlEncodedContent(values);

                    var response = await httpClient.PostAsync("https://emlak-bazasi.com/search/agency/", content);

                    if (response != null)
                    {
                        var responseString = await response.Content.ReadAsStringAsync();

                        if (!string.IsNullOrEmpty(responseString))
                        {
                            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                            doc.LoadHtml(responseString);

                            var counts = doc.DocumentNode.SelectNodes(".//div[@class='count']");
                            if (counts != null && counts[1] != null)
                            {
                                string result = counts[1].InnerText.Trim();

                                if (result != null)
                                {
                                    if (result.Trim() == "Vasitəçidir")
                                    {
                                        for (int j = 0; j < numbers.Length; j++)
                                        {
                                            await unitOfWork.RieltorRepository.CreateAsync(new Rieltor { Phone = numbers[j] });
                                        }

                                        await unitOfWork.Announces.ArendaAzUpdateAnnouncerAsync(new ArendaAzAnnouncerUpdateVM { Id = id, Announcer = 2 });
                                        turn = true;
                                        break;

                                    }
                                    else if (result.Trim() == "Vasitəçi deyil")
                                    {


                                        if (i == (numbers.Length - 1))
                                        {
                                            await unitOfWork.Announces.ArendaAzUpdateAnnouncerAsync(new ArendaAzAnnouncerUpdateVM { Id = id, Announcer = 1 });

                                        }

                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
            }
        }
    }
}
