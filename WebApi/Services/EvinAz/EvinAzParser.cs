using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using WebApi.Models;
using WebApi.Repository;
using WebApi.Services.EvinAz.Interfaces;
using WebApi.ViewModels;

namespace WebApi.Services.EvinAz
{
    public class EvinAzParser
    {
        private HttpClient _httpClient;
        private readonly EmlakBazaWithProxy _emlakBaza;
        HttpResponseMessage header;
        private readonly UnitOfWork _unitOfWork;
        private readonly ITypeOfPropertyEvinAz _propertyType;
        private readonly EvinAzMetroNames _metrosNames;
        private readonly EvinAzSettlementNames _settlementNames;
        private readonly EvinAzImageUploader _imageUploader;
        static string[] proxies = SingletonProxyServersIp.Instance;
        private readonly HttpClientCreater clientCreater;
        private static bool isActive = false;
        public const int maxRequest = 20;

        public EvinAzParser(HttpClient httpClient,
                                UnitOfWork unitOfWork,
                                EmlakBazaWithProxy emlakBaza,
                                ITypeOfPropertyEvinAz propertyType,
                                EvinAzSettlementNames settlementNames,
                                EvinAzMetroNames metrosNames,
                                EvinAzImageUploader imageUploader,
                                HttpClientCreater httpClientCreater
                               )
        {
            _httpClient = httpClient;
            _unitOfWork = unitOfWork;
            _emlakBaza = emlakBaza;
            _propertyType = propertyType;
            _settlementNames = settlementNames;
            _metrosNames = metrosNames;
            //_marksNames = marksNames;
            _imageUploader = imageUploader;
            clientCreater = httpClientCreater;
        }


