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

namespace WebApi.Services.UcuzTapAz
{
    public class UcuzTapAzParser
    {
        private  HttpClient _httpClient;
        private readonly EmlakBazaWithProxy _emlakBaza;
        HttpResponseMessage header;
        private readonly UnitOfWork _unitOfWork;
        private readonly TypeOfPropertyUcuzTapAz _typeOfProperty;
        private readonly HttpClientCreater clientCreater;
        private readonly UcuzTapAzImageUploader _imageUploader;
        //private readonly MetrosRepository _metroRepository;
        private readonly UcuzTapAzSettlementsNames _settlementNames;
        private readonly UcuzTapAzMetroNames _metroNames;
        private readonly UcuzTapAzMarksNames _marksNames;
        static string[] proxies = SingletonProxyServersIp.Instance;

        private static bool isActive = false;
        public const int maxRequest = 50;

        public UcuzTapAzParser(HttpClient httpClient,
                                UnitOfWork unitOfWork,
                                TypeOfPropertyUcuzTapAz typeOfProperty,
                                EmlakBazaWithProxy emlakBaza,
                                UcuzTapAzImageUploader imageUploader,
                                //MetrosRepository metroRepository,
                                UcuzTapAzSettlementsNames settlementsNames,
                                UcuzTapAzMetroNames metroNames,
                                UcuzTapAzMarksNames marksNames,
                                HttpClientCreater httpClientCreater
                               )
        {
            _httpClient = httpClient;
            _unitOfWork = unitOfWork;
            _typeOfProperty = typeOfProperty;
            _emlakBaza = emlakBaza;
            _imageUploader = imageUploader;
            //_metroRepository = metroRepository;
            _settlementNames = settlementsNames;
            _metroNames = metroNames;
            _marksNames = marksNames;
            clientCreater = httpClientCreater;
        }


