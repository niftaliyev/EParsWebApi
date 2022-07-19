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

namespace WebApi.Services.EmlaktapAz
{
    public class EmlaktapAzParser
    {
        private HttpClient _httpClient;
        private readonly EmlakBaza _emlakBaza;
        HttpResponseMessage header;
        private readonly UnitOfWork _unitOfWork;
        private readonly TypeOfPropertyEmlaktapAz _propertyType;
        private readonly EmlaktapAzMetrosNames _metrosNames;
        private readonly EmlaktapAzSettlementNames _settlementNames;
        private readonly EmlaktapAzMarksNames _marksNames;
        private readonly EmlaktapAzImageUploader _imageUploader;
        //static string[] proxies = SingletonProxyServersIp.Instance;
       // private readonly HttpClientCreater clientCreater;
        private static bool isActive = false;
        public const int maxRequest = 50;

        public EmlaktapAzParser(HttpClient httpClient,
                                UnitOfWork unitOfWork,
                                EmlakBaza emlakBaza,
                                TypeOfPropertyEmlaktapAz propertyType,
                                EmlaktapAzSettlementNames settlementNames,
                                EmlaktapAzMetrosNames metrosNames,
                                EmlaktapAzMarksNames marksNames,
                                EmlaktapAzImageUploader imageUploader
                               //HttpClientCreater httpClientCreater
                               )
        {
            _httpClient = httpClient;
            _unitOfWork = unitOfWork;
            _emlakBaza = emlakBaza;
            _propertyType = propertyType;
            _settlementNames = settlementNames;
            _metrosNames = metrosNames;
            _marksNames = marksNames;
            _imageUploader = imageUploader;
            // clientCreater = httpClientCreater;
        }

