using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WebApi.Models;
using WebApi.Repository;
using WebApi.Services.LalafoAz.Models;
using WebApi.ViewModels;

namespace WebApi.Services.LalafoAz
{
    public class LalafoAzParser
    {
        private HttpClient _httpClient;
        private readonly UnitOfWork unitOfWork;
        private readonly LalafoCountryNames countryNames;
        private readonly LalafoSettlementsName settlementsName;
        private readonly LalafoImageUploader imageUploader;
        private readonly EmlakBazaWithProxy bazaWithProxy;
        private readonly LalalafoAzMetrosNames metrosNames;
        private readonly PropertyTypeLalafo _propertyType;
        static string[] proxies = SingletonProxyServersIp.Instance;
        private readonly HttpClientCreater _clientCreater;
        public const int maxRequest = 20;
        private dynamic dynjson;
        int iteration = 0;


        public LalafoAzParser(HttpClientCreater httpClientCreator,
                              UnitOfWork unitOfWork,
                              LalafoCountryNames countryNames,
                              LalafoSettlementsName settlementsName,
                              LalafoImageUploader imageUploader,
                              EmlakBazaWithProxy bazaWithProxy,
                              LalalafoAzMetrosNames metrosNames,
                              PropertyTypeLalafo propertyType
                              //HttpClient httpClient
                              )
        {
            this.unitOfWork = unitOfWork;
            this.countryNames = countryNames;
            this.settlementsName = settlementsName;
            this.imageUploader = imageUploader;
            this.bazaWithProxy = bazaWithProxy;
            this.metrosNames = metrosNames;
            _propertyType = propertyType;
            //_httpClient = httpClient;
            _clientCreater = httpClientCreator;
            Random rnd = new Random();
            _httpClient = _clientCreater.Create(proxies[rnd.Next(0, 99)]);

        }
      
      



