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

        public EmlakBaza(UnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public async Task CheckAsync(HttpClient httpClient,int id,params string[] numbers)
        {
            try
            {
                bool turn = false;

                for (int i = 0; i < numbers.Length; i++)
                    {
                        var values = new Dictionary<string, string>();
                        var number = numbers[i];
                        values.Add("number", number);

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
                                        Console.WriteLine(result.Trim());
                                        Console.WriteLine(numbers[0]);
                                        for (int j = 0; j < numbers.Length; j++)
                                        {
                                            await unitOfWork.RieltorRepository.CreateAsync(new Rieltor { Phone = numbers[j] });
                                        }

                                        await unitOfWork.Announces.UpdateAnnouncerAsync(new AnnounceAnnouncerUpdateViewModel { OriginalId = id, Announcer = 2 });
                                        turn = true;
                                        break;

                                    }
                                    else if (result.Trim() == "Vasitəçi deyil")
                                    {
                                        
                                            Console.WriteLine(result.Trim());
                                            Console.WriteLine("-------------------****************------------------------------");
                                        if (i == (numbers.Length - 1))
                                        {
                                            await unitOfWork.Announces.UpdateAnnouncerAsync(new AnnounceAnnouncerUpdateViewModel { OriginalId = id, Announcer = 1 });

                                        }
                                        // unitOfWork.OwnerRepository.CreateAsync(new OwnerViewModel { Phone = number});


                                    }
                                    

                                    else
                                        {
                                            Console.WriteLine(result.Trim());
                                            Console.WriteLine("********************************************");
                                        }
                                    }
                                }
                            }
                        }
                    }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Data.ToString());
            }
        }
    }
}