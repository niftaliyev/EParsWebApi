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
using WebApi.ViewModels;

namespace WebApi.Services.UnvanAz
{
    public class UnvanAzParser
    {
        private HttpClient _httpClient;
        private readonly EmlakBazaWithProxy _emlakBaza;
        HttpResponseMessage header;
        private readonly UnitOfWork _unitOfWork;
        private readonly ITypeOfPropertyUnvanAz _propertyType;
        private readonly UnvanAzMetroNames _metrosNames;
        private readonly UnvanAzSettlementNames _settlementNames;
        private readonly UnvanAzImageUploader _imageUploader;
        static string[] proxies = SingletonProxyServersIp.Instance;
        private readonly HttpClientCreater clientCreater;
        private static bool isActive = false;
        public const int maxRequest = 50;

        public UnvanAzParser(//HttpClient httpClient,
                                UnitOfWork unitOfWork,
                                EmlakBazaWithProxy emlakBaza,
                                ITypeOfPropertyUnvanAz propertyType,
                                UnvanAzSettlementNames settlementNames,
                                UnvanAzMetroNames metrosNames,
                                UnvanAzImageUploader imageUploader,
                               HttpClientCreater httpClientCreater
                               )
        {
           // _httpClient = httpClient;
            _unitOfWork = unitOfWork;
            _emlakBaza = emlakBaza;
            _propertyType = propertyType;
            _settlementNames = settlementNames;
            _metrosNames = metrosNames;
            _imageUploader = imageUploader;
            clientCreater = httpClientCreater;
            Random rnd = new Random();
            _httpClient = this.clientCreater.Create(proxies[rnd.Next(0, 99)]);
        }


        public async Task UnvanAzPars()
        {

            var model = _unitOfWork.ParserAnnounceRepository.GetBySiteName("https://unvan.az");
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
                                if (x >= 100)
                                    x = 0;

                                _httpClient = clientCreater.Create(proxies[x]);
                                count = 0;
                            }

