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
using WebApi.Services.ArendaAz.Interfaces;

namespace WebApi.Services.ArendaAz
{
    public class ArendaAzParser
    {
        private HttpClient _httpClient;
        private readonly UnitOfWork _unitOfWork;
        HttpResponseMessage header;
        private static bool isActive = false;
        private readonly ArendaAzSettlementNames _settlementNames;
        private readonly ArendaAzMetroNames _metroNames;
        private readonly ITypeOfPropertyArendaAz _propertyType;
        private readonly ArendaAzEmlakBazaWithProxy _emlakBaza;
        private readonly ArendaAzImageUploader _imageUploader;
        private readonly HttpClientCreater clientCreater;

        static string[] proxies = SingletonProxyServersIp.Instance;
        public ArendaAzParser(HttpClient httpClient, UnitOfWork unitOfWork, ArendaAzSettlementNames settlementNames
                               , ArendaAzMetroNames metroNames, ITypeOfPropertyArendaAz propertyType, ArendaAzEmlakBazaWithProxy emlakBaza
                                , ArendaAzImageUploader imageUploader,
                                    HttpClientCreater clientCreater)
        {
            _httpClient = httpClient;
            _unitOfWork = unitOfWork;
            _settlementNames = settlementNames;
            _metroNames = metroNames;
            _propertyType = propertyType;
            _emlakBaza = emlakBaza;
            _imageUploader = imageUploader;
        }

