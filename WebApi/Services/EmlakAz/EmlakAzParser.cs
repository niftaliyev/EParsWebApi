﻿using HtmlAgilityPack;
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
        private readonly EmlakBazaWithProxy _emlakBaza;
        private readonly EmlakAzImageUploader _uploader;
        private static bool isActive = false;
        private readonly UnitOfWork unitOfWork;
        private readonly ITypeOfProperty typeOfProperty;
        private readonly EmlakAzMetrosNames metrosNames;
        private readonly EmlakAzMarksNames marksNames;
        private readonly EmlakAzRegionsNames regionsNames;
        private readonly EmlakAzSettlementNames settlementNames;
        HttpResponseMessage header;
        static bool turn = false;
        public const int maxRequest = 50;
        public EmlakAzParser(HttpClient httpClient,
            EmlakBazaWithProxy emlakBaza,
            EmlakAzImageUploader uploader,
            UnitOfWork unitOfWork,
            ITypeOfProperty typeOfProperty,
            EmlakAzMetrosNames metrosNames,
            EmlakAzMarksNames marksNames,
            EmlakAzRegionsNames regionsNames,
            EmlakAzSettlementNames settlementNames)
        {
            this.httpClient = httpClient;
            this._emlakBaza = emlakBaza;
            _uploader = uploader;
            this.unitOfWork = unitOfWork;
            this.typeOfProperty = typeOfProperty;
            this.metrosNames = metrosNames;
            this.marksNames = marksNames;
            this.regionsNames = regionsNames;
            this.settlementNames = settlementNames;
        }

        public async Task EmlakAzPars()
        {
            var model = unitOfWork.ParserAnnounceRepository.GetBySiteName("https://emlak.az");
            if (!isActive)
            {
                if (model.isActive)
                {
                    turn = true;
                    var id = model.last_id;
                    int counter = 0;
                    isActive = true;
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


                                            if (mobileregex != null)
                                                announce.mobile = mobileregex;


                                            announce.name = doc.DocumentNode.SelectSingleNode(".//p[@class='name-seller']").FirstChild.InnerText.Trim();


                                            Regex regex = new Regex("\\d+");
                                            if (doc.DocumentNode.SelectSingleNode(".//p[@class='pull-right']") != null)
                                            {
                                                string result = regex.Match(doc.DocumentNode.SelectSingleNode(".//p[@class='pull-right']").InnerText).ToString();
                                                announce.original_id = Int32.Parse(result);
                                            }

                                            //////////////// METRO ID AND RENT TYPE ///////////////
                                            if (doc.DocumentNode.SelectSingleNode(".//h1[@class='title']") != null)
                                            {
                                                announce.announce_type = typeOfProperty.GetTitleOfProperty(doc.DocumentNode.SelectSingleNode(".//h1[@class='title']").InnerText);

                                                if (doc.DocumentNode.SelectSingleNode(".//h1[@class='title']").InnerText.EndsWith(" m."))
                                                {
                                                    var dictionaryMetrosNames = metrosNames.GetMetroNameAll();
                                                    foreach (var titleMetroName in dictionaryMetrosNames)
                                                    {
                                                        if (doc.DocumentNode.SelectSingleNode(".//h1[@class='title']").InnerText.Contains(titleMetroName.Key))
                                                        {
                                                            announce.metro_id = titleMetroName.Value;
                                                            break;
                                                        }
                                                    }
                                                }
                                                if (doc.DocumentNode.SelectSingleNode(".//h1[@class='title']").InnerText.EndsWith(" r."))
                                                {
                                                    var dictionaryRegionsNames = regionsNames.GetRegionsNamesAll();
                                                    foreach (var regionName in dictionaryRegionsNames)
                                                    {
                                                        if (doc.DocumentNode.SelectSingleNode(".//h1[@class='title']").InnerText.Contains(regionName.Key))
                                                        {
                                                            announce.region_id = regionName.Value;
                                                            break;
                                                        }
                                                    }
                                                }
                                                var marksNamesOriginal = marksNames.GetMarksNamesAll();

                                                foreach (var markNameOriginal in marksNamesOriginal)
                                                {
                                                    if (doc.DocumentNode.SelectSingleNode(".//h1[@class='title']").InnerText.Contains(markNameOriginal.Key))
                                                    {
                                                        announce.mark = markNameOriginal.Value;
                                                        break;
                                                    }
                                                }
                                                var citiesNames = unitOfWork.CitiesRepository.GetAll();
                                                foreach (var cityName in citiesNames)
                                                {
                                                    if (doc.DocumentNode.SelectSingleNode(".//h1[@class='title']").InnerText.Contains(cityName.name))
                                                    {
                                                        announce.city_id = cityName.id;
                                                        break;
                                                    }
                                                }
                                                var settlementsNames = settlementNames.GetSettlementsNamesAll();
                                                foreach (var settlementName in settlementsNames)
                                                {
                                                    if (doc.DocumentNode.SelectSingleNode(".//h1[@class='title']").InnerText.Contains(settlementName.Key))
                                                    {
                                                        announce.settlement_id = settlementName.Value;
                                                        break;
                                                    }
                                                }
                                            }

                                            foreach (var item in doc.DocumentNode.SelectNodes(".//dl[@class='technical-characteristics']//dd"))
                                            {
                                                if (item != null)
                                                {
                                                    if (item.InnerText.StartsWith("Əmlakın növü"))
                                                        announce.property_type = typeOfProperty.GetTypeOfProperty(item.InnerText);
                                                    if (item.InnerText.StartsWith("Sahə"))
                                                    {
                                                        var space = regex.Match(item.InnerText).ToString();
                                                        announce.space = space;

                                                    }
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





                                            if (doc.DocumentNode.SelectSingleNode(".//p[@class='name-seller']//span") != null)
                                            {
                                                if (doc.DocumentNode.SelectSingleNode(".//p[@class='name-seller']//span").InnerText.Trim() == "(Mülkiyyətçi)")
                                                    announce.announcer = 1;
                                                if (doc.DocumentNode.SelectSingleNode(".//p[@class='name-seller']//span").InnerText.Trim() == "(Vasitəçi)")
                                                    announce.announcer = 2;
                                            }


                                            if (doc.DocumentNode.SelectSingleNode(".//span[@class='m']") != null)
                                                announce.price = Int32.Parse(doc.DocumentNode.SelectSingleNode(".//span[@class='m']").InnerText.Replace(" ", string.Empty));

                                            if (doc.DocumentNode.SelectSingleNode(".//div[@class='desc']//p") != null)
                                                announce.text = doc.DocumentNode.SelectSingleNode(".//div[@class='desc']//p").InnerText.Replace(" \n\n", string.Empty);

                                            if (doc.DocumentNode.SelectNodes(".//dl[@class='technical-characteristics']//dd")[2] != null)
                                                announce.room_count = Int32.Parse(regex.Match(doc.DocumentNode.SelectNodes(".//dl[@class='technical-characteristics']//dd")[2].InnerText).ToString());

                                            if (doc.DocumentNode.SelectSingleNode(".//span[@class='views-count']//strong") != null)
                                                announce.view_count = Int32.Parse(doc.DocumentNode.SelectSingleNode(".//span[@class='views-count']//strong").InnerText);

                                            if (doc.DocumentNode.SelectSingleNode(".//span[@class='date']//strong") != null)
                                                announce.original_date = doc.DocumentNode.SelectSingleNode(".//span[@class='date']//strong").InnerText;


                                            announce.parser_site = model.site;
                                            /////////////////////////// ImageUploader //////////////////////////////
                                            announce.cover = $@"EmlakAz/{DateTime.Now.Year}/{DateTime.Now.Month}/{id}/Thumb.jpg";

                                            var filePath = $@"EmlakAz/{DateTime.Now.Year}/{DateTime.Now.Month}/{id}/";
                                            var images = await _uploader.ImageDownloaderAsync(doc, id.ToString(), filePath);
                                            if (images != null)
                                            {

                                                announce.logo_images = JsonSerializer.Serialize(images);

                                            }


                                            announce.announce_date = DateTime.Now;
                                            counter = 0;

                                            bool checkedNumber = false;
                                            var numberList = mobileregex.Split(',');
                                            var checkNumberRieltorResult = unitOfWork.CheckNumberRepository.CheckNumberForRieltor(numberList);

                                            if (checkNumberRieltorResult > 0)
                                            {
                                                announce.number_checked = true;
                                                announce.announcer = checkNumberRieltorResult;
                                                checkedNumber = true;
                                            }

                                            if (doc.DocumentNode.SelectSingleNode(".//div[@class='map-address']//h4") != null)
                                            {
                                                announce.address = doc.DocumentNode.SelectSingleNode(".//div[@class='map-address']//h4").InnerText.Split("Ünvan: ")[1];
                                            }


                                            var lastAnnounceId = await unitOfWork.Announces.Create(announce);
                                            unitOfWork.Dispose();
                                            if (checkedNumber == false)
                                            {
                                                //EMLAK - BAZASI
                                                await _emlakBaza.CheckAsync(id, numberList);
                                            }



                                            if (doc.GetElementbyId("google_map").GetAttributeValue("value", "") != null)
                                            {
                                                var charsToRemoveMapCordinats = new string[] { "(", ")", " " };
                                                var mapCordinats = doc.GetElementbyId("google_map").GetAttributeValue("value", "");
                                                foreach (var c in charsToRemoveMapCordinats)
                                                {
                                                    mapCordinats = mapCordinats.Replace(c, string.Empty);
                                                }

                                                announce.google_map = mapCordinats;

                                                var marks = unitOfWork.MarkRepository.GetMarks(mapCordinats);
                                                foreach (var item in marks)
                                                {
                                                    var announceMark = new AnnounceMark { announce_id = lastAnnounceId, mark_id = item.id };
                                                    unitOfWork.MarkRepository.Create(announceMark);
                                                }

                                            }
                                        }
                                    }
                                }
                            } //if end
                            else
                            {

                                counter++;
                                if (counter >= maxRequest)
                                {
                                    model.last_id = (id - maxRequest);
                                    isActive = false;
                                    turn = false;
                                    unitOfWork.ParserAnnounceRepository.Update(model);
                                    counter = 0;
                                    TelegramBotService.Sender($"emlak.az limited {maxRequest}");

                                    break;

                                }
                            } // else end
                        }
                        catch (Exception e)
                        {
                            TelegramBotService.Sender($"no connection emlak.az {e.Message}");

                        }
                    } // while end
                }      // if isactive      

            }

        } // Method end

    } // class end
}
