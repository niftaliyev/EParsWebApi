using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using WebApi.Models;
using WebApi.Repository;

namespace WebApi.Services.LalafoAz
{
    public class LalafoAzParser
    {
        private readonly HttpClient httpClient;
        private readonly UnitOfWork unitOfWork;
        private readonly LalafoCountryNames countryNames;
        private readonly LalafoSettlementsName settlementsName;
        private readonly LalafoImageUploader imageUploader;
        private readonly EmlakBazaWithProxy bazaWithProxy;
        private readonly LalalafoAzMetrosNames metrosNames;
        private static bool isActive = false;
        public const int maxRequest = 50;

        HttpResponseMessage header;
        public LalafoAzParser(HttpClient httpClient, UnitOfWork unitOfWork, 
            LalafoCountryNames countryNames, LalafoSettlementsName settlementsName,
            LalafoImageUploader imageUploader , EmlakBazaWithProxy bazaWithProxy,LalalafoAzMetrosNames metrosNames)
        {
            this.httpClient = httpClient;
            this.unitOfWork = unitOfWork;
            this.countryNames = countryNames;
            this.settlementsName = settlementsName;
            this.imageUploader = imageUploader;
            this.bazaWithProxy = bazaWithProxy;
            this.metrosNames = metrosNames;
        }
        public async Task LalafoAzPars()
        {
            var model = unitOfWork.ParserAnnounceRepository.GetBySiteName("https://lalafo.az");

            if (!isActive)
            {
                if (model.isActive)
                {

                    var id = model.last_id;

                    isActive = true;
                    int x = 0;
                    int counter = 0;
                    while (true)
                    {
                        try
                        {
                            Console.WriteLine(model.site);

                            Uri myUri = new Uri($"{model.site}/baku/ads/{++id}", UriKind.Absolute);
                            header = await httpClient.GetAsync(myUri);
                            var url = header.RequestMessage.RequestUri.AbsoluteUri;
                            HtmlDocument doc = new HtmlDocument();


                            var response = await httpClient.GetAsync(url);
                            if (response.IsSuccessStatusCode)
                            {
                                var html = await response.Content.ReadAsStringAsync();

                                if (!string.IsNullOrEmpty(html))
                                {
                                    counter = 0;
                                    doc.LoadHtml(html);
                                    Announce announce = new Announce();

                                    if (doc.DocumentNode.SelectNodes(".//ul[@class='desktop css-h8ujnu']//li//a") != null)
                                    {
                                        if (doc.DocumentNode.SelectNodes(".//ul[@class='desktop css-h8ujnu']//li//a")[2].InnerText == "Daşınmaz əmlak")
                                        {
                               

                                            if (doc.DocumentNode.SelectSingleNode(".//span[@class='userName-text']") != null)
                                            {

                                                var script = doc.DocumentNode.SelectSingleNode("//script[@id='__NEXT_DATA__']");
                                                dynamic dynjson = JsonConvert.DeserializeObject(script.InnerText);



                                                var countries = countryNames.GetRegionsNamesAll();
                                                var settlements = settlementsName.GetSettlementNamesAll();
                                                var metros = metrosNames.GetMetroNameAll();


                                                /// City search
                                                foreach (var item in countries)
                                                {
                                                    if (dynjson.props.initialState.feed.adDetails[id.ToString()].item.city.ToString() != null)
                                                    {
                                                        if (item.Key == dynjson.props.initialState.feed.adDetails[id.ToString()].item.city.ToString())
                                                        {
                                                            announce.city_id = item.Value;
                                                        }
                                                    }
                    
                                                }


                                                var price = dynjson.props.initialState.feed.adDetails[id.ToString()].item.price;
                                                if (price != null)
                                                    announce.price = price;
                                                var mobile = dynjson.props.initialState.feed.adDetails[id.ToString()].item.mobile;
                            
                                                var name = dynjson.props.initialState.feed.adDetails[id.ToString()].item.username;
                                                if (name != null)
                                                    announce.name = name;
                                                var text = dynjson.props.initialState.feed.adDetails[id.ToString()].item.description;
                                                if (text != null)
                                                    announce.text = text;

                                                bool checkedNumber = false;


                                                if (mobile != null)
                                                {
                                                    announce.mobile = mobile;

                                                    var checkNumberRieltorResult = unitOfWork.CheckNumberRepository.CheckNumberForRieltor(mobile.ToString());
                                                    if (checkNumberRieltorResult > 0)
                                                    {
                                                        announce.number_checked = true;
                                                        announce.announcer = checkNumberRieltorResult;
                                                        checkedNumber = true;
                                                    }
                                                    if (checkedNumber == false)
                                                    {
                                                        //EMLAK - BAZASI

                                                        await bazaWithProxy.CheckAsync(id, mobile.ToString());
                                                    }
                                                }
                                                announce.original_id = id;
                                                announce.announce_date = DateTime.Now;
                                                announce.original_date = doc.DocumentNode.SelectNodes(".//div[@class='about-ad-info__date']//span")[1].InnerText;
                                                announce.parser_site = model.site;

                                           

                                                /////// parametrler 
                                                foreach (var item in doc.DocumentNode.SelectNodes(".//li"))
                                                {
                                                    if (item.FirstChild.InnerText.StartsWith("Otaqların"))
                                                        announce.room_count = Int32.Parse(item.LastChild.InnerText.Split(" ")[0]);
                                                    if (item.FirstChild.InnerText.StartsWith("Sahə"))
                                                        announce.space = item.LastChild.InnerText;
                                                    if (item.FirstChild.InnerText.StartsWith("Mərtəbə"))
                                                        announce.current_floor = Int32.Parse(item.LastChild.InnerText);
                                                    if (item.FirstChild.InnerText.StartsWith("Rayon"))
                                                    {
                                                        foreach (var setlement in settlements)
                                                        {
                                                            if (setlement.Key == item.LastChild.InnerText)
                                                                announce.region_id = setlement.Value;
                                                        }
                                                    }
                                                    if (item.FirstChild.InnerText.StartsWith("Metro"))
                                                    {
                                                        foreach (var metro in metros)
                                                        {
                                                            if (metro.Key == item.LastChild.InnerText)
                                                                announce.metro_id = metro.Value;
                                                        }
                                                    }
                                                }




                                                /////////////////////////// ImageUploader //////////////////////////////
                                                announce.cover = $@"LalafoAz/{DateTime.Now.Year}/{DateTime.Now.Month}/{id}/Thumb.jpg";

                                                var filePath = $@"LalafoAz/{DateTime.Now.Year}/{DateTime.Now.Month}/{id}/";
                                                var images = await imageUploader.ImageDownloaderAsync(doc, id.ToString(), filePath);
                                                if (images != null)
                                                    announce.logo_images = JsonConvert.SerializeObject(images);


                                                Console.WriteLine(announce.logo_images);
                                                Console.WriteLine($"City {announce.city_id}");
                                                Console.WriteLine($"Region {announce.region_id}");

                                                Console.WriteLine(doc.DocumentNode.SelectNodes(".//ul[@class='desktop css-h8ujnu']//li//a")[2].InnerText);

                                                if (images.Count > 0 && announce.city_id > 0)
                                                {
                                                    await unitOfWork.Announces.Create(announce);
                                                    unitOfWork.Dispose();
                                                }

                                            }
                                        }

                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine("404");
                                Console.WriteLine(id);
                                counter++;
                                Console.WriteLine(counter);
                                if (counter >= maxRequest)
                                {
                                    model.last_id = (id - maxRequest);
                                    isActive = false;
                                    unitOfWork.ParserAnnounceRepository.Update(model);
                                    counter = 0;
                                    Console.WriteLine($"= {maxRequest} = ");
                                    TelegramBotService.Sender($"emlak.az limited {maxRequest}");

                                    break;

                                }
                            } // else end
                        }
                        catch (Exception e)
                        {

                            Console.WriteLine(e.Message);
                            TelegramBotService.Sender($"exception {e.Message}");

                        }
                    }
                }

            }
        }

    }
}