        public async Task ArendaAzPars()
        {
            if (!isActive)
            {
                int x = 0;
                isActive = true;


                var uri = new Uri("https://arenda.az/filtirli-axtaris/?home_search=1&lang=1&site=1&home_s=1&elan_novu%5B1%5D=1&elan_novu%5B2%5D=2&elan_novu%5B3%5D=3&emlak_novu%5B370%5D=370&emlak_novu%5B371%5D=371&emlak_novu%5B5%5D=5&emlak_novu%5B861%5D=861&emlak_novu%5B94%5D=94&emlak_novu%5B8%5D=8&emlak_novu%5B270%5D=270&emlak_novu%5B854%5D=854&price_min=&price_max=&otaq_min=0&otaq_max=0&sahe_min=&sahe_max=&mertebe_min=0&mertebe_max=0&y_mertebe_min=0&y_mertebe_max=0&axtar=&order=2");

                header = await _httpClient.GetAsync(uri);
                var url = header.RequestMessage.RequestUri.AbsoluteUri;
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var html = await response.Content.ReadAsStringAsync();

                    HtmlDocument doc = new HtmlDocument();

                    doc.LoadHtml(html);


                    var announceLinks = doc.DocumentNode.SelectNodes(".//ul[@class='a_netice full elan_list results_list_']//li//a");
                    var announceDates = doc.DocumentNode.SelectNodes(".//ul[@class='a_netice full elan_list results_list_']//li//a//span[@class='elan_box_date']");
                    
                    var model = _unitOfWork.InfoSiteRepository.GetBySiteName("https://arenda.az");



                    bool isFirstAnnounce = true;
                    int counter = 0;
                    int count = 0;
                    int y = 0;
                    while (true)
                    {

                        if (count >= 10)
                        {
                            y++;
                            if (y >= 350)
                                y = 0;

                            _httpClient = clientCreater.Create(proxies[y]);
                            count = 0;
                        }

                        try
                        {
                            if (announceDates[x++].InnerText.Contains("Bugün"))
                            {
                                var req = await _httpClient.GetAsync(announceLinks[x].Attributes["href"].Value);
                                if (req.IsSuccessStatusCode)
                                {


                                    HtmlDocument doc2 = new HtmlDocument();
                                    var html2 = await req.Content.ReadAsStringAsync();
                                    doc2.LoadHtml(html2);

                                    int announceCode = Int32.Parse(doc2.DocumentNode.SelectSingleNode(".//div[@class='elan_e_kodu']//p[1]//strong").InnerText);
                                   
                                    // if announce is valid in DB , skip it 
                                    bool isAnnounceValid = await _unitOfWork.Announces.IsAnnounceValid(announceCode);
                                    if (isAnnounceValid)
                                    {
                                        y++;
                                        if (y >= 5)
                                        {
                                            isActive = false;
                                            break;
                                        }
                                        continue;
                                    }
                                    else
                                    {
                                        y = 0;
                                    }

                                    if (model.last_index == announceCode)
                                    {
                                        break;
                                    }
                                    if (isFirstAnnounce)
                                    {

                                        isFirstAnnounce = false;
                                        model.last_index = announceCode;
                                        _unitOfWork.InfoSiteRepository.Update(model);
                                    }


                                    Announce announce = new Announce();
                                    announce.original_id = announceCode;
                                    announce.address = doc2.DocumentNode.SelectSingleNode(".//span[@class='elan_unvan_txt']").InnerText;

                                    announce.parser_site = model.parser_site;
                                    var text = doc2.DocumentNode.SelectSingleNode(".//div[@class='full elan_info_txt']").InnerText;

                                    announce.text = text;


                                    var price = doc2.DocumentNode.SelectSingleNode(".//div[@class='full elan_new_price_box text-center']//p/text()").InnerText.Trim();

                                    announce.price = Int32.Parse(price.Replace(" ", ""));

                                    var locationList = doc2.DocumentNode.SelectNodes(".//ul[@class='elan_adr_list full']//li//a");

                                    for (int i = 0; i < locationList.Count; i++)
                                    {
                                        var cities = _unitOfWork.CitiesRepository.GetAll();
                                        var regions = _unitOfWork.CitiesRepository.GetAllRegions();
                                        var settlements = _settlementNames.GetSettlementsNamesAll();
                                        var metros = _metroNames.GetMetroNameAll();


                                        foreach (var city in cities)
                                        {
                                            if (locationList[i].InnerText.Trim() == city.name)
                                            {
                                                announce.city_id = city.id;
                                                break;
                                            }
                                        }

                                        foreach (var region in regions)
                                        {
                                            if (locationList[i].InnerText.Trim().ToLower() == region.name.ToLower())
                                            {
                                                announce.region_id = region.id;
                                                break;
                                            }
                                        }

                                        foreach (var settlement in settlements)
                                        {
                                            if (settlement.Key.ToLower().Contains(locationList[i].InnerText.Trim().ToLower()))
                                            {
                                                announce.settlement_id = settlement.Value;
                                                break;
                                            }
                                        }

                                        foreach (var metro in metros)
                                        {
                                            if (metro.Key.ToLower().Contains(locationList[i].InnerText.Trim().Replace("metro ", "").ToLower()))
                                            {
                                                announce.metro_id = metro.Value;
                                                break;
                                            }
                                        }
                                    }

                                    // Kirayə Satılır
                                    var announce_type = doc2.DocumentNode.SelectSingleNode(".//h1[@class='elan_title']").InnerText;

                                    var type = _propertyType.GetTypeOfProperty(announce_type);

                                    announce.announce_type = type;

                                    // Yeni tikili ve s ....

                                    var property_type = doc2.DocumentNode.SelectSingleNode(".//h2[@class='full elan_in_title_link elan_main_title']//text()").InnerText.Replace(" / ", "");

                                    announce.property_type = _propertyType.GetTitleOfProperty(property_type);

                                    var allPropertiesValues = doc2.DocumentNode.SelectNodes(".//ul[@class='full elan_property_list']//li//a");

                                    for (int i = 0; i < allPropertiesValues.Count; i++)
                                    {
                                        if (allPropertiesValues[i].InnerText.Contains("otaq"))
                                        {
                                            var room_count = allPropertiesValues[i].InnerText.Trim().Split(" ")[0];
                                            announce.room_count = Int32.Parse(room_count);
                                        }
                                        else if (allPropertiesValues[i].InnerText.Contains("m2"))
                                        {
                                            var space = allPropertiesValues[i].InnerText.Trim().Split(" ")[0];
                                            announce.space = space;
                                        }
                                        else if (allPropertiesValues[i].InnerText.Contains("mərtəbə"))
                                        {
                                            if (allPropertiesValues[i].InnerText.Contains(" / "))
                                            {
                                                var floors = allPropertiesValues[i].InnerText.Trim().Replace(" mərtəbə", "");
                                                var floor_count = floors.Split(" / ")[1];
                                                var current_floor = floors.Split(" / ")[0];
                                                announce.floor_count = Int32.Parse(floor_count);
                                                announce.current_floor = Int32.Parse(current_floor);
                                            }
                                            else if (allPropertiesValues[i].InnerText.Contains("mərtəbəli"))
                                            {
                                                var floors = allPropertiesValues[i].InnerText.Trim().Replace(" mərtəbəli", "");
                                                announce.floor_count = Int32.Parse(floors);
                                            }

                                        }

                                        else if (allPropertiesValues[i].InnerText.Contains("Kupça"))
                                        {
                                            announce.document = 1;
                                        }

                                    }


                                    var name = doc2.DocumentNode.SelectSingleNode(".//div[@class='new_elan_user_info full']//p[1]").InnerText.Split(" (")[0];
                                    var announcer = doc2.DocumentNode.SelectSingleNode(".//div[@class='new_elan_user_info full']//p[1]").InnerText.Split(" (")[1].Replace(")", "");

                                    announce.name = name;

                                    if (announcer == "Əmlak sahibi")
                                    {
                                        announce.announcer = 1;
                                    }
                                    if (announcer == "Vasitəçi")
                                    {
                                        announce.announcer = 2;
                                    }

                                    var mobile = doc2.DocumentNode.SelectNodes(".//a[@class='elan_in_tel']");
                                    StringBuilder numbers = new StringBuilder();
                                    for (int i = 0; i < mobile.Count; i++)
                                    {
                                        var number = mobile[i].InnerText;
                                        var charsToRemove = new string[] { " ", "(", ")", "-" };
                                        foreach (var item in charsToRemove)
                                        {
                                            number = number.Replace(item, "");

                                        }

                                        if (i < mobile.Count - 1)
                                            numbers.Append($"{number},");
                                        else
                                            numbers.Append($"{number}");
                                    }

                                    announce.mobile = numbers.ToString();


                                    bool checkedNumber = false;
                                    var numberList = numbers.ToString().Split(',');
                                    var checkNumberRieltorResult = _unitOfWork.CheckNumberRepository.CheckNumberForRieltor(numberList);
                                    if (checkNumberRieltorResult > 0)
                                    {

                                        announce.announcer = checkNumberRieltorResult;
                                        announce.number_checked = true;
                                        checkedNumber = true;

                                    }
                                    /////IMAGES
                                    var filePath = $@"ArendaAz/{DateTime.Now.Year}/{DateTime.Now.Month}/{announceCode}/";
                                    var images = _imageUploader.ImageDownloader(doc2, announceCode.ToString(), filePath, _httpClient);


                                    var thumbLink = doc2.DocumentNode.SelectSingleNode(".//ul[@class='slides elan_g_images_list full']//li[1]//a[1]//img[1]").Attributes["src"].Value;
                                    var imageUri = new Uri(thumbLink);

                                    var uriWithoutQuery = imageUri.GetLeftPart(UriPartial.Path);
                                    var fileExtension = Path.GetExtension(uriWithoutQuery);
                                    announce.cover = $@"ArendaAz/{DateTime.Now.Year}/{DateTime.Now.Month}/{announceCode}/Thumb{fileExtension}";
                                    announce.logo_images = JsonSerializer.Serialize(await images);
                                    /////

                                    announce.announce_date = DateTime.Now;
                                    //*[@id = "elan_desc"] / section[2] / div[3] / div[1] / p[1]v

                                    var date = doc2.DocumentNode.SelectSingleNode(".//div[@class='elan_date_box_rside']//p[1]//text()").InnerText;
                                    announce.original_date = date.Replace("Elanın tarixi: ", "");

                                    var lastId = await _unitOfWork.Announces.Create(announce);
                                    _unitOfWork.Dispose();

                                    if (checkedNumber == false)
                                    {

                                        //EMLAK - BAZASI
                                        await _emlakBaza.CheckAsync(lastId, numberList);
                                    }






                                }

                            }


                            //if (counter >= maxRequest)
                            //{


                            //    //TelegramBotService.Sender("unvan.az limited");

                            //    isActive = false;
                            //    _unitOfWork.InfoSiteRepository.Update(model);
                            //    counter = 0;
                            //    break;
                            //}
                        }
                        catch (Exception e)
                        {
                            TelegramBotService.Sender($"Error : {e.Message}");
                            isActive = false;
                            break;
                        }

                    }



                }
                else
                {
                    TelegramBotService.Sender("no access to arenda.az");
                }

            }



        }



    }
}