        public async Task LalafoAzPars()
        {
            var model = unitOfWork.ParserAnnounceRepository.GetBySiteName("https://lalafo.az");

            var mainUri = new Uri("https://lalafo.az/azerbaijan/nedvizhimost");
            var mainResponse = await _httpClient.GetAsync(mainUri);

            if (model.isActive && mainResponse.IsSuccessStatusCode)
            {
                try
                {
                    var html2 = await mainResponse.Content.ReadAsStringAsync();

                    HtmlDocument doc2 = new HtmlDocument();
                
                    doc2.LoadHtml(html2);
                    var script2 = doc2.DocumentNode.SelectSingleNode("//script[@id='__NEXT_DATA__']");
                    var json2 = JsonConvert.DeserializeObject<Root>(script2.InnerText);
                    var announces = json2.props.initialState.listing.listingFeed.items;

                  
                    int x = 0;
                    int count = 0;
                    int duration = 0;

                    while (true)
                    {
                        if (iteration >= announces.Count())
                        {
                            TelegramBotService.Sender("lalafo break");
                            break;
                        }

                        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(announces[iteration].created_time);
                        TimeZoneInfo bakuTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Baku");
                        DateTimeOffset bakuTime = TimeZoneInfo.ConvertTime(dateTimeOffset, bakuTimeZone);

                        if (DateTime.Now.Date == bakuTime.Date)
                        {
                            if (count >= 10)
                            {
                                x++;
                                if (x >= 100)
                                    x = 0;

                                _httpClient = _clientCreater.Create(proxies[x]);

                                count = 0;

                            }
                            var searchVM = new AnnounceSearchViewModel()
                            {
                                OriginalId = announces[iteration].id,
                                ParserSite = model.site
                            };

                            if (await unitOfWork.Announces.IsAnnounceValidAsync(searchVM))
                            {
                                x++;
                                iteration++;
                                continue;
                            }
                           
                            Uri myUri = new Uri($"{model.site}/baku/ads/{announces[iteration].id}", UriKind.Absolute);
                            

                            var response = await _httpClient.GetAsync(myUri);
                            var url = response.RequestMessage.RequestUri.AbsoluteUri;
                            HtmlDocument doc = new HtmlDocument();

                            try
                            {

                                if (response.IsSuccessStatusCode)
                                {
                                    var html = await response.Content.ReadAsStringAsync();

                                    if (!string.IsNullOrEmpty(html))
                                    {
                                        duration = 0;
                                        doc.LoadHtml(html);
                                        Announce announce = new Announce();
                                        var nav = doc.DocumentNode.SelectNodes(".//ul[@class='desktop css-h8ujnu']//li//a");
                                        if (nav != null)
                                        {
                                            if (nav[2].InnerText == "Daşınmaz əmlak" && !nav[4].InnerText.Contains("Günlük", StringComparison.OrdinalIgnoreCase))
                                            {

                                                if (doc.DocumentNode.SelectSingleNode(".//span[@class='userName-text']") != null)
                                                {



                                                    var script = doc.DocumentNode.SelectSingleNode("//script[@id='__NEXT_DATA__']");
                                                    dynjson = JsonConvert.DeserializeObject(script.InnerText);



                                                    var countries = countryNames.GetRegionsNamesAll();
                                                    var settlements = settlementsName.GetSettlementNamesAll();
                                                    var metros = metrosNames.GetMetroNameAll();

                                                    var id =  announces[iteration].id ;
                                                   
                                                    /// City search
                                                    foreach (var item in countries)
                                                    {
                                                        if (dynjson?.props?.initialState?.feed?.adDetails[id.ToString()]?.item?.city?.ToString() != null)
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

                                                    var address = dynjson.props.initialState.feed.adDetails[id.ToString()].item.city.ToString();
                                                    if (address != null)
                                                        announce.address = address;

                                                    bool checkedNumber = false;


                                                    if (mobile != null)
                                                    {
                                                        announce.mobile = mobile;

                                                        var checkNumberRieltorResult = await unitOfWork.CheckNumberRepository.CheckNumberForRieltorAsync(mobile.ToString());
                                                        if (checkNumberRieltorResult > 0)
                                                        {
                                                            announce.number_checked = true;
                                                            announce.announcer = checkNumberRieltorResult;
                                                            checkedNumber = true;
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
                                                        if (item.FirstChild.InnerText.StartsWith("Mərtəbələrin"))
                                                            announce.floor_count = Int32.Parse(item.LastChild.InnerText);
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


                                                    int announceType = _propertyType.GetTypeOfProperty(nav[4].InnerText);
                                                    announce.announce_type = announceType;
                                                    if (nav[3].InnerText == "Mənzillər" && announceType == 1 && announce.floor_count != 0)
                                                    {
                                                        if (announce.floor_count >= 12)
                                                        {
                                                            announce.property_type = 2;
                                                        }
                                                        else
                                                        {
                                                            announce.property_type = 1;
                                                        }
                                                    }
                                                    else if (nav[3].InnerText == "Mənzillər" && announceType == 1)
                                                    {
                                                        if (text.Contains("yeni tikili", StringComparison.OrdinalIgnoreCase)
                                                            || text.Contains("yaşayış kompleks", StringComparison.OrdinalIgnoreCase)
                                                            || text.Contains("yasayis kompleks", StringComparison.OrdinalIgnoreCase))
                                                        {
                                                            announce.property_type = 2;
                                                        }
                                                        else if (text.Contains("köhnə tikili", StringComparison.OrdinalIgnoreCase) ||
                                                                 text.Contains("kohne tikili", StringComparison.OrdinalIgnoreCase))
                                                        {
                                                            announce.property_type = 1;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        announce.property_type = _propertyType.GetTitleOfProperty(nav[3].InnerText);
                                                    }

                                                    ///////////////////////// ImageUploader //////////////////////////////
                                                    announce.cover = $@"LalafoAz/{DateTime.Now.Year}/{DateTime.Now.Month}/{id}/Thumb.jpeg";

                                                    var filePath = $@"LalafoAz/{DateTime.Now.Year}/{DateTime.Now.Month}/{id}/";
                                                    var images = await imageUploader.ImageDownloaderAsync(doc, id.ToString(), filePath);
                                                    if (images != null)
                                                        announce.logo_images = JsonConvert.SerializeObject(images);




                                                    if (images.Count > 0 && announce.city_id > 0)
                                                    {
                                                        var lastId = await unitOfWork.Announces.CreateAsync(announce);

                                                        if (checkedNumber == false)
                                                        {
                                                            //EMLAK - BAZASI

                                                            await bazaWithProxy.CheckAsync(lastId, mobile.ToString());
                                                        }
                                                        unitOfWork.Dispose();
                                                    }

                                                }
                                            }

                                        }
                                    }
                                }
                                else
                                {
                                    duration++;
                                    if (duration >= maxRequest)
                                    {
                                        TelegramBotService.Sender($"lalafo.az limited {maxRequest}");

                                        break;

                                    }
                                } // else end
                            }
                            catch (Exception e)
                            {
                                TelegramBotService.Sender($"lalafo - parser exception {e.Message}");
                            }
                           
                        }


                        iteration++;
                    }
                }
                catch (Exception e)
                {
                  
                    TelegramBotService.Sender($"lalafo - httpRequest - exception {e.Message}");

                }
               
            }


        }

     
    }
}

