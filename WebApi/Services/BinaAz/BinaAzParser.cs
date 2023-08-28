using HtmlAgilityPack;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebApi.Models;
using WebApi.Repository;
using WebApi.Services.LalafoAz.Models;
using WebApi.ViewModels;

namespace WebApi.Services.BinaAz
{
    public class BinaAzParser
    {
        private readonly EmlakBazaWithProxy _emlakBaza;
        private static bool isActive = false;
        private readonly UnitOfWork unitOfWork;
        private readonly HttpClientCreater clientCreater;

        private HttpClient _httpClient;
        private readonly BinaAzParserImageUploader imageUploader;
        HttpResponseMessage header;
        public int maxRequest = 20;
        static string[] proxies = SingletonProxyServersIp.Instance;

        public BinaAzParser(EmlakBazaWithProxy emlakBaza,
                            HttpClientCreater clientCreater,
                            UnitOfWork unitOfWork,
                            BinaAzParserImageUploader imageUploader
                           //  HttpClient httpClient
                           )
        {
            this._emlakBaza = emlakBaza;
            this.clientCreater = clientCreater;
            this.unitOfWork = unitOfWork;
            this.imageUploader = imageUploader;
            Random rnd = new Random();
            _httpClient = this.clientCreater.Create(proxies[rnd.Next(0, 99)]);
            //  _httpClient = httpClient;
        }