        public async Task EmlaktapAzPars()
        {
            var model = _unitOfWork.ParserAnnounceRepository.GetBySiteName("http://emlaktap.az");

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
                            //if (count >= 10)
                            //{
                            //    x++;
                            //    if (x >= 350)
                            //        x = 0;


                            //    _httpClient = clientCreater.Create(proxies[x]);
                            //    count = 0;


                            //}

                            try
                            {
                              
                               
                                Uri myUri = new Uri($"{model.site}/yasamal-rayonu-3-otaqli-147-m2-yeni-tikili-kirayesi-{++id}/", UriKind.Absolute);

                                header = await _httpClient.GetAsync(myUri);
                                var url = header.RequestMessage.RequestUri.AbsoluteUri;
                                //count++;
                                HtmlDocument doc = new HtmlDocument();

                                var response = await _httpClient.GetAsync(url);

                                if (response.IsSuccessStatusCode)
                                {
                                    var html =await response.Content.ReadAsStringAsync();
                                    if (!String.IsNullOrEmpty(html))
                                    {
                                        doc.LoadHtml(html);
                                        try
                                        {
                                            //Announce exists
                                            if (doc.DocumentNode.SelectSingleNode("//*[@id='elan']/div[1]/div[1]/ul/li[1]/h1").InnerText != null)
                                            {
                                                var mobile = doc.DocumentNode.SelectNodes(".//div[@class='line line-phone']//strong");
                                              

                                                if (mobile != null)
                                                {
                                                    Announce announce = new Announce();

                                                    announce.original_id = id;
                                                    announce.parser_site = model.site;
                                                    announce.announce_date = DateTime.Now;

                                                   
                                                    StringBuilder mobileSb = new StringBuilder();

                                                    for (int i = 0; i < mobile.Count; i++)
                                                    {

                                                        if (i < mobile.Count - 1)
                                                            mobileSb.Append($"{mobile[i].InnerText.Replace(" ","")},");
                                                        else
                                                            mobileSb.Append($"{mobile[i].InnerText.Replace(" ", "")}");
                                                    }
                                                   

                                                    announce.mobile = mobileSb.ToString();


                                                    var name = doc.DocumentNode.SelectSingleNode(".//div[@class='line line-name']").InnerText.Trim();
                                                    announce.name = name.Replace(" (Rieltor / Makler)", "");

                                                    var price = doc.DocumentNode.SelectSingleNode("//*[@id='elan']/div[2]/aside[1]/span").InnerText.Replace(" ", "");

                                                    announce.price = Int32.Parse(price);

                                                    var text = doc.DocumentNode.SelectSingleNode("//*[@id='elan']/div[1]/div[2]/div/p").InnerText;
                                                    announce.text = text;

                                                    var announcer = doc.DocumentNode.SelectSingleNode("//*[@id='elan']/div[2]/aside[2]/div/div[1]/span").InnerText;
                                                    if (announcer.Contains("Rieltor"))
                                                    {
                                                        announce.announcer = 2;
                                                    }
                                                    else
                                                    {
                                                        announce.announcer = 1;
                                                    }

                                                    var title = doc.DocumentNode.SelectSingleNode("//*[@id='elan']/div[1]/div[1]/ul/li[1]/h1").InnerText;
                                                    var propTitle = _propertyType.GetTitleOfProperty(title);
                                                    announce.property_type = propTitle;

                                                    // .//div[@Class='body']//table//tr//td[1]
                                                    var allPropertyKeys = doc.DocumentNode.SelectNodes(".//div[@class='body']//table//tr//td[1]");
                                                    var allPropertyValues = doc.DocumentNode.SelectNodes(".//div[@class='body']//table//tr//td[2]");

                                                    for (int i = 0; i < allPropertyKeys.Count; i++)
                                                    {
                                                        if (allPropertyKeys[i].InnerText == "Sahə:")
                                                        {
                                                            var space = allPropertyValues[i].InnerText.Trim().Split(" ")[0];
                                                            announce.space = space;
                                                        }
                                                        else if (allPropertyKeys[i].InnerText == "Otaqlar:")
                                                        {
                                                            announce.room_count = Int32.Parse(allPropertyValues[i].InnerText);
                                                        }
                                                        else if (allPropertyKeys[i].InnerText == "Mərtəbə:")
                                                        {
                                                            if (allPropertyValues[i].InnerText.Length > 2)
                                                            {
                                                                var floor_count = allPropertyValues[i].InnerText.Replace(" ", "").Split("/")[1];
                                                                var current_floor = allPropertyValues[i].InnerText.Replace(" ", "").Split("/")[0];

                                                                announce.floor_count = Int32.Parse(floor_count);
                                                                announce.current_floor =Int32.Parse(current_floor);
                                                            }
                                                            else
                                                            {
                                                                announce.floor_count = Int32.Parse(allPropertyValues[i].InnerText);
                                                            }
                                                        }
                                                        else if (allPropertyKeys[i].InnerText == "Sənəd:")
                                                        {
                                                            if (allPropertyValues[i].InnerText == "Kupça")
                                                            {
                                                                announce.document = 1;
                                                            }
                                                            else
                                                            {
                                                                announce.document = 0;
                                                            }
                                                        }
                                                        else if (allPropertyKeys[i].InnerText == "Təmir:")
                                                        {
                                                            if (allPropertyValues[i].InnerText.Contains("Təmirli"))
                                                            {
                                                                announce.repair = true;
                                                            }
                                                            else
                                                            {
                                                                announce.repair = false;
                                                            }
                                                        }
                                                        else if (allPropertyKeys[i].InnerText == "Şəhər:")
                                                        {
                                                            var cities = _unitOfWork.CitiesRepository.GetAll();
                                                            foreach (var city in cities)
                                                            {
                                                                if (allPropertyValues[i].InnerText.Contains(city.name))
                                                                {
                                                                    announce.city_id = city.id;
                                                                    break;
                                                                }
                                                            }
                                                            if (allPropertyValues[i].InnerText.Contains("Xırdalan"))
                                                            {
                                                                announce.city_id = 26;
                                                            }
                                                        }
                                                        else if (allPropertyKeys[i].InnerText == "Rayon:")
                                                        {
                                                            if (allPropertyValues[i].InnerText.EndsWith("r."))
                                                            {
                                                                var regions = _unitOfWork.CitiesRepository.GetAllRegions();
                                                                foreach (var region in regions)
                                                                {
                                                                    if (region.name.ToLower().Contains(allPropertyValues[i].InnerText.Split(" r.")[0].ToLower()))
                                                                    {
                                                                        announce.region_id = region.id;
                                                                        break;
                                                                    }
                                                                }
                                                            }
                                                            
                                                            if (allPropertyValues[i].InnerText.EndsWith("ş."))
                                                            {
                                                                announce.city_id = 26;
                                                            }
                                                        }
                                                        else if (allPropertyKeys[i].InnerText == "Nişangahlar:")
                                                        {
                                                            if (allPropertyValues[i].InnerText.EndsWith("q."))
                                                            {
                                                                var settlements = _settlementNames.GetSettlementsNamesAll();
                                                                foreach (var settlement in settlements)
                                                                {
                                                                    if (settlement.Key.ToLower().Contains(allPropertyValues[i].InnerText.Split(" q.")[0].ToLower()))
                                                                    {
                                                                        announce.settlement_id = settlement.Value;
                                                                        break;
                                                                    }
                                                                }

                                                            }
                                                            if (allPropertyValues[i].InnerText.EndsWith("Metrosu"))
                                                            {
                                                                var metros = _metrosNames.GetMerosNamesAll();

                                                                foreach (var metro in metros)
                                                                {
                                                                    if (metro.Key.ToLower().Contains(allPropertyValues[i].InnerText.Split(" Metrosu")[0].ToLower()))
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
                                                                    if (mark.Key.ToLower().Contains(allPropertyValues[i].InnerText.ToLower()))
                                                                    {
                                                                        announce.mark = mark.Value;
                                                                        break;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        else if (allPropertyKeys[i].InnerText == "Dərc olundu:")
                                                        {
                                                            if (allPropertyValues[i].InnerText.Trim() == "Bugün")
                                                            {
                                                                announce.original_date = DateTime.Now.ToShortDateString();
                                                            }
                                                            else if (allPropertyValues[i].InnerText.Trim() == "Dünən")
                                                            {
                                                                announce.original_date = DateTime.Now.AddDays(-1).ToShortDateString();
                                                            }
                                                            else
                                                            {
                                                                announce.original_date = allPropertyValues[i].InnerText;
                                                            }
                                                        }
                                                    }


                                                    bool checkedNumber = false;
                                                    var numberList = mobileSb.ToString().Split(',');
                                                    var checkNumberRieltorResult = _unitOfWork.CheckNumberRepository.CheckNumberForRieltor(numberList);
                                                    if (checkNumberRieltorResult > 0)
                                                    {

                                                        announce.announcer = checkNumberRieltorResult;
                                                        announce.number_checked = true;
                                                        checkedNumber = true;

                                                    }
                                                    var filePath = $@"EmlaktapAz/{DateTime.Now.Year}/{DateTime.Now.Month}/{id}/";
                                                    var images = _imageUploader.ImageDownloader(doc, id.ToString(), filePath, _httpClient);
                                                    var thumbLink = doc.DocumentNode.SelectNodes(".//div[@class='fotorama']//a//img")[0].Attributes["src"].Value;
                                                    var abosulteLink = $"https://emlaktap.az{thumbLink}";

                                                    var uri = new Uri(abosulteLink););

                                                    var uriWithoutQuery = uri.GetLeftPart(UriPartial.Path);
                                                    var fileExtension = Path.GetExtension(uriWithoutQuery);
                                                    announce.cover = $@"EmlaktapAz/{DateTime.Now.Year}/{DateTime.Now.Month}/{id}/Thumb{fileExtension}";

                                                    announce.logo_images = JsonSerializer.Serialize(await images);

                                                    await _unitOfWork.Announces.Create(announce);
                                                    _unitOfWork.Dispose();
                                                    if (checkedNumber == false)
                                                    {

                                                        //EMLAK - BAZASI
                                                        await _emlakBaza.CheckAsync(_httpClient, id, numberList);
                                                    }



                                                }
                                            }
                                            
                                        }
                                        catch (Exception)
                                        {
                                            //If announce or mobileNo does not exist 
                                            duration++;
                                        }



                                        
                                    }
                                 



                                }


                                if (header.StatusCode.ToString() == "NotFound")
                                {
                                    duration++;
                                }
                                if (duration >= maxRequest)
                                {
                                    model.last_id = (id - maxRequest);
                                    isActive = false;
                                    _unitOfWork.ParserAnnounceRepository.Update(model);
                                    duration = 0;
                                    TelegramBotService.Sender($"emlaktap.az limited {maxRequest}");

                                    break;
                                }


                            }
                            catch (Exception e)
                            {
                                TelegramBotService.Sender($"emlaktap.az exception {e.Message}");
                               // count = 10;
                            }
                        }
                    }
                    catch (Exception e)
                    {

                        TelegramBotService.Sender($"no connection emlaktap.az {e.Message}");
                    }
                }
            }
        }
    }
}