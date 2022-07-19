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

namespace WebApi.Services.VipEmlakAz
{
    public class VipEmlakAzParser
    {
        private HttpClient _httpClient;
        private readonly EmlakBazaWithProxy _emlakBaza;
        HttpResponseMessage header;
        private readonly UnitOfWork _unitOfWork;
        private readonly ITypeOfPropertyVipEmlakAz _propertyType;
        private readonly VipEmlakAzMetroNames _metrosNames;
        private readonly VipEmlakAzSettlementNames _settlementNames;
        ////private readonly EmlaktapAzMarksNames _marksNames;
        private readonly VipEmlakAzImageUploader _imageUploader;
        static string[] proxies = SingletonProxyServersIp.Instance;
        private readonly HttpClientCreater clientCreater;
        private static bool isActive = false;
        public const int maxRequest = 50;

        public VipEmlakAzParser(HttpClient httpClient,
                                UnitOfWork unitOfWork,
                                EmlakBazaWithProxy emlakBaza,
                               ITypeOfPropertyVipEmlakAz propertyType,
                               VipEmlakAzSettlementNames settlementNames,
                               VipEmlakAzMetroNames metrosNames,
                               ////EmlaktapAzMarksNames marksNames,
                               VipEmlakAzImageUploader imageUploader,
                               HttpClientCreater httpClientCreater
                               )
        {
            _httpClient = httpClient;
            _unitOfWork = unitOfWork;
            _emlakBaza = emlakBaza;
            _propertyType = propertyType;
            _settlementNames = settlementNames;
            _metrosNames = metrosNames;
            ////_marksNames = marksNames;
            _imageUploader = imageUploader;
            clientCreater = httpClientCreater;
        }