                            try
                            {


                                id++;
                                var searchViewModel = new AnnounceSearchViewModel
                                {
                                    ParserSite = model.site,
                                    OriginalId = id
                                };

                                if (await _unitOfWork.Announces.IsAnnounceValidAsync(searchViewModel))
                                {
                                    continue;
                                };
                                //https://unvan.az/ehmedli-qesebesi-2533300.html
                                var myUri = new Uri($"{model.site}/ehmedli-qesebesi-{id}.html", UriKind.Absolute);

                                //header = await _httpClient.GetAsync(myUri);
                                //var url = header.RequestMessage.RequestUri.AbsoluteUri;
                                count++;

                                var response = await _httpClient.GetAsync(myUri);
                                var html = await response.Content.ReadAsStringAsync();
                                if (response.IsSuccessStatusCode)
                                {
                                    HtmlDocument doc = new HtmlDocument();
                                    doc.LoadHtml(html);
                                    try
                                    {


                                        if (doc.DocumentNode.SelectSingleNode(".//div[@class='breadcrumb']//a[2]") != null)
                                        {
                                            if (doc.DocumentNode.SelectSingleNode(".//div[@class='breadcrumb']//a[2]").InnerText.Trim() == "Daşınmaz Əmlak")
                                            {

                                                duration = 0;
                                                Announce announce = new Announce();

                                                announce.parser_site = model.site;

                                                announce.announce_date = DateTime.Now;
                                                announce.original_id = id;

                                                var address = doc.DocumentNode.SelectSingleNode(".//p[@class='infop100 linkteshow']");
                                                if (address != null)
                                                    announce.address = address.InnerText.Trim().Split("Ünvan: ")[1];

                                                var text = doc.DocumentNode.SelectSingleNode(".//p[@class='infop100 fullteshow']");
                                                if (text != null)
                                                    announce.text = text.InnerText;


                                                if (doc.DocumentNode.SelectSingleNode(".//span[@class='pricecolor']//following-sibling::text()[1]") != null)
                                                {
                                                    var announceTypeText = doc.DocumentNode.SelectSingleNode(".//span[@class='pricecolor']//following-sibling::text()[1]").InnerText.Split("/ ")[1];
                                                    if (announceTypeText == "Gün" || announceTypeText == "Ay")
                                                    {
                                                        announce.announce_type = 1;
                                                    }

                                                }
                                                else
                                                {
                                                    announce.announce_type = 2;
                                                }

                                                var originalDate = doc.DocumentNode.SelectSingleNode(".//span[@class='viewsbb clear']").InnerText.Trim().Split("Tarix: ")[1];
                                                announce.original_date = originalDate;

                                                var allPropertiesKeys = doc.DocumentNode.SelectNodes(".//div[@id='openhalf']//p//b");


                                                for (int i = 0; i < allPropertiesKeys.Count; i++)
                                                {
                                                    if (allPropertiesKeys[i].InnerText.Trim() == "Əmlakın növü")
                                                    {
                                                        var type = doc.DocumentNode.SelectSingleNode(".//div[@id='openhalf']//p[2]//a").InnerText;
                                                        var propertyTypeId = _propertyType.GetTypeOfProperty(type);
                                                        announce.property_type = propertyTypeId;

                                                    }
                                                    else if (allPropertiesKeys[i].InnerText.Trim() == "Otaq sayı")
                                                    {
                                                        var room_count = doc.DocumentNode.SelectSingleNode(".//div[@id='openhalf']//p[3]").InnerText.Trim()
                                                                                          .Split("Otaq sayı ")[1];
                                                        announce.room_count = Int32.Parse(room_count);
                                                    }
                                                    else if (allPropertiesKeys[i].InnerText.Trim() == "Sahə")
                                                    {
                                                        var space = doc.DocumentNode.SelectSingleNode(".//div[@id='openhalf']//p[4]").InnerText.Trim().Split("Sahə ")[1].Split(" ")[0];
                                                        announce.space = space;
                                                    }
                                                    else if (allPropertiesKeys[i].InnerText.Trim() == "Qiyməti")
                                                    {
                                                        var price = doc.DocumentNode.SelectSingleNode(".//span[@class='pricecolor']").InnerText.Split(" Azn")[0].Replace(" ", "");
                                                        announce.price = Int32.Parse(price);
                                                    }
                                                }

                                                var locations = doc.DocumentNode.SelectNodes(".//p[@class='infop100 linkteshow']//a");

                                                for (int i = 0; i < locations.Count; i++)
                                                {
                                                    if (locations[i].InnerText.Trim().Contains("şəhəri"))
                                                    {
                                                        var cities = _unitOfWork.CitiesRepository.GetAll();

                                                        foreach (var city in cities)
                                                        {
                                                            if (locations[i].InnerText.ToLower().Contains(city.name.ToLower()))
                                                            {
                                                                announce.city_id = city.id;
                                                                break;
                                                            }
                                                        }
                                                    }
                                                    else if (locations[i].InnerText.Trim().Contains("rayonu"))
                                                    {
                                                        var regions = _unitOfWork.CitiesRepository.GetAllRegions();
                                                        foreach (var region in regions)
                                                        {
                                                            if (locations[i].InnerText.Split(" rayonu")[0].ToLower() == region.name.ToLower())
                                                            {
                                                                announce.region_id = region.id;
                                                                break;
                                                            }
                                                        }
                                                    }
                                                    else if (locations[i].InnerText.Trim().Contains("qəsəbəsi"))
                                                    {
                                                        var settlements = _settlementNames.GetSettlementsNamesAll();

                                                        foreach (var settlement in settlements)
                                                        {
                                                            if (locations[i].InnerText.ToLower() == settlement.Key.ToLower())
                                                            {
                                                                announce.settlement_id = settlement.Value;
                                                                break;
                                                            }
                                                        }
                                                    }
                                                    else if (locations[i].InnerText.Trim().Contains("metrosu"))
                                                    {
                                                        var metros = _metrosNames.GetMetroNameAll();

                                                        foreach (var metro in metros)
                                                        {
                                                            if (locations[i].InnerText.ToLower() == metro.Key.ToLower())
                                                            {
                                                                announce.metro_id = metro.Value;
                                                                break;
                                                            }
                                                        }
                                                    }
                                                }

                                                string contactName;
                                                if (doc.DocumentNode.SelectSingleNode(".//div[@class='infocontact']/a") != null)
                                                {
                                                    contactName = doc.DocumentNode.SelectSingleNode(".//div[@class='infocontact']//a").InnerText.Trim();
                                                    announce.name = contactName;
                                                }
                                                else
                                                {
                                                    contactName = doc.DocumentNode.SelectSingleNode(".//div[@id='openhalf']/div[2]/text()[4]").InnerText.Trim();
                                                    announce.name = contactName;
                                                }

                                                var contactNumbers = doc.DocumentNode.SelectSingleNode(".//div[@id='telshow']").InnerText;

                                                string[] charsToRemove = new string[] { "(", ")", " " };

                                                foreach (var item in charsToRemove)
                                                {
                                                    contactNumbers = contactNumbers.Replace(item, string.Empty);
                                                }

                                                if (contactNumbers.Length > 10)
                                                {
                                                    int divide = (contactNumbers.Length / 10) - 1;
                                                    int b = 10;
                                                    while (divide > 0)
                                                    {
                                                        contactNumbers = contactNumbers.Insert(b, ",");
                                                        b += 10;
                                                        divide--;
                                                    }
                                                }

                                                announce.mobile = contactNumbers;

                                                bool checkedNumber = false;
                                                var numberList = contactNumbers.Split(',');
                                                var checkNumberRieltorResult = await  _unitOfWork.CheckNumberRepository.CheckNumberForRieltorAsync(numberList);
                                                if (checkNumberRieltorResult > 0)
                                                {

                                                    announce.announcer = checkNumberRieltorResult;
                                                    announce.number_checked = true;
                                                    checkedNumber = true;

                                                }
                                                //Images
                                                var filePath = $@"UnvanAz/{DateTime.Now.Year}/{DateTime.Now.Month}/{id}/";
                                                var images = _imageUploader.ImageDownloader(doc, id.ToString(), filePath, _httpClient);

                                                var thumbLink = doc.DocumentNode.SelectNodes(".//div[@id='picsopen']//div//a//img")[0].Attributes["src"].Value;
                                                var absoluteThumbLink = $"https://unvan.az{thumbLink}";

                                                var uri = new Uri(absoluteThumbLink);

                                                var uriWithoutQuery = uri.GetLeftPart(UriPartial.Path);
                                                var fileExtension = Path.GetExtension(uriWithoutQuery);
                                                announce.cover = $@"UnvanAz/{DateTime.Now.Year}/{DateTime.Now.Month}/{id}/Thumb{fileExtension}";

                                                announce.logo_images = JsonSerializer.Serialize(await images);
                                                ////


                                                var lastAnnounceId = await _unitOfWork.Announces.CreateAsync(announce);


                                                if (checkedNumber == false)
                                                {

                                                    //EMLAK - BAZASI
                                                    await _emlakBaza.CheckAsync(lastAnnounceId, numberList);
                                                }
                                                _unitOfWork.Dispose();



                                            }
                                        }

                                        else if (response.RequestMessage.RequestUri.ToString() == "https://unvan.az/")
                                        {
                                            duration++;
                                        }

                                    }
                                    catch (Exception e)
                                    {
                                        TelegramBotService.Sender($"unvan.az error {e.Message}- OriginalId = {id}");

                                        duration++;
                                    }

                                }


                                if (duration >= maxRequest)
                                {
                                    model.last_id = (id - maxRequest);
                                    TelegramBotService.Sender("unvan.az limited");
                                    isActive = false;
                                    _unitOfWork.ParserAnnounceRepository.Update(model);
                                    duration = 0;
                                    break;
                                }

                            }
                            catch (Exception e)
                            {
                                TelegramBotService.Sender($"no connection unvan.az {e.Message}");

                                count = 10;
                            }
                        }
                    }
                    catch (Exception e)
                    {

                        TelegramBotService.Sender($"end catch unvan.az {e.Message}");
                    }
                }
            }




        }
    }
}
