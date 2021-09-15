using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WebApi.Models;
using WebApi.Proxy;
using WebApi.Repository;

namespace WebApi.Services
{
    public class EmlakBaza
    {
        private readonly UnitOfWork unitOfWork;
        private readonly ProxysHttpClient proxysHttp;
        private readonly HttpClient httpClient;

        public EmlakBaza(UnitOfWork unitOfWork, ProxysHttpClient proxysHttp,HttpClient httpClient)
        {
            this.unitOfWork = unitOfWork;
            this.proxysHttp = proxysHttp;
            this.httpClient = httpClient;
        }
        public async void CheckAsync(params string[] numbers)
        {
            try
            {
                await Task.Run(() =>
                {
                    for (int i = 0; i < numbers.Length; i++)
                    {
                        var values = new Dictionary<string, string>();

                        values.Add("number", numbers[i]);

                        var content = new FormUrlEncodedContent(values);

                        var response = httpClient.PostAsync("https://emlak-bazasi.com/search/agency/", content).Result;

                        if (response != null)
                        {
                            var responseString = response.Content.ReadAsStringAsync().Result;

                            if (!string.IsNullOrEmpty(responseString))
                            {
                                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                                doc.LoadHtml(responseString);

                                var counts = doc.DocumentNode.SelectNodes(".//div[@class='count']");
                                if (counts != null && counts[1].InnerText != null)

                                {
                                    string result = counts[1].InnerText.Trim();
                                    Console.WriteLine(result);

                                    if (result != null)
                                    {

                                        //context.mediators.Add(new Mediator { Phone = result });
                                        //context.SaveChanges();

                                        if (result.Trim() == "Vasitəçi deyil")
                                        {
                                            //context.Owners.Add(new Owner() { Phone = numbers[i] });
                                            //context.SaveChanges();
                                            Console.WriteLine(result.Trim());
                                        }
                                        else if (result.Trim() == "Vasitəçidir")
                                        {
                                            Console.WriteLine(result.Trim());
                                            //context.Rieltors.Add(new Rieltor() { Phone = numbers[i] });
                                            //context.SaveChanges();

                                        }
                                        else
                                        {
                                            Console.WriteLine(result.Trim());
                                        }
                                    }

                                }

                            }

                        }
                    }




                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Data.ToString());
            }




        }

    }

}