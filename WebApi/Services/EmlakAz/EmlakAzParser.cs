using HtmlAgilityPack;
using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebApi.Models;
using WebApi.Repository;
using IronOcr;

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

            if (!model.isActive)
            {
                var id = model.last_id;
                int counter = 0;
                model.isActive = true;
                unitOfWork.ParserAnnounceRepository.Update(model);
                
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

                                    if (doc.DocumentNode.SelectSingleNode(".//p[@class='phone']") != null)
                                    {
                                        string mobileregex = doc.DocumentNode.SelectSingleNode(".//p[@class='phone']").InnerText;



                                        var charsToRemove = new string[] { "(", ")", "-", ".", " " };
                                        foreach (var c in charsToRemove)
                                        {
                                            mobileregex = mobileregex.Replace(c, string.Empty);
                                        }

                                        var numberList = mobileregex.Split(',');

                                        if (mobileregex != null)
                                            announce.mobile = mobileregex;


                                        //EMLAK - BAZASI
                                        //_emlakBaza.CheckAsync(numberList);

                                        Regex regex = new Regex("\\d+");
                                        if (doc.DocumentNode.SelectSingleNode(".//p[@class='pull-right']") != null)
                                        {
                                            string result = regex.Match(doc.DocumentNode.SelectSingleNode(".//p[@class='pull-right']").InnerText).ToString();
                                            announce.original_id = Int32.Parse(result);
                                        }

                                        if (doc.DocumentNode.SelectSingleNode(".//h1[@class='title']") != null)
                                        {
                                            if (doc.DocumentNode.SelectSingleNode(".//h1[@class='title']").InnerText.StartsWith("İcarəyə verilir"))
                                                announce.rent_type = 1;
                                            if (doc.DocumentNode.SelectSingleNode(".//h1[@class='title']").InnerText.StartsWith("Satılır"))
                                                announce.rent_type = 2;
                                            if (doc.DocumentNode.SelectSingleNode(".//h1[@class='title']").InnerText.Trim().StartsWith("Satılır"))
                                                announce.announce_type = 1;
                                            if (doc.DocumentNode.SelectSingleNode(".//h1[@class='title']").InnerText.Trim().StartsWith("İcarəyə verilir"))
                                                announce.announce_type = 2;

                                            Console.WriteLine(doc.DocumentNode.SelectSingleNode(".//h1[@class='title']").InnerText);

                                        }



                                        if (doc.DocumentNode.SelectSingleNode(".//p[@class='name-seller']//span") != null)
                                        {
                                            if (doc.DocumentNode.SelectSingleNode(".//p[@class='name-seller']//span").InnerText.Trim() == "(Mülkiyyətçi)")
                                                announce.announcer = 1;
                                            if (doc.DocumentNode.SelectSingleNode(".//p[@class='name-seller']//span").InnerText.Trim() == "(Vasitəçi)")
                                                announce.announcer = 2;
                                        }

                                        if (doc.DocumentNode.SelectNodes(".//dl[@class='technical-characteristics']//dd")[0] != null)
                                        {
                                            if (doc.DocumentNode.SelectNodes(".//dl[@class='technical-characteristics']//dd")[0].InnerText.EndsWith("Yeni tikili"))
                                                announce.property_type = 1;
                                            if (doc.DocumentNode.SelectNodes(".//dl[@class='technical-characteristics']//dd")[0].InnerText.EndsWith("Köhnə tikili"))
                                                announce.property_type = 2;
                                            if (doc.DocumentNode.SelectNodes(".//dl[@class='technical-characteristics']//dd")[0].InnerText.EndsWith("Torpaq"))
                                                announce.property_type = 3;
                                            if (doc.DocumentNode.SelectNodes(".//dl[@class='technical-characteristics']//dd")[0].InnerText.EndsWith("Həyət evi"))
                                                announce.property_type = 4;
                                        }


                                        if (doc.DocumentNode.SelectSingleNode(".//span[@class='m']") != null)
                                            announce.price = Int32.Parse(doc.DocumentNode.SelectSingleNode(".//span[@class='m']").InnerText.Replace(" ", string.Empty)); 

                                        if (doc.DocumentNode.SelectSingleNode(".//div[@class='desc']//p") != null)
                                            announce.text = doc.DocumentNode.SelectSingleNode(".//div[@class='desc']//p").InnerText.Replace(" \n\n", string.Empty);

                                        if (doc.DocumentNode.SelectNodes(".//dl[@class='technical-characteristics']//dd")[2] != null)
                                            announce.room_count = Int32.Parse(regex.Match(doc.DocumentNode.SelectNodes(".//dl[@class='technical-characteristics']//dd")[2].InnerText).ToString()); 

                                        if(doc.DocumentNode.SelectSingleNode(".//span[@class='views-count']//strong") != null)
                                            announce.view_count = Int32.Parse(doc.DocumentNode.SelectSingleNode(".//span[@class='views-count']//strong").InnerText);

                                        if(doc.DocumentNode.SelectSingleNode(".//span[@class='date']//strong") != null)
                                            announce.original_date = doc.DocumentNode.SelectSingleNode(".//span[@class='date']//strong").InnerText;

                                        var cities = unitOfWork.CitiesRepository.GetAll();
                                            foreach (var item in cities)
                                            {
                                                if ("Füzuli" == item.name)
                                                {
                                                    announce.region_id = item.id;

                                                }
                                            }
                                            announce.parser_site = model.site;
                                            /////////////////////////// ImageUploader //////////////////////////////
                                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), $@"wwwroot\UploadFile\EmlakAz\{DateTime.Now.Year}\{DateTime.Now.Month}\{id}");
                                        var images = _uploader.ImageDownloaderAsync(doc, id.ToString(), filePath);
                                        if (images != null)
                                        {
                                            var jsonImages = JsonSerializer.Serialize(await images);
                                            if (jsonImages != null)
                                                announce.logo_images = jsonImages;
                                        }



                                            announce.cover = $@"\UploadFile\EmlakAz\{DateTime.Now.Year}\{DateTime.Now.Month}\{id}\Thumb.jpg";
                                            announce.announce_date = DateTime.Now;
                                            unitOfWork.Announces.Create(announce);
                                            counter = 0;
                                    }
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
                                model.last_id = (id - 50);
                                model.isActive = false;
                                unitOfWork.ParserAnnounceRepository.Update(model);
                                counter = 0;
                                Console.WriteLine("= 50 =");
                                break;

                            }
                        } // else end
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                } // while end
            }      // if isactive      

        } // Method end

    } // class end
}