        public async Task BinaAzPars()
        {
            var model = unitOfWork.ParserAnnounceRepository.GetBySiteName("https://bina.az");
            //_httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/97.0.4692.99 Safari/537.36");
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/97.0.4692.99 Safari/537.36");

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

                                if (await unitOfWork.Announces.IsAnnounceValidAsync(searchViewModel))
                                {
                                    continue;
                                };

                                Uri myUri = new Uri($"{model.site}/items/{id}", UriKind.Absolute);

                                //header = await _httpClient.GetAsync(myUri);

                                //var url = header.RequestMessage.RequestUri.AbsoluteUri;
                                count++;
                                HtmlDocument doc = new HtmlDocument();

                                var response = await _httpClient.GetAsync(myUri);


                                if (response.IsSuccessStatusCode)
                                {

                                    var html = await response.Content.ReadAsStringAsync();
                                    if (!string.IsNullOrEmpty(html))
                                    {

                                        try
                                        {
                                            duration = 0;
                                            doc.LoadHtml(html);

                                            //If there is a button(Ödə)
                                            if (doc.DocumentNode.SelectNodes(".//div[@class='payment-action-container']") != null)
                                            {
                                                continue;
                                            }

                                            //Announcement is waiting for acception
                                            if (doc.DocumentNode.SelectNodes(".//header[@class='item-show-pending-header']") != null)
                                            {
                                                model.last_id = id - 1;
                                                TelegramBotService.Sender($"bina.az waiting for acception");
                                                isActive = false;
                                                unitOfWork.ParserAnnounceRepository.Update(model);

                                                break;
                                            }
                                            Announce announce = new Announce();

                                            var phoneResponse = await _httpClient.GetAsync($"{myUri}/phones");
                                            if (phoneResponse.StatusCode != HttpStatusCode.OK)
                                            {
                                                continue;
                                            }
                                            StringBuilder numbers = new StringBuilder();

                                            var json = await phoneResponse.Content.ReadAsStringAsync();
                                            var result = JsonSerializer.Deserialize<PhonesModel>(json);
                                            for (int i = 0; i < result.phones.Length; i++)
                                            {
                                                var charsToRemove = new string[] { "(", ")", "-", ".", " " };
                                                foreach (var c in charsToRemove)
                                                {
                                                    result.phones[i] = result.phones[i].Replace(c, string.Empty);
                                                }
                                                if (i < result.phones.Length - 1)
                                                    numbers.Append($"{result.phones[i]},");
                                                else
                                                    numbers.Append($"{result.phones[i]}");
                                            }
                                            announce.mobile = numbers.ToString();

                                            bool checkedNumber = false;
                                            var numberList = numbers.ToString().Split(',');
                                            //var checkNumberRieltorResult =await unitOfWork.CheckNumberRepository.CheckNumberForRieltorAsync(numberList);
                                            //if (checkNumberRieltorResult > 0)
                                            //{

                                            //    announce.announcer = checkNumberRieltorResult;
                                            //    announce.number_checked = true;
                                            //    checkedNumber = true;

                                            //}


                                            var address = doc.DocumentNode.SelectSingleNode(".//div[@class='product-map__left__address']").InnerText;
                                            announce.address = address;
                                            announce.original_id = id;
                                            announce.parser_site = model.site;
                                            announce.price = Int32.Parse(doc.DocumentNode.SelectSingleNode(".//span[@class='price-val']").InnerText.Replace(" ", ""));

                                            announce.announce_date = DateTime.Now;
                                            announce.name = doc.DocumentNode.SelectSingleNode(".//div[@class='product-owner__info-name']").InnerText;
                                            announce.text = doc.DocumentNode.SelectSingleNode(".//div[@class='product-description__content']//p").InnerText;

                                            var stats = doc.DocumentNode.SelectNodes(".//div[@class='product-statistics__i']//span");
                                            announce.view_count = Int32.Parse(stats[1].InnerText.Replace("Baxışların sayı: ", ""));
                                            announce.original_date = stats[0].InnerText.Replace("Yeniləndi: ", "");


                                            ///////////////////////IMAGES
                                            var filePath = $@"BinaAz/{DateTime.Now.Year}/{DateTime.Now.Month}/{id}/";

                                            var images = imageUploader.ImageDownloader(doc, id.ToString(), filePath, _httpClient);






                                            var lnk = doc.DocumentNode.SelectNodes(".//div[@class='product-photos__slider-nav-i_picture']")[0];
                                            string styleAttributeValue = lnk.GetAttributeValue("style", "");

                                            int startIndex = styleAttributeValue.IndexOf("&#39;") + 5;

                                            // Find the ending index of the URL within the style attribute value
                                            int endIndex = styleAttributeValue.LastIndexOf("&#39;");

                                            // Extract the URL
                                            string imageUrl = styleAttributeValue.Substring(startIndex, endIndex - startIndex);


                                            var uri = new Uri(imageUrl);
                                            var uriWithoutQuery = uri.GetLeftPart(UriPartial.Path);
                                            var fileExtension = Path.GetExtension(uriWithoutQuery);
                                            announce.cover = $@"BinaAz/{DateTime.Now.Year}/{DateTime.Now.Month}/{id}/Thumb{fileExtension}";
                                            announce.logo_images = JsonSerializer.Serialize(await images);
                                            /////////////////////



                                            if (doc.DocumentNode.SelectNodes(".//a[@class='product-breadcrumbs__i-link']")[0].InnerText.Contains("Kirayə"))
                                                announce.announce_type = 1;
                                            else if (doc.DocumentNode.SelectNodes(".//a[@class='product-breadcrumbs__i-link']")[0].InnerText.Contains("Satış"))
                                                announce.announce_type = 2;

                                            //var props = doc.DocumentNode.SelectNodes(".//div[@class='product-properties__i']");

                                            foreach (var item in doc.DocumentNode.SelectNodes(".//div[@class='product-properties__i']"))
                                            {
                                                if (item.FirstChild.InnerText == "Kateqoriya")
                                                {
                                                    if (item.LastChild.InnerText == "Yeni tikili")
                                                        announce.property_type = 1;
                                                    else if (item.LastChild.InnerText == "Köhnə tikili")
                                                        announce.property_type = 2;
                                                    else if (item.LastChild.InnerText == "Bağ")
                                                        announce.property_type = 4;
                                                    else if (item.LastChild.InnerText == "Ev / Villa")
                                                        announce.property_type = 5;
                                                    else if (item.LastChild.InnerText == "Ofis")
                                                        announce.property_type = 6;
                                                    else if (item.LastChild.InnerText == "Obyekt")
                                                        announce.property_type = 7;
                                                    else if (item.LastChild.InnerText == "Torpaq")
                                                        announce.property_type = 9;
                                                    else if (item.LastChild.InnerText == "Qaraj")
                                                        announce.property_type = 10;
                                                }
                                                if (item.FirstChild.InnerText == "Sahə")
                                                    announce.space = item.LastChild.InnerText.Split(" ")[0];
                                                if (item.FirstChild.InnerText == "Otaq sayı")
                                                    announce.room_count = Int32.Parse(item.LastChild.InnerText);
                                                if (item.FirstChild.InnerText == "Mərtəbə")
                                                {
                                                    announce.current_floor = Int32.Parse(item.LastChild.InnerText.Split(" / ")[0]);
                                                    announce.floor_count = Int32.Parse(item.LastChild.InnerText.Split(" / ")[1]);
                                                }

                                            }

                                            var document = doc.DocumentNode.SelectSingleNode(".//div[@class='product-labels__i-icon--bill-of-sale']");
                                            if (document != null)
                                                announce.document = 1;

                                            var ipoteka = doc.DocumentNode.SelectSingleNode(".//div[@class='product-labels__i-icon--mortgag']");
                                            if (ipoteka != null)
                                                announce.kredit = 1;


                                            /////city
                                            var cities = unitOfWork.CitiesRepository.GetAll();

                                            var title = doc.DocumentNode.SelectSingleNode(".//h1[@class='product-title']").InnerText;

                                            bool isFound = false;
                                            foreach (var city in cities)
                                            {

                                                if (title.ToLower().Contains(city.name.ToLower()))
                                                {
                                                    announce.city_id = city.id;
                                                    isFound = true;
                                                    break;
                                                }
                                            }
                                            if (!isFound)
                                            {
                                                //Baku
                                                announce.city_id = 1;
                                            }

                                            var props = doc.DocumentNode.SelectNodes(".//li[@class='product-extras__i']//a");
                                            if (props != null)
                                            {
                                                foreach (var item in props)
                                                {
                                                    //// metro
                                                    if (item.InnerText.Contains(" m."))
                                                    {
                                                        var metros = unitOfWork.MetrosRepository.GetAll();
                                                        foreach (var metro in metros)
                                                        {
                                                            if (item.InnerText.Contains(metro.name))
                                                            {
                                                                announce.metro_id = metro.id;

                                                                break;
                                                            }
                                                        }
                                                    }
                                                    //////rayon
                                                    else if (item.InnerText.Contains(" r."))
                                                    {
                                                        var regions = unitOfWork.CitiesRepository.GetAllRegions();
                                                        foreach (var region in regions)
                                                        {
                                                            if (item.InnerText.Contains(region.name))
                                                            {
                                                                announce.region_id = region.id;
                                                            }
                                                        }
                                                    }
                                                    //////qesebe
                                                    else if (item.InnerText.Contains(" q."))
                                                    {
                                                        var settlements = unitOfWork.CitiesRepository.GetAllSettlement();
                                                        foreach (var settlement in settlements)
                                                        {
                                                            if (item.InnerText.Contains(settlement.name))
                                                            {
                                                                announce.settlement_id = settlement.id;
                                                                announce.region_id = settlement.region_id;
                                                            }
                                                        }
                                                    }
                                                }
                                            }



                                            var ownerType = doc.DocumentNode.SelectSingleNode("//div[@class='product-owner__info-region']").InnerText;
                                            if (ownerType == "vasitəçi (agent)")
                                            {
                                                announce.announcer = 2;
                                                announce.number_checked = true;

                                                int checkNumberRieltorResult = await unitOfWork.CheckNumberRepository.CheckNumberForRieltorAsync(numberList);

                                                // Mobile exists in rieltor table
                                                if (checkNumberRieltorResult > 0)
                                                {
                                                    announce.number_checked = true;
                                                }
                                                else
                                                {
                                                    List<Rieltor> rieltors = new();

                                                    for (int i = 0; i < numberList.Length; i++)
                                                    {
                                                        rieltors.Add(new Rieltor { Phone = numberList[i] });
                                                    }

                                                    await unitOfWork.RieltorRepository.BulkInsertAsync(rieltors);
                                                }

                                                var announceIds = await unitOfWork.Announces.GetAnnouncesByMobileListAsync(numberList);
                                                bool hasAnyChanges = false;

                                                foreach (var announceId in announceIds)
                                                {

                                                    await unitOfWork.Announces.ArendaAzUpdateAnnouncerAsync(new ArendaAzAnnouncerUpdateVM { Id = announceId, Announcer = 2 });
                                                    TelegramBotService.Sender($"bina.az - modify correct announcer for id = {announceId}");
                                                    hasAnyChanges = true;
                                                }

                                                if (hasAnyChanges)
                                                {
                                                    TelegramBotService.Sender($"bina.az - announce modification completed");
                                                }
                                            }


                                            var lastAnnounceId = await unitOfWork.Announces.CreateAsync(announce);

                                            if (ownerType != "vasitəçi (agent)")
                                            {
                                                //EMLAK - BAZASI
                                                await _emlakBaza.CheckAsync(lastAnnounceId, numberList);
                                            }

                                            HandleMapCoordinates(doc, lastAnnounceId);



                                            unitOfWork.Dispose();
                                        }

                                        catch (Exception e)
                                        {
                                            TelegramBotService.Sender($"Error ocurred while parsing -bina.az {e.Message}. OriginalId = {id}");
                                        }

                                    }
                                }
                                else
                                {
                                    duration++;

                                }

                                if (duration >= maxRequest)
                                {
                                    model.last_id = id - maxRequest;
                                    TelegramBotService.Sender($"bina.az limited {maxRequest}");
                                    isActive = false;

                                    unitOfWork.ParserAnnounceRepository.Update(model);
                                    duration = 0;


                                    break;
                                }
                            }
                            catch (Exception e)
                            {

                                TelegramBotService.Sender($"bina.az exception {e.Message}. OriginalId = {id}");

                                count = 10;

                            }

                        }
                    }
                    catch (Exception e)
                    {

                        TelegramBotService.Sender($"no connection bina.az {e.Message}");

                    }
                }
            }
        }


        public void HandleMapCoordinates(HtmlDocument doc, int announceId)
        {
            if (doc.GetElementbyId("item_map").GetAttributeValue("data-lat", "") != null && doc.GetElementbyId("item_map").GetAttributeValue("data-lng", "") != null)
            {
                var latitude = doc.GetElementbyId("item_map").GetAttributeValue("data-lat", "");
                var longitude = doc.GetElementbyId("item_map").GetAttributeValue("data-lng", "");

                var announceCoordinates = new AnnounceCoordinates { longitude = Convert.ToDecimal(longitude), latitude = Convert.ToDecimal(latitude), announce_id = announceId };
                unitOfWork.MarkRepository.CreateAnnounceCoordinates(announceCoordinates);

                var coordinates = $"{latitude},{longitude}";
                FindNearMarksAroundCurrentAnnounce(coordinates, announceId);
            }



        }

        public void FindNearMarksAroundCurrentAnnounce(string mapCoordinates, int announceId)
        {
            var marks = unitOfWork.MarkRepository.GetMarks(mapCoordinates);
            foreach (var item in marks)
            {
                var announceMark = new AnnounceMark { announce_id = announceId, mark_id = item.id };
                unitOfWork.MarkRepository.Create(announceMark);
            }
        }


    }
}