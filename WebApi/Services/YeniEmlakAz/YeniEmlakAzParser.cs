using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WebApi.Models;
using WebApi.Repository;
using WebApi.Services.EmlakAz;

namespace WebApi.Services.YeniEmlakAz
{
    public class YeniEmlakAzParser
    {
        private HttpClient httpClient;
        private readonly EmlakBaza emlakBaza;
        private static bool isActive = false;
        private readonly UnitOfWork unitOfWork;
        private readonly HttpClientCreater clientCreater;
        HttpResponseMessage header;
        static string[] proxies; // лучше добавить ентер

        public YeniEmlakAzParser(EmlakBaza emlakBaza, UnitOfWork unitOfWork , HttpClientCreater clientCreater )
        {
            proxies = File.ReadAllLines("proxies.txt");
            httpClient = clientCreater.Create(proxies[0]);
            this.emlakBaza = emlakBaza;
            this.unitOfWork = unitOfWork;
            this.clientCreater = clientCreater;
        }

        public async Task YeniEmlakAzPars()
        {

            var model = unitOfWork.ParserAnnounceRepository.GetBySiteName("https://yeniemlak.az");
            var id = model.last_id;
            int counter = 0;


            if (!isActive)
            {
                if (model.isActive)
                {
                    int x = 0;
                    int count = 0;
                    while (true)
                    {
                        if (count >= 10)
                        {
                            x++;
                            if (x >= 350)
                                x = 0;

                            httpClient = clientCreater.Create(proxies[x]);
                            count = 0;
                        }

                        Announce announce = new Announce();
                        try
                        {

                            header = await httpClient.GetAsync($"{model.site}/elan/{++id}");
                            string url = header.RequestMessage.RequestUri.AbsoluteUri;
                            count++;
                            Console.WriteLine(id);
                            var response = await httpClient.GetAsync(url);
                            if (header.IsSuccessStatusCode)
                            {
                                var html = await response.Content.ReadAsStringAsync();
                                if (!string.IsNullOrEmpty(html))
                                {
                                    HtmlDocument doc = new HtmlDocument();
                                    doc.LoadHtml(html);

                                    announce.parser_site = model.site;
                                    announce.announce_date = DateTime.Now;
                                    announce.original_id = id;

                                    if (doc.DocumentNode.SelectSingleNode(".//div[@class='text']") != null)
                                    {
                                        announce.text = doc.DocumentNode.SelectSingleNode(".//div[@class='text']").InnerText;
                                    }
                                    if (doc.DocumentNode.SelectSingleNode(".//div[@class='title']//price") != null)
                                    {
                                        announce.price = Int32.Parse(doc.DocumentNode.SelectSingleNode(".//div[@class='title']//price").InnerText);
                                    }

                                    if (doc.DocumentNode.SelectNodes(".//div[@class='title']//titem") != null)
                                    {
                                        foreach (var item1 in doc.DocumentNode.SelectNodes(".//div[@class='title']//titem"))
                                        {

                                            if (item1.InnerText.StartsWith("Tarix: "))
                                            {
                                                announce.original_date = item1.LastChild.InnerText;
                                            }
                                            if (item1.InnerText.StartsWith("Baxış sayı: "))
                                            {
                                                announce.view_count = Int32.Parse(item1.LastChild.InnerText);
                                            }
                                        }
                                    }
                                    if (doc.DocumentNode.SelectNodes(".//div[@class='params']") != null)
                                    {
                                        foreach (var item in doc.DocumentNode.SelectNodes(".//div[@class='params']"))
                                        {
                                            if (item.InnerText.EndsWith(" otaq"))
                                            {
                                                announce.room_count = Int32.Parse(item.FirstChild.InnerText);
                                            }
                                            if (item.InnerText.EndsWith(" m2"))
                                            {
                                                announce.space = Int32.Parse(item.FirstChild.InnerText);
                                            }
                                        }
                                    }

                                    if (doc.DocumentNode.SelectNodes(".//div[@class='tel']") != null)
                                    {
                                        int counPhoneImages = doc.DocumentNode.SelectNodes(".//div[@class='tel']//img").Count;
                                        StringBuilder numbers = new StringBuilder();
                                        string delimiter = "";

                                        for (int i = 0; i < counPhoneImages; i++)
                                        {
                                            var numbersArr = doc.DocumentNode.SelectNodes(".//div[@class='tel']//img")[i].Attributes["src"].Value.Split('/');
                                            var index = numbersArr.Length;
                                            numbers.Append(delimiter);
                                            numbers.Append(numbersArr[index - 1]);
                                            delimiter = ",";
                                        }
                                        announce.mobile = numbers.ToString();
                                        Console.WriteLine(numbers.ToString());
                                        Console.WriteLine("----------------------------");
                                    }
                                    unitOfWork.Announces.Create(announce);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("No Connection");
                        }
                    }
                }

            }

        }
    }
}
