﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WebApi.Models;
using WebApi.Proxy;
using WebApi.Repository;
using WebApi.ViewModels;

namespace WebApi.Services
{
    public class EmlakBaza
    {
        private readonly UnitOfWork unitOfWork;
        private readonly ProxysHttpClient proxysHttp;

        public EmlakBaza(UnitOfWork unitOfWork, ProxysHttpClient proxysHttp)
        {
            this.unitOfWork = unitOfWork;
            this.proxysHttp = proxysHttp;
        }
        public async void CheckAsync(HttpClient httpClient,int id,params string[] numbers)
        {
            try
            {
                await Task.Run(async () =>
                {
                    for (int i = 0; i < numbers.Length; i++)
                    {
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
                                    Console.WriteLine(result);
                                    
                                    if (result != null)
                                    {
                                        if (result.Trim() == "Vasitəçidir")
                                        {
                                            Console.WriteLine(result.Trim());
                                            unitOfWork.RieltorRepository.Create(new Rieltor { Phone = numbers[i] });
                                            await unitOfWork.Announces.UpdateAnnouncerAsync(new AnnounceAnnouncerUpdateViewModel { OriginalId = id, Announcer = 2 });

                                        }
                                        else if (result.Trim() == "Vasitəçi deyil")
                                        {
                                            Console.WriteLine(result.Trim());
                                            unitOfWork.OwnerRepository.Create(new Owner { Phone = numbers[i] });
                                            await unitOfWork.Announces.UpdateAnnouncerAsync(new AnnounceAnnouncerUpdateViewModel { OriginalId = id, Announcer = 1 });
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