        public async Task VipEmlakAzPars()
        {

            var model = _unitOfWork.ParserAnnounceRepository.GetBySiteName("https://vipemlak.az");
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
                                //https://vipemlak.az/masazir-qesebesi-461991.html
                                var myUri = new Uri($"{model.site}/masazir-qesebesi-{++id}.html", UriKind.Absolute);

                                header = await _httpClient.GetAsync(myUri);
                                var url = header.RequestMessage.RequestUri.AbsoluteUri;
                                count++;

                                var response = await _httpClient.GetAsync(url);
                                var html = await response.Content.ReadAsStringAsync();
                                if (response.IsSuccessStatusCode)
                                {
                                    HtmlDocument doc = new HtmlDocument();
                                    doc.LoadHtml(html);
                                    try
                                    {
                                        if (doc.DocumentNode.SelectSingleNode(".//div[@class='breadcrumb']//a[1]").InnerText == "Dasinmaz emlak")
                                        {
                                            Announce announce = new Announce();

                                            announce.original_id = id;
                                            announce.announce_date = DateTime.Now;
                                            announce.parser_site = model.site;

                                            var text = doc.DocumentNode.SelectSingleNode(".//div[@id='openhalf']//div[2]").InnerText;
                                            announce.text = text;

                                            var address = doc.DocumentNode.SelectNodes("//*[@id='openhalf']/div[11]/text()");


                                            StringBuilder sb = new StringBuilder();
                                            for (int i = 0; i < address.Count; i++)
                                            {
                                                if (i == address.Count - 1)
                                                {
                                                    sb.Append(address[i].InnerText);
                                                    break;
                                                }
                                                if (address[i].InnerText.ToLower().Contains("şəhəri"))
                                                {
                                                    var cities = _unitOfWork.CitiesRepository.GetAll();
                                                    bool isFound = false;
                                                    foreach (var city in cities)
                                                    {
                                                        if (address[i].InnerText.Trim().ToLower().Contains(city.name.ToLower()))
                                                        {
                                                            announce.city_id = city.id;
                                                            isFound = true;
                                                            break;
                                                        }
                                                    }

                                                    if (!isFound && address[i].InnerText.Trim().Contains("Xırdalan şəhəri"))
                                                    {
                                                        announce.city_id = 26;
                                                    }
                                                }
                                                else if (address[i].InnerText.ToLower().Contains("rayonu"))
                                                {
                                                    var regions = _unitOfWork.CitiesRepository.GetAllRegions();

                                                    foreach (var region in regions)
                                                    {
                                                        if (address[i].InnerText.Trim().ToLower().Contains(region.name.Trim().ToLower()))
                                                        {
                                                            announce.region_id = region.id;
                                                            break;
                                                        }
                                                    }
                                                }
                                                else if (address[i].InnerText.ToLower().Contains("qəsəbəsi"))
                                                {
                                                    var settlements = _settlementNames.GetSettlementsNamesAll();

                                                    foreach (var settlement in settlements)
                                                    {
                                                        if (address[i].InnerText.ToLower().Contains(settlement.Key.ToLower().Replace(" qəsəbəsi", "")))
                                                        {
                                                            announce.settlement_id = settlement.Value;
                                                            break;
                                                        }
                                                    }
                                                }

                                                else if (address[i].InnerText.ToLower().Contains("metrosu"))
                                                {
                                                    var metros = _metrosNames.GetMetroNameAll();

                                                    foreach (var metro in metros)
                                                    {
                                                        if (address[i].InnerText.ToLower().Contains(metro.Key.ToLower().Replace(" metrosu", "")))
                                                        {
                                                            announce.metro_id = metro.Value;
                                                            break;
                                                        }
                                                    }
                                                }


                                            }

                                            announce.address = sb.ToString();


                                            var allPropertiesKeys = doc.DocumentNode.SelectNodes(".//div[@class='infotd']//b");
                                            var allPropertiesValues = doc.DocumentNode.SelectNodes(".//div[@class='infotd2']");


                                            for (int i = 0; i < allPropertiesKeys.Count; i++)
                                            {
                                                if (allPropertiesKeys[i].InnerText == "Əmlakın növü")
                                                {
                                                    var type = allPropertiesValues[i].InnerText;
                                                    var resultType = _propertyType.GetTypeOfProperty(type);
                                                    announce.property_type = resultType;
                                                }
                                                else if (allPropertiesKeys[i].InnerText == "Otaq sayı")
                                                {
                                                    var room_count = allPropertiesValues[i].InnerText;
                                                    announce.room_count = Int32.Parse(room_count);
                                                }
                                                else if (allPropertiesKeys[i].InnerText == "Sahə")
                                                {
                                                    var space = allPropertiesValues[i].InnerText.Split(" ")[0];
                                                    announce.space = space;
                                                }
                                                else if (allPropertiesKeys[i].InnerText == "Qiymət")
                                                {
                                                    var price = doc.DocumentNode.SelectSingleNode(".//span[@class='pricecolor']").InnerText.Replace(" Azn", "").Replace(" ", "");
                                                    announce.price = Int32.Parse(price);
                                                }
                                            }


                                            var mobile = doc.DocumentNode.SelectSingleNode(".//div[@id='telshow']").InnerText;

                                            string[] charsToRemove = new string[] { "(", ")", " " };

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

                                            bool checkedNumber = false;
                                            var numberList = mobile.Split(',');
                                            var checkNumberRieltorResult = _unitOfWork.CheckNumberRepository.CheckNumberForRieltor(numberList);
                                            if (checkNumberRieltorResult > 0)
                                            {

                                                announce.announcer = checkNumberRieltorResult;
                                                announce.number_checked = true;
                                                checkedNumber = true;

                                            }

                                            var name = doc.DocumentNode.SelectSingleNode(".//div[@class='infocontact']").InnerText.Trim().Split("\r\n\t\t ")[1];
                                            if (name != null)
                                                announce.name = name.Trim();

                                            var original_date = doc.DocumentNode.SelectSingleNode(".//span[@class='viewsbb clear']").InnerText.Trim();
                                            announce.original_date = original_date.Split(" ")[1];


                                            //Images
                                            var filePath = $@"VipEmlakAz/{DateTime.Now.Year}/{DateTime.Now.Month}/{id}/";
                                            var images = _imageUploader.ImageDownloader(doc, id.ToString(), filePath, _httpClient);

                                            var thumbLink = doc.DocumentNode.SelectNodes(".//div[@id='picsopen']//div//a//img")[0].Attributes["src"].Value;
                                            var absoluteThumbLink = $"https://vipemlak.az{thumbLink}";

                                            var uri = new Uri(absoluteThumbLink);

                                            var uriWithoutQuery = uri.GetLeftPart(UriPartial.Path);
                                            var fileExtension = Path.GetExtension(uriWithoutQuery);
                                            announce.cover = $@"VipEmlakAz/{DateTime.Now.Year}/{DateTime.Now.Month}/{id}/Thumb{fileExtension}";

                                            announce.logo_images = JsonSerializer.Serialize(await images);

                                            await _unitOfWork.Announces.Create(announce);
                                            _unitOfWork.Dispose();

                                            if (checkedNumber == false)
                                            {

                                                //EMLAK - BAZASI
                                                await _emlakBaza.CheckAsync(id, numberList);
                                            }
                                        }
                                        //else
                                        //{
                                        //    duration++;
                                        //}
                                    }
                                    catch (Exception)
                                    {
                                        duration++;
                                    }

                                }

                                if (header.StatusCode.ToString() == "NotFound")
                                {
                                    duration++;
                                }

                                if (duration >= maxRequest)
                                {
                                    model.last_id = (id - maxRequest);
                                    //TelegramBotService.Sender("vipemlak.az limited");

                                    isActive = false;
                                    _unitOfWork.ParserAnnounceRepository.Update(model);
                                    duration = 0;
                                    break;
                                }

                            }
                            catch (Exception e)
                            {
                                //TelegramBotService.Sender($"no connection vipemlak.az {e.Message}");

                                count = 10;
                            }
                        }
                    }
                    catch (Exception e)
                    {

                        //TelegramBotService.Sender($"end catch vipemlak.az {e.Message}");
                    }
                }
            }




        }
    }
}