        public async Task UcuzTapAzPars()
        {

            var model = _unitOfWork.ParserAnnounceRepository.GetBySiteName("https://ucuztap.az");

            if (!isActive)
            {
                if (model.isActive)
                {
                    try
                    {
                        int id = model.last_id;
                        isActive = true;
                        int x = 0;
                        int duration = 0;
                        int count = 0;

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
                                Uri myUri = new Uri($"{model.site}/elan/{++id}-sat%C4%B1l%C4%B1r-kohne-tikili-65m-3-otaql%C4%B1/", UriKind.Absolute);

                                header = await _httpClient.GetAsync(myUri);
                                var url = header.RequestMessage.RequestUri.AbsoluteUri;
                                count++;
                                HtmlDocument doc = new HtmlDocument();

                                var response = await _httpClient.GetAsync(url);

                                if (response.IsSuccessStatusCode)
                                {
                                    var html = await response.Content.ReadAsStringAsync();
                                    if (!String.IsNullOrEmpty(html))
                                    {
                                        doc.LoadHtml(html);
                                        try
                                        {
                                            //only parse "Daşınmaz əmlak"
                                            if (doc.DocumentNode.SelectSingleNode("/html/body/div[4]/div/ol/li[2]/a").InnerText.Trim() == "Daşınmaz əmlak")
                                            {
                                                //if mobile exists
                                                if (doc.DocumentNode.SelectSingleNode("/html/body/div[5]/div/div[1]/div/aside/div/div/div[1]/div[2]/strong").InnerText != null)
                                                {
                                                    duration = 0;
                                                    Announce announce = new Announce();

                                                    announce.original_id = id;
                                                    announce.parser_site = model.site;
                                                    announce.announce_date = DateTime.Now;

                                                    var announcePrice = doc.DocumentNode.SelectSingleNode("/html/body/div[5]/div/div[1]/section/div/div[2]/div[2]/div[1]/button[1]/strong").InnerText;
                                                    if (announcePrice != null && announcePrice != "Razılaşma ilə")
                                                    {
                                                        announce.price = Int32.Parse(announcePrice.Replace(" ", ""));
                                                    }

                                                    var announceText = doc.DocumentNode.SelectSingleNode("/html/body/div[5]/div/div[1]/section/div/div[3]/div[2]/h2").InnerText;
                                                    if (announceText != null)
                                                    {
                                                        announce.text = announceText;
                                                    }

                                                    var announceOriginalDate = doc.DocumentNode.SelectSingleNode("/html/body/div[5]/div/div[1]/section/div/div[4]/div[2]/div/span[2]").InnerText;
                                                    if (announceOriginalDate.Contains("Bugün") || announceOriginalDate.Contains("əvvəl"))
                                                    {
                                                        announce.original_date = DateTime.Now.ToShortDateString();
                                                    }
                                                    else if (announceOriginalDate.Contains("Bu həftə"))
                                                    {
                                                        announce.original_date = DateTime.Now.AddDays(-2).ToShortDateString();
                                                    }
                                                    else if (announceOriginalDate.Contains("Dünən"))
                                                    {
                                                        announce.original_date = DateTime.Now.AddDays(-1).ToShortDateString();
                                                    }
                                                    else
                                                    {
                                                        announce.original_date = announceOriginalDate;
                                                    }

                                                    var allPropertiesKeys = doc.DocumentNode.SelectNodes("/html/body/div[5]/div/div[1]/section/div/div[3]/div[1]/table/tbody/tr/td[1]");
                                                    var allPropertiesValues = doc.DocumentNode.SelectNodes("/html/body/div[5]/div/div[1]/section/div/div[3]/div[1]/table/tbody/tr/td[2]");

                                                    /////////////// CHECK 
                                                    ///
                                                    for (int i = 0; i < allPropertiesKeys.Count; i++)
                                                    {
                                                        if (allPropertiesKeys[i].InnerText == "Otaq sayı:")
                                                        {
                                                            announce.room_count = Int32.Parse(allPropertiesValues[i].InnerText);
                                                        }
                                                        else if (allPropertiesKeys[i].InnerText.StartsWith("Sahə"))
                                                        {
                                                            announce.space = allPropertiesValues[i].InnerText;
                                                        }
                                                        //Check again
                                                        else if (allPropertiesKeys[i].InnerText == "Rayon:")
                                                        {
                                                            if (allPropertiesValues[i].InnerText.EndsWith("q."))
                                                            {
                                                                var settlements = _settlementNames.GetSettlementsNamesAll();

                                                                foreach (var settlement in settlements)
                                                                {
                                                                    if (settlement.Key.ToLower().Contains(allPropertiesValues[i].InnerText.Split(" q.")[0].ToLower()))
                                                                    {
                                                                        announce.settlement_id = settlement.Value;
                                                                        break;
                                                                    }
                                                                }

                                                            }
                                                            else if (allPropertiesValues[i].InnerText.EndsWith("r."))
                                                            {
                                                                var regions = _unitOfWork.CitiesRepository.GetAllRegions();
                                                                foreach (var region in regions)
                                                                {
                                                                    if (allPropertiesValues[i].InnerText.ToLower().Contains(region.name.ToLower()))
                                                                    {
                                                                        announce.region_id = region.id;
                                                                        break;
                                                                    }

                                                                }

                                                            }
                                                            else if (allPropertiesValues[i].InnerText.Contains("Xırdalan ş."))
                                                            {
                                                                announce.city_id = 26;
                                                                break;
                                                            }
                                                            //else if (allPropertiesValues[i].InnerText.EndsWith("q."))
                                                            //{
                                                            //    var settlements = _settlementNames.GetSettlementsNamesAll();

                                                            //    foreach (var settlement in settlements)
                                                            //    {
                                                            //        if (settlement.Key.ToLower().Contains(allPropertiesValues[i].InnerText.Split(" q.")[0].ToLower()))
                                                            //        {
                                                            //            announce.settlement_id = settlement.Value;
                                                            //            break;
                                                            //        }
                                                            //    }
                                                            //}

                                                        }
                                                        else if (allPropertiesKeys[i].InnerText == "Ərazi:")
                                                        {

                                                            if (allPropertiesValues[i].InnerText.EndsWith("q."))
                                                            {
                                                                var settlements = _settlementNames.GetSettlementsNamesAll();

                                                                foreach (var settlement in settlements)
                                                                {
                                                                    if (settlement.Key.ToLower().Contains(allPropertiesValues[i].InnerText.Split(" q.")[0].ToLower()))
                                                                    {
                                                                        announce.settlement_id = settlement.Value;
                                                                        break;
                                                                    }
                                                                }

                                                            }

                                                            else if (allPropertiesValues[i].InnerText.EndsWith("Metrosu"))
                                                            {
                                                                var metros = _metroNames.GetMerosNamesAll();
                                                                foreach (var metro in metros)
                                                                {
                                                                    if (metro.Key.ToLower().Contains(allPropertiesValues[i].InnerText.Split(" Metrosu")[0].ToLower()))
                                                                    {
                                                                        announce.metro_id = metro.Value;
                                                                        break;
                                                                    }
                                                                }
                                                            }


                                                            else
                                                            {
                                                                var marks = _marksNames.GetMarksNamesAll();

                                                                foreach (var mark in marks)
                                                                {
                                                                    if (mark.Key.ToLower().Contains(allPropertiesValues[i].InnerText.ToLower()))
                                                                    {
                                                                        announce.mark = mark.Value;
                                                                        break;
                                                                    }
                                                                }
                                                            }


                                                        }
                                                    }


                                                    var cityName = doc.DocumentNode.SelectSingleNode("/html/body/div[5]/div/div[1]/section/div/div[4]/div[2]/div/span[1]").InnerText.Trim();

                                                    if (cityName != null)
                                                    {
                                                        var cities = _unitOfWork.CitiesRepository.GetAll();

                                                        foreach (var city in cities)
                                                        {
                                                            if (city.name.ToLower().Contains(cityName.ToLower()))
                                                            {
                                                                announce.city_id = city.id;
                                                                break;
                                                            }
                                                        }
                                                    }

                                                    var name = doc.DocumentNode.SelectSingleNode("/html/body/div[5]/div/div[1]/div/aside/div/a/h3").InnerText;
                                                    if (name != null)
                                                    {
                                                        announce.name = name;
                                                    }


                                                    var prop = doc.DocumentNode.SelectSingleNode("/html/body/div[5]/div/div[1]/section/div/div[2]/div[2]/a/strong").InnerText;
                                                    var typeProp = _typeOfProperty.GetTitleOfProperty(prop);
                                                    if (typeProp > 0)
                                                    {
                                                        announce.property_type = typeProp;
                                                    }
                                                    else
                                                    {
                                                        var checkFromTitle = doc.DocumentNode.SelectSingleNode("/html/body/div[5]/div/div[1]/section/div/div[2]/div[2]/h1/strong").InnerText;
                                                        if (checkFromTitle.Contains("Ofis"))
                                                        {
                                                            announce.property_type = 6;
                                                        }
                                                        else if (checkFromTitle.Contains("Obyekt"))
                                                        {
                                                            announce.property_type = 7;
                                                        }
                                                        else if (checkFromTitle.Contains("Mağaza"))
                                                        {
                                                            announce.property_type = 8;
                                                        }
                                                    }

                                                    var announceType = doc.DocumentNode.SelectSingleNode("/html/body/div[5]/div/div[1]/section/div/div[2]/div[2]/h1/strong").InnerText;

                                                    var typeOfProp = _typeOfProperty.GetTypeOfProperty(announceType);
                                                    announce.announce_type = typeOfProp;

                                                    var announceViewCount = doc.DocumentNode.SelectSingleNode("/html/body/div[5]/div/div[1]/section/div/div[4]/div[2]/div/span[3]").InnerText;
                                                    announce.view_count = Int32.Parse(announceViewCount.Split(" dəfə")[0].Trim());



                                                    var mobile = doc.DocumentNode.SelectSingleNode("/html/body/div[5]/div/div[1]/div/aside/div/div/div[1]/div[2]/strong").InnerText;
                                                    var charsToRemove = new string[] { " ", "(", ")" };

                                                    foreach (var item in charsToRemove)
                                                    {
                                                        mobile = mobile.Replace(item, "");

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

                                                    var uri = new Uri(doc.DocumentNode.SelectSingleNode("/html/body/div[5]/div/div[1]/section/div/div[2]/div[2]/div[2]/div[1]/a[1]/img").Attributes["src"].Value);

                                                    var filePath = $@"UcuzTapAz/{DateTime.Now.Year}/{DateTime.Now.Month}/{id}/";


                                                    var images =  _imageUploader.ImageDownloader(doc, id.ToString(), filePath, _httpClient);
                                                    var uriWithoutQuery = uri.GetLeftPart(UriPartial.Path);
                                                    var fileExtension = Path.GetExtension(uriWithoutQuery);

                                                    announce.cover = $@"UcuzTapAz/{DateTime.Now.Year}/{DateTime.Now.Month}/{id}/Thumb{fileExtension}";
                                                    announce.logo_images = JsonSerializer.Serialize(await images);

                                                    await _unitOfWork.Announces.Create(announce);
                                                    _unitOfWork.Dispose();

                                                    if (checkedNumber == false)
                                                    {

                                                        //EMLAK - BAZASI
                                                        await _emlakBaza.CheckAsync(id, numberList);
                                                    }

                                                }

                                            }
                                            else
                                            {
                                                duration++;
                                            }
                                        }
                                        catch (Exception)
                                        {
                                            duration++;
                                        }


                                    }



                                }

                                if (response.StatusCode.ToString() == "NotFound")
                                {
                                    duration++;
                                }


                                if (duration >= maxRequest)
                                {
                                    model.last_id = (id - maxRequest);
                                    TelegramBotService.Sender("ucuztap.az limited");

                                    isActive = false;
                                    _unitOfWork.ParserAnnounceRepository.Update(model);
                                    duration = 0;
                                    break;
                                }

                            }
                            catch (Exception e)
                            {
                                TelegramBotService.Sender($"ucuztap.az exception {e}");
                            }
                        }
                    }
                    catch (Exception e)
                    {

                        TelegramBotService.Sender($"ucuztap.az no connection  {e}");
                    }
                }

            }








        }

    }
}
