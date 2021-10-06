using HtmlAgilityPack;
using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebApi.Models;
using WebApi.Repository;
using WebApi.Services.EmlakAz.Interfaces;

namespace WebApi.Services.EmlakAz
{
    public class EmlakAzParser
    {
        private readonly HttpClient httpClient;
        private readonly EmlakBaza _emlakBaza;
        private readonly EmlakAzImageUploader _uploader;
        private readonly UnitOfWork unitOfWork;
        private readonly ITypeOfProperty typeOfProperty;
        HttpResponseMessage header;
        static bool turn = false;
        public EmlakAzParser(HttpClient httpClient, EmlakBaza emlakBaza, EmlakAzImageUploader uploader, UnitOfWork unitOfWork,ITypeOfProperty typeOfProperty)
        {
            this.httpClient = httpClient;
            this._emlakBaza = emlakBaza;
            _uploader = uploader;
            this.unitOfWork = unitOfWork;
            this.typeOfProperty = typeOfProperty;
        }

        public async Task EmlakAzPars()
        {
            var model = unitOfWork.ParserAnnounceRepository.GetBySiteName("https://emlak.az");

            if (!model.isActive)
            {
                turn = true;
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



                                        var checkNumberRieltorResult = unitOfWork.CheckNumberRepository.CheckNumberForRieltor(numberList);
                                        var checkNumberOwnerResult = unitOfWork.CheckNumberRepository.CheckNumberForOwner(numberList);
                                        if (checkNumberRieltorResult > 0)
                                        {
                                            announce.announcer = checkNumberRieltorResult;
                                        }
                                        else if (checkNumberOwnerResult > 0)
                                        {
                                            announce.announcer = checkNumberOwnerResult;
                                        }
                                        else
                                        {
                                            announce.announcer = 1;
                                            //EMLAK - BAZASI
                                            _emlakBaza.CheckAsync(httpClient, numberList);
                                        }

                                        Regex regex = new Regex("\\d+");
                                        if (doc.DocumentNode.SelectSingleNode(".//p[@class='pull-right']") != null)
                                        {
                                            string result = regex.Match(doc.DocumentNode.SelectSingleNode(".//p[@class='pull-right']").InnerText).ToString();
                                            announce.original_id = Int32.Parse(result);
                                        }

                                        if (doc.DocumentNode.SelectSingleNode(".//h1[@class='title']") != null)
                                            announce.rent_type = typeOfProperty.GetTitleOfProperty(doc.DocumentNode.SelectSingleNode(".//h1[@class='title']").InnerText);

                                        foreach (var item in doc.DocumentNode.SelectNodes(".//dl[@class='technical-characteristics']//dd"))
                                        {
                                            if (item != null)
                                            {
                                                if (item.InnerText.StartsWith("Əmlakın növü"))
                                                    announce.property_type = typeOfProperty.GetTypeOfProperty(item.InnerText);
                                                if (item.InnerText.StartsWith("Sahə"))
                                                    announce.space = regex.Match(item.InnerText).ToString();
                                                if (item.InnerText.StartsWith("Otaqların sayı"))
                                                    announce.floor_count = Int32.Parse(item.LastChild.InnerText);
                                                if (item.InnerText.StartsWith("Otaqların sayı"))
                                                    announce.room_count = Int32.Parse(item.LastChild.InnerText);
                                                if (item.InnerText.StartsWith("Yerləşdiyi mərtəbə"))
                                                    announce.current_floor = Int32.Parse(item.LastChild.InnerText);
                                                if (item.InnerText.StartsWith("Mərtəbə sayı"))
                                                    announce.floor_count = Int32.Parse(item.LastChild.InnerText);
                                                if (item.InnerText.StartsWith("Təmiri"))
                                                {
                                                    if (item.LastChild.InnerText == "Təmirsiz")
                                                        announce.repair = false;
                                                    else
                                                        announce.repair = true;
                                                }
                                                   
                                            }
                                        }

                                        //if (doc.DocumentNode.SelectNodes(".//dl[@class='technical-characteristics']//dd")[0] != null)

                                        //if (doc.DocumentNode.SelectNodes(".//dl[@class='technical-characteristics']//dd")[1] != null)
                                        //if (doc.DocumentNode.SelectNodes(".//dl[@class='technical-characteristics']//dd")[3] != null)
                                        //if (doc.DocumentNode.SelectNodes(".//dl[@class='technical-characteristics']//dd")[2] != null)




                                        if (doc.DocumentNode.SelectSingleNode(".//p[@class='name-seller']//span") != null)
                                        {
                                            if (doc.DocumentNode.SelectSingleNode(".//p[@class='name-seller']//span").InnerText.Trim() == "(Mülkiyyətçi)")
                                                announce.announcer = 1;
                                            if (doc.DocumentNode.SelectSingleNode(".//p[@class='name-seller']//span").InnerText.Trim() == "(Vasitəçi)")
                                                announce.announcer = 2;
                                        }

                                        Console.WriteLine(turn);

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
                                        var filePath = $@"EmlakAz/{DateTime.Now.Year}/{DateTime.Now.Month}/{id}/";
                                        var images = await _uploader.ImageDownloaderAsync(doc, id.ToString(), filePath);
                                        if (images != null)
                                        {
                                            
                                            var jsonImages = string.Join(',', images);
                                            if (jsonImages != null)
                                                announce.logo_images = jsonImages;
                                        }



                                            announce.cover = $@"EmlakAz/{DateTime.Now.Year}/{DateTime.Now.Month}/{id}/Thumb.jpg";
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
                            Console.WriteLine(turn);
                            counter++;
                            if (counter >= 50)
                            {
                                model.last_id = (id - 50);
                                model.isActive = false;
                                turn = false;
                                unitOfWork.ParserAnnounceRepository.Update(model);
                                counter = 0;
                                Console.WriteLine("= 50 =");
                                Console.WriteLine(turn);
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
