﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WebApi.Models;
using WebApi.Proxy;
using WebApi.Repository;
using WebApi.ViewModels;

namespace WebApi.Services
{
    public class EmlakBazaWithProxy
    {
        private readonly UnitOfWork unitOfWork;
        private readonly HttpClientCreater clientCreater;
        HttpClient httpClient;
        static string[] proxies; 

        public EmlakBazaWithProxy(UnitOfWork unitOfWork, HttpClientCreater clientCreater)
        {
            this.unitOfWork = unitOfWork;
            this.clientCreater = clientCreater;
            proxies = File.ReadAllLines("proxies.txt");

        }
        public async void CheckAsync(int id, params string[] numbers)
        {
            try
            {
                await Task.Run(async () =>
                {
                    Random rnd = new Random();
                    httpClient = clientCreater.Create(proxies[rnd.Next(0, 350)]);

                    for (int i = 0; i < numbers.Length; i++)
                    {
                        httpClient = clientCreater.Create(proxies[rnd.Next(0,350)]);
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
                                            await unitOfWork.RieltorRepository.CreateAsync(new Rieltor { Phone = numbers[i] });
                                            await unitOfWork.Announces.UpdateAnnouncerAsync(new AnnounceAnnouncerUpdateViewModel { OriginalId = id, Announcer = 2 });

                                        }
                                        else if (result.Trim() == "Vasitəçi deyil")
                                        {
                                            Console.WriteLine(result.Trim());
                                            await unitOfWork .OwnerRepository.CreateAsync(new Owner { Phone = numbers[i] });
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
