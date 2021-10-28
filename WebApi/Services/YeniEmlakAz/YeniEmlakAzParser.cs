using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
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
        private readonly YeniEmlakAzParserImageUploader uploader;
        public int maxRequest = 50;
        HttpResponseMessage header;
        static string[] proxies = SingletonProxyServersIp.Instance;

        public YeniEmlakAzParser(EmlakBaza emlakBaza, UnitOfWork unitOfWork , HttpClientCreater clientCreater , YeniEmlakAzParserImageUploader uploader)
        {
            httpClient = clientCreater.Create(proxies[0]);
            this.emlakBaza = emlakBaza;
            this.unitOfWork = unitOfWork;
            this.clientCreater = clientCreater;
            this.uploader = uploader;
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
                    int duration = 0;

                    while (true)
                    {
                        if (count >= 5)
                        {
                            x++;
                            if (x >= 350)
                                x = 0;

                            httpClient = clientCreater.Create(proxies[x]);
                            //Random random = new Random();
                            //httpClient = clientCreater.Create(proxies[random.Next(1 ,350)]);
                            //count = 0;
                        }

                        Announce announce = new Announce();
                        try
                        {
                            Console.WriteLine(model.site);

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

                                   

                                    if (doc.DocumentNode.SelectSingleNode(".//div[@class='text']") != null && doc.DocumentNode.SelectSingleNode(".//table[@class='msg']//h3") == null)
                                    {
                                        announce.text = doc.DocumentNode.SelectSingleNode(".//div[@class='text']").InnerText;
                                        announce.parser_site = model.site;
                                        announce.announce_date = DateTime.Now;
                                        announce.original_id = id;
                                        duration = 0;
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

                                    StringBuilder numbers = new StringBuilder();
                                    if (doc.DocumentNode.SelectNodes(".//div[@class='tel']") != null)
                                    {
                                        int counPhoneImages = doc.DocumentNode.SelectNodes(".//div[@class='tel']//img").Count;
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
                                            ///////////////////IMAGE DOWNLOAD /////////////////////////
                                        announce.cover = $@"YeniemlakAz/{DateTime.Now.Year}/{DateTime.Now.Month}/{id}/Thumb.jpg";

                                        var filePath = $@"YeniemlakAz/{DateTime.Now.Year}/{DateTime.Now.Month}/{id}/";
                                        var images = uploader.ImageDownloader(doc, id.ToString(), filePath, httpClient);
                                        announce.logo_images = JsonSerializer.Serialize(await images);

                                        bool checkedNumber = false;
                                        var numberList = numbers.ToString().Split(',');
                                        var checkNumberRieltorResult = unitOfWork.CheckNumberRepository.CheckNumberForRieltor(numberList);
                                        if (checkNumberRieltorResult > 0)
                                        {
                                            Console.WriteLine("FIND IN RIELTOR BASE YENIEMLAK");
                                            announce.announcer = checkNumberRieltorResult;
                                            announce.number_checked = true;
                                            checkedNumber = true;
                                            Console.WriteLine("Checked YENIEMLAK");
                                        }
                                        unitOfWork.Announces.Create(announce);

                                        if (checkedNumber == false)
                                        {
                                            Console.WriteLine("Find in emlak-baza YENIEMLAK");
                                            //EMLAK - BAZASI
                                            await emlakBaza.CheckAsync(httpClient, id, numberList);
                                            Console.WriteLine("emlakbazaemlakbazaemlakbaza");
                                        }
                                    } //end if for text
                                    else
                                    {
                                        duration++;
                                        Console.WriteLine(duration);
                                    }
                                    if (duration >= maxRequest)
                                    {
                                        announce.announcer = 1;

                                        Console.WriteLine("END***********");
                                        break;
                                    }
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