        public async Task EvinAzPars()
        {

            var model = _unitOfWork.ParserAnnounceRepository.GetBySiteName("https://evin.az");
            if (!isActive)
            {
                if (model.isActive)
                {
                    try
                    {
                        var id = model.last_id;

                        isActive = true;
                        int x = 0;
                        int count = 0;
                        int duration = 0;

                        while (true)
                        {

                            if (count >= 10)
                            {
                                x++;
                                if (x >= 350)
                                    x = 0;

                                _httpClient = clientCreater.Create(proxies[x]);
                                count = 0;
                            }

                            try
                            {
                               
                               
                                //https://evin.az/real-estate/1641200494
                                var myUri = new Uri($"{model.site}/real-estate/{++id}", UriKind.Absolute);
                               
                                header = await _httpClient.GetAsync(myUri);
                                var url = header.RequestMessage.RequestUri.AbsoluteUri;
                                count++;

                                var response = await _httpClient.GetAsync(url);
                                var html = await response.Content.ReadAsStringAsync();
                                if (response.IsSuccessStatusCode)
                                {
                                    HtmlDocument doc = new HtmlDocument();
                                    doc.LoadHtml(html);

                                    Announce announce = new Announce();


                                    announce.announce_date = DateTime.Now;
                                    announce.parser_site = model.site;
                                    announce.original_id = id;
                                    announce.original_date = doc.DocumentNode.SelectSingleNode("/html/body/div[7]/div[2]/div/div[3]/div[2]/div[2]/div[2]/div[3]/span[2]").InnerText.Trim();

                                    var text = doc.DocumentNode.SelectSingleNode(".//div[@class='bg-white p-3 mb-3']//p").InnerText;

                                    announce.text = text;

                                    var allPropertiesKeys = doc.DocumentNode.SelectNodes(".//div[@class='font-weight-bold font-13']");
                                    var allPropertiesValues = doc.DocumentNode.SelectNodes(".//div[@class='font-13']");
                                    bool isSpaceEvaluated = false;
                                    for (int i = 0; i < allPropertiesKeys.Count; i++)
                                    {
                                        if (allPropertiesKeys[i].InnerText.Trim() == "Ünvan:")
                                        {
                                            announce.address = allPropertiesValues[i].InnerText.Trim();
                                        }
                                        else if (allPropertiesKeys[i].InnerText.Trim() == "Kateqoriya:")
                                        {
                                            var type = allPropertiesValues[i].InnerText.Trim();
                                            var resultType = _propertyType.GetTitleOfProperty(type);
                                            announce.property_type = resultType;
                                        }
                                        else if (allPropertiesKeys[i].InnerText.Trim() == "Ümumi sahə :")
                                        {
                                            var space = allPropertiesValues[i].InnerText.Trim().Split(" ")[0];
                                            announce.space = space;
                                            isSpaceEvaluated = true;
                                        }
                                        else if (allPropertiesKeys[i].InnerText.Trim() == "Yerləşdiyi mərtəbə:")
                                        {
                                            var floors = allPropertiesValues[i].InnerText.Trim();
                                            var current_floor = floors.Split(" / ")[0];
                                            var floor_count = floors.Split(" / ")[1];

                                            announce.current_floor = Int32.Parse(current_floor);
                                            announce.floor_count = Int32.Parse(floor_count);
                                        }
                                        else if (allPropertiesKeys[i].InnerText.Trim() == "Ümumi otaq sayı:")
                                        {
                                            announce.room_count = Int32.Parse(allPropertiesValues[i].InnerText.Trim());
                                        }
                                        else if (allPropertiesKeys[i].InnerText.Trim() == "Torpaq sahəsi:" && !isSpaceEvaluated)
                                        {
                                            announce.space = allPropertiesValues[i].InnerText.Trim().Split(" ")[0];
                                        }
                                        else if (allPropertiesKeys[i].InnerText.Trim() == "Kupça")
                                        {
                                            if (allPropertiesValues[i].InnerText.Trim() == "var")
                                            {
                                                announce.document = 1;
                                            }
                                            else
                                            {
                                                announce.document = 0;
                                            }
                                        }
                                    }

                                    var view_count = doc.DocumentNode.SelectNodes(".//span[@class='font-13 text-muted']")[1].InnerText;
                                    announce.view_count = Int32.Parse(view_count);

                                    var price = doc.DocumentNode.SelectSingleNode(".//div[@class='bg-main h-100 p-3 font-25 text-white font-weight-bold']").InnerText.Trim()
                                                                                                                                                           .Split(" ")[0];

                                    announce.price = Int32.Parse(price);

                                    var name = doc.DocumentNode.SelectSingleNode(".//div[@class='bg-light p-3 w-100 ml-lg-1 d-flex align-items-center']//div//div").InnerText.Trim();
                                    name = name.Split(" (")[0].Trim();

                                    if (name != null)
                                        announce.name = name;

                                    var mobile = doc.DocumentNode.SelectSingleNode(".//div[@class='bg-light p-3 w-100 ml-lg-1 d-flex align-items-center']//div//a[1]").InnerText.Trim();

                                    string[] charsToRemove = new string[] { "(", ")", "-", " " };
                                    foreach (var c in charsToRemove)
                                    {
                                        mobile = mobile.Replace(c, string.Empty);
                                    }

                                    if (mobile.Length > 10)
                                    {
                                        int divide = (mobile.Length / 10) - 1;
                                        int b = 10;
                                        while (divide > 0)
                                        {
                                            mobile = mobile.Insert(b, ",");
                                            b += 10;
                                            divide--;
                                        }

                                    }

                                    if (mobile != null)
                                        announce.mobile = mobile;

                                    var location = doc.DocumentNode.SelectSingleNode(".//li[@class='breadcrumb-item active']").InnerText.Trim();

                                    if (location.EndsWith("m/s"))
                                    {
                                        var metros = _metrosNames.GetMerosNamesAll();

                                        foreach (var metro in metros)
                                        {
                                            if (location.ToLower().Contains(metro.Key.ToLower()))
                                            {
                                                announce.metro_id = metro.Value;
                                                break;
                                            }
                                        }
                                    }
                                    else if (location.EndsWith("q."))
                                    {
                                        var settlements = _settlementNames.GetSettlementsNamesAll();

                                        foreach (var settlement in settlements)
                                        {
                                            if (location.ToLower().Contains(settlement.Key.ToLower()))
                                            {
                                                announce.settlement_id = settlement.Value;
                                                break;
                                            }
                                        }
                                    }
                                    else if (location.EndsWith("r."))
                                    {
                                        var regions = _unitOfWork.CitiesRepository.GetAllRegions();

                                        foreach (var region in regions)
                                        {
                                            if (location.ToLower().Contains(region.name.ToLower()))
                                            {
                                                announce.region_id = region.id;
                                                break;
                                            }
                                        }
                                    }


                                    var announce_type = doc.DocumentNode.SelectSingleNode(".//h1[@class='font-weight-bold font-14 font-lg-17 pr-1 mb-0']").InnerText;
                                    if (announce_type.Contains("Satı"))
                                    {
                                        announce.announce_type = 2;
                                    }
                                    else if (announce_type.Contains("İcarə"))
                                    {
                                        announce.announce_type = 1;
                                    }

                                    //images


                                    var filePath = $@"EvinAz/{DateTime.Now.Year}/{DateTime.Now.Month}/{id}/";
                                    var images = _imageUploader.ImageDownloader(doc, id.ToString(), filePath, _httpClient);

                                    var uri = new Uri(doc.DocumentNode.SelectNodes(".//div[@class='embed-responsive embed-responsive-4by3 border']//img")[0].Attributes["src"].Value);

                                    var uriWithoutQuery = uri.GetLeftPart(UriPartial.Path);
                                    var fileExtension = Path.GetExtension(uriWithoutQuery);
                                    announce.cover = $@"EvinAz/{DateTime.Now.Year}/{DateTime.Now.Month}/{id}/Thumb{fileExtension}";

                                    announce.logo_images = JsonSerializer.Serialize(await images);

                                   
                                    duration = 0;

                                    bool checkedNumber = false;
                                    var numberList = mobile.Split(',');
                                    var checkNumberRieltorResult = await _unitOfWork.CheckNumberRepository.CheckNumberForRieltorAsync(numberList);
                                    if (checkNumberRieltorResult > 0)
                                    {

                                        announce.announcer = checkNumberRieltorResult;
                                        announce.number_checked = true;
                                        checkedNumber = true;

                                    }

                                    await _unitOfWork.Announces.CreateAsync(announce);
                                   
                                 

                                    if (checkedNumber == false)
                                    {

                                        //EMLAK - BAZASI
                                        await _emlakBaza.CheckAsync(id, numberList);
                                    }
                                    _unitOfWork.Dispose();
                                }

                                if (header.StatusCode.ToString() == "NotFound")
                                {
                                    duration++;
                                }

                                if (duration >= maxRequest)
                                {
                                    model.last_id = (id - maxRequest);
                                    TelegramBotService.Sender("evin.az limited");

                                    isActive = false;
                                    _unitOfWork.ParserAnnounceRepository.Update(model);
                                    duration = 0;
                                    break;
                                }

                            }
                            catch (Exception e)
                            {
                                TelegramBotService.Sender($"no connection evin.az {e.Message}");

                                count = 10;
                            }
                        }
                    }
                    catch (Exception e)
                    {

                        TelegramBotService.Sender($"end catch evin.az {e.Message}");
                    }
                }
            }




        }
    }
}
