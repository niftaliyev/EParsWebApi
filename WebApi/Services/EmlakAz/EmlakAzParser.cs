using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebApi.Models;
using WebApi.Repository;

namespace WebApi.Services.EmlakAz
{
    public class EmlakAzParser
    {
        private readonly HttpClient httpClient;
        private readonly EmlakBaza _emlakBaza;
        private readonly EmlakAzImageUploader _uploader;
        private readonly UnitOfWork unitOfWork;
        HttpResponseMessage header;

        public EmlakAzParser(HttpClient httpClient, EmlakBaza emlakBaza, EmlakAzImageUploader uploader, UnitOfWork unitOfWork)
        {
            this.httpClient = httpClient;
            this._emlakBaza = emlakBaza;
            _uploader = uploader;
            this.unitOfWork = unitOfWork;
        }

        public async Task EmlakAzPars()
        {
            var model = unitOfWork.ParserAnnounceRepository.GetBySiteName("https://emlak.az");
            var id = model.last_id;
            int counter = 0;


            while (true)
            {
                Announce announce = new Announce();
                try
                {

                    header = await httpClient.GetAsync($"{model.site}/{++id}-.html");
                    string url = header.RequestMessage.RequestUri.AbsoluteUri;
                    if (!url.EndsWith("/site"))
                    {

                        var response = await httpClient.GetAsync(url);
                        if (response.IsSuccessStatusCode)
                        {
                            var html = await response.Content.ReadAsStringAsync();
                            if (!string.IsNullOrEmpty(html))
                            {
                                HtmlDocument doc = new HtmlDocument();
                                doc.LoadHtml(html);



                                ////////////////////////////////////////////////////////////////////////////

                                if (doc.DocumentNode.SelectSingleNode(".//h1[@class='title']") != null)
                                {
                                    Console.WriteLine(doc.DocumentNode.SelectSingleNode(".//h1[@class='title']").InnerText);

                                    Regex regex = new Regex("\\d+");
                                    if (doc.DocumentNode.SelectSingleNode(".//p[@class='pull-right']") != null)
                                    {
                                        string result = regex
                                            .Match(doc.DocumentNode.SelectSingleNode(".//p[@class='pull-right']").InnerText).ToString();
                                        if (doc.DocumentNode.SelectSingleNode(".//p[@class='phone']") != null)
                                        {
                                            string mobileregex = doc.DocumentNode.SelectSingleNode(".//p[@class='phone']").InnerText;
                                            var charsToRemove = new string[] { "(", ")", "-", ".", " " };
                                            foreach (var c in charsToRemove)
                                            {
                                                mobileregex = mobileregex.Replace(c, string.Empty);
                                            }
                                            var numberList = mobileregex.Split(',');

                                            ////EMLAK - BAZASI
                                            //_emlakBaza.CheckAsync(numberList);

                                            Console.WriteLine(mobileregex);

                                            if (doc.DocumentNode.SelectSingleNode(".//h1[@class='title']").InnerText != null)
                                            {

                                                if (doc.DocumentNode.SelectSingleNode(".//h1[@class='title']").InnerText.StartsWith("İcarəyə verilir"))
                                                    announce.rent_type = 1;
                                                if (doc.DocumentNode.SelectSingleNode(".//h1[@class='title']").InnerText.StartsWith("Satılır"))
                                                    announce.rent_type = 2;
                                                if (doc.DocumentNode.SelectSingleNode(".//p[@class='name-seller']//span").InnerText.Trim() == "(Mülkiyyətçi)")
                                                    announce.announcer = 1;
                                                if (doc.DocumentNode.SelectSingleNode(".//p[@class='name-seller']//span").InnerText.Trim() == "(Vasitəçi)")
                                                    announce.announcer = 2;
                                                if (doc.DocumentNode.SelectSingleNode(".//h1[@class='title']").InnerText.Trim().StartsWith("Satılır"))
                                                    announce.announce_type = 1;
                                                if (doc.DocumentNode.SelectSingleNode(".//h1[@class='title']").InnerText.Trim().StartsWith("İcarəyə verilir")) announce.announce_type = 2;

                                                if (doc.DocumentNode.SelectNodes(".//dl[@class='technical-characteristics']//dd")[0].InnerText.EndsWith("Yeni tikili"))
                                                    announce.property_type = 1;
                                                if (doc.DocumentNode.SelectNodes(".//dl[@class='technical-characteristics']//dd")[0].InnerText.EndsWith("Köhnə tikili"))
                                                    announce.property_type = 2;
                                                if (doc.DocumentNode.SelectNodes(".//dl[@class='technical-characteristics']//dd")[0].InnerText.EndsWith("Torpaq"))
                                                    announce.property_type = 3;
                                                if (doc.DocumentNode.SelectNodes(".//dl[@class='technical-characteristics']//dd")[0].InnerText.EndsWith("Həyət evi"))
                                                    announce.property_type = 4;

                                                if (doc.DocumentNode.SelectSingleNode(".//span[@class='m']").InnerText != null)
                                                {
                                                    announce.price = doc.DocumentNode.SelectSingleNode(".//span[@class='m']").InnerText;
                                                    if (doc.DocumentNode.SelectSingleNode(".//div[@class='desc']//p")
                                                        .InnerText != null)
                                                    {
                                                        announce.text = doc.DocumentNode.SelectSingleNode(".//div[@class='desc']//p").InnerText;
                                                        announce.mobile = mobileregex;
                                                        announce.original_id = Int32.Parse(result);
                                                        announce.cover = "1";
                                                        announce.images = $@"\UploadFile\EmlakAz\{DateTime.Now.Year}\{DateTime.Now.Month}\{id}";
                                                        announce.room_count = regex.Match(doc.DocumentNode.SelectNodes(".//dl[@class='technical-characteristics']//dd")[2].InnerText).ToString();
                                                        announce.cities_regions_id = 2;
                                                        announce.view_count = doc.DocumentNode.SelectSingleNode(".//span[@class='views-count']//strong").InnerText;
                                                        announce.announce_date = DateTime.Now.ToShortDateString();
                                                        announce.original_date = doc.DocumentNode.SelectSingleNode(".//span[@class='date']//strong").InnerText;
                                                        announce.parser_announce = model.site;
                                                        /////////////////////////////// ImageUploader //////////////////////////////
                                                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), $@"wwwroot\UploadFile\EmlakAz\{DateTime.Now.Year}\{DateTime.Now.Month}\{id}");
                                                        _uploader.ImageDownloader(doc, id.ToString(), filePath,announce);
                                                        List<string> images = new List<string>();
                                                        string[] arr = new string[4];
                                                        arr[0] = "/UploadFile/EmlakAz/2021/9/741404/Thumb.jpg";
                                                        arr[1] = "/UploadFile/EmlakAz/2021/9/741404/Thumb.jpg";
                                                        arr[2] = "/UploadFile/EmlakAz/2021/9/741404/Thumb.jpg";
                                                        arr[3] = "/UploadFile/EmlakAz/2021/9/741404/Thumb.jpg";
                                                        images.Add("/UploadFile/EmlakAz/2021/9/741404/Thumb.jpg");
                                                        images.Add("/UploadFile/EmlakAz/2021/9/741404/Thumb.jpg");
                                                        images.Add("/UploadFile/EmlakAz/2021/9/741404/Thumb.jpg");
                                                        images.Add("/UploadFile/EmlakAz/2021/9/741404/Thumb.jpg");

                                                        announce.logo_images = JsonSerializer.Serialize(arr);
                                                        unitOfWork.Announces.Create(announce);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                counter = 0;
                            }
                        }
                    } //if end
                    else
                    {
                        Console.WriteLine("404");
                        Console.WriteLine(id);
                        counter++;
                        if (counter >= 50)
                        {

                            counter = 0;
                            Console.WriteLine("= 50 =");
                            break;

                        }
                    } // else end
                }
                catch (Exception e)
                {
                    Console.WriteLine("No Connection");
                }
            } // while end

        } // Method end

    } // class end
}
