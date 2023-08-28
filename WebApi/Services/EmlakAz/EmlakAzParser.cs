using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebApi.Models;
using WebApi.Repository;
using WebApi.Services.EmlakAz.Interfaces;
using WebApi.ViewModels;

namespace WebApi.Services.EmlakAz
{
    public class EmlakAzParser : SiteParser, IMap
    {
        private HtmlDocument _htmlDocument;
        private Announce _announce;
        private int _duration = 0;
        private string _siteUrl = null;
        private ParserAnnounce _model;
        private const int maxRequest = 10;
        private int count = 0;

        private HttpClient _httpClient;
        private readonly EmlakBazaWithProxy _emlakBaza;
        private readonly EmlakAzImageUploader _uploader;
        private readonly UnitOfWork unitOfWork;
        private readonly ITypeOfProperty typeOfProperty;
        private readonly EmlakAzMetrosNames metrosNames;
        private readonly EmlakAzMarksNames marksNames;
        private readonly EmlakAzRegionsNames regionsNames;
        private readonly EmlakAzSettlementNames settlementNames;
        private readonly HttpClientCreater clientCreater;
        static string[] proxies = SingletonProxyServersIp.Instance;

        public EmlakAzParser(
                            EmlakBazaWithProxy emlakBaza,
                            EmlakAzImageUploader uploader,
                            UnitOfWork unitOfWork,
                            ITypeOfProperty typeOfProperty,
                            EmlakAzMetrosNames metrosNames,
                            EmlakAzMarksNames marksNames,
                            EmlakAzRegionsNames regionsNames,
                            EmlakAzSettlementNames settlementNames,
                            HttpClientCreater clientCreater)
        {
            _emlakBaza = emlakBaza;
            _uploader = uploader;
            this.unitOfWork = unitOfWork;
            this.typeOfProperty = typeOfProperty;
            this.metrosNames = metrosNames;
            this.marksNames = marksNames;
            this.regionsNames = regionsNames;
            this.settlementNames = settlementNames;
            this.clientCreater = clientCreater;
            Random rnd = new Random();
            _httpClient = this.clientCreater.Create(proxies[rnd.Next(0, 99)]);
        }

        public override async Task ParseSite()
        {
            try
            {
                TelegramBotService.Sender("start emlak.az scraping");

                _model = unitOfWork.ParserAnnounceRepository.GetBySiteName("https://emlak.az");

                if (_model.isActive)
                {
                    var id = _model.last_id;
                    int countOfDbHit = 0;
                    int x = 0;

                    while (true)
                    {
                        id++;

                        if (countOfDbHit < 10 && await IsAnnouncementAvailableInDbAsync(_model.site, id))
                        {
                            continue;
                        }
                        else if (countOfDbHit <= 10)
                        {
                            countOfDbHit++;
                        }

                        CreateHttpClientByIpAddressFromProxyList(count, ref x);

                        var siteUri = new Uri($"{_model.site}/{id}-.html");
                        var request = new HttpRequestMessage(HttpMethod.Get, siteUri);
                        request.Headers.Add("Cookie", "PHPFRONTSESSID=o657t5ei59umr3gr8dsqa47443; _csrf=97dba28e2e9c8665564a8dcd77f79bd00b67a53bd9d831f7948d3a0b2b9775e2a%3A2%3A%7Bi%3A0%3Bs%3A5%3A%22_csrf%22%3Bi%3A1%3Bs%3A32%3A%22PNH-Zcai3w7OTcY_Ms5mW9symUglmSGP%22%3B%7D; popup=ok");

                        using var responseMessage = await _httpClient.SendAsync(request);

                        count++;

                        _siteUrl = responseMessage.RequestMessage.RequestUri.AbsoluteUri;

                        if (responseMessage.IsSuccessStatusCode)
                        {
                            await PopulateDocumentWithResponseMessageAsync(responseMessage);

                            await HandleScrapingAsync(id);
                        }
                        else if (responseMessage.StatusCode == HttpStatusCode.NotFound)
                        {
                            _duration++;
                        }

                        if (ShouldScrapingStop(_duration))
                        {
                            StopScraping(id);
                            break;
                        }

                    } // while end
                }// if end
            }
            catch (Exception e)
            {
                int line = (new StackTrace(e, true)).GetFrame(0).GetFileLineNumber();
                TelegramBotService.Sender($"Main - emlak.az - {line} - {e.Message}");
            }


        }
        protected override async Task<bool> IsAnnouncementAvailableInDbAsync(string siteUrl, int id)
        {
            var searchViewModel = new AnnounceSearchViewModel
            {
                ParserSite = siteUrl,
                OriginalId = id
            };

            if (await unitOfWork.Announces.IsAnnounceValidAsync(searchViewModel))
            {
                return true;
            };

            return false;
        }
        protected override void CreateHttpClientByIpAddressFromProxyList(int count, ref int x)
        {
            if (count >= 10)
            {
                //Dispose old instance
                _httpClient.Dispose();

                x++;
                if (x >= 100)
                    x = 0;

                _httpClient = clientCreater.Create(proxies[x]);
                this.count = 0;
            }
        }
        protected override async Task PopulateDocumentWithResponseMessageAsync(HttpResponseMessage message)
        {
            var stringContent = await message.Content.ReadAsStringAsync();
            _htmlDocument = new HtmlDocument();
            _htmlDocument.LoadHtml(stringContent);
        }
        protected override async Task HandleScrapingAsync(int announcementId)
        {
            try
            {
                if (!_siteUrl.EndsWith("/site"))
                {
                    if (_htmlDocument.DocumentNode.SelectSingleNode(".//p[@class='phone']") != null)
                    {
                        //Reset _duration
                        _duration = 0;

                        #region Populating _announcement ('_announce' shortly)
                        _announce = new Announce();

                        _announce.original_id = announcementId;
                        _announce.announce_date = DateTime.Now;
                        _announce.parser_site = _model.site;
                        HandleText();
                        HandleAddressRelatedStuff();
                        HandlePropertySpecifications();
                        HandleAnnouncementType();
                        HandleSellerName();
                        HandleOriginalDate();
                        await HandleImagesAsync(announcementId);

                        string[] mobileList = HandleMobile();
                        bool isRieltor = false;
                        if (VerifyOwnerTypeAndCheckIfRieltor())
                        {
                            isRieltor = true;
                            int checkNumberRieltorResult = await unitOfWork.CheckNumberRepository.CheckNumberForRieltorAsync(mobileList);

                            // Mobile exists in rieltor table
                            if (checkNumberRieltorResult > 0)
                            {
                                _announce.number_checked = true;
                            }
                            else
                            {
                                List<Rieltor> rieltors = new();

                                for (int i = 0; i < mobileList.Length; i++)
                                {
                                    rieltors.Add(new Rieltor { Phone = mobileList[i] });
                                }

                                await unitOfWork.RieltorRepository.BulkInsertAsync(rieltors);
                            }

                            var ids = await unitOfWork.Announces.GetAnnouncesByMobileListAsync(mobileList);
                            foreach (var id in ids)
                            {
                                await unitOfWork.Announces.ArendaAzUpdateAnnouncerAsync(new ArendaAzAnnouncerUpdateVM { Id = id, Announcer = 2 });
                                TelegramBotService.Sender($"emlak.az - modify correct announcer for id = {id}");
                            }

                            TelegramBotService.Sender($"emlak.az - announce modification completed");
                        }


                        int createdAnnounceId = await unitOfWork.Announces.CreateAsync(_announce);

                        HandleMap(createdAnnounceId);


                        if (!isRieltor)
                        {
                            //EMLAK - BAZASI
                            await _emlakBaza.CheckAsync(createdAnnounceId, mobileList);
                        }
                           
                        


                        #endregion

                        unitOfWork.Dispose();
                    }
                }
                else
                {
                    _duration++;
                }
            }
            catch (Exception e)
            {
                int line = (new StackTrace(e, true)).GetFrame(0).GetFileLineNumber();
                TelegramBotService.Sender($"Scraping - emlak.az - {line} - {e.Message}");
            }

        }
        protected override void HandleText()
        {
            if (_htmlDocument.DocumentNode.SelectSingleNode(".//div[@class='desc']//p") != null)
                _announce.text = _htmlDocument.DocumentNode.SelectSingleNode(".//div[@class='desc']//p").InnerText.Replace(" \n\n", string.Empty);
        }
        protected override void HandleAddressRelatedStuff()
        {
            //Address
            if (_htmlDocument.DocumentNode.SelectSingleNode(".//div[@class='map-address']//h4") != null)
            {
                _announce.address = _htmlDocument.DocumentNode.SelectSingleNode(".//div[@class='map-address']//h4").InnerText.Split("Ünvan: ")[1];
            }

            //City,Settlement,Region,Metro,Mark
            if (_htmlDocument.DocumentNode.SelectSingleNode(".//h1[@class='title']") != null)
            {
                if (_htmlDocument.DocumentNode.SelectSingleNode(".//h1[@class='title']").InnerText.EndsWith(" m."))
                {
                    var dictionaryMetrosNames = metrosNames.GetMetroNameAll();
                    foreach (var titleMetroName in dictionaryMetrosNames)
                    {
                        if (_htmlDocument.DocumentNode.SelectSingleNode(".//h1[@class='title']").InnerText.Contains(titleMetroName.Key))
                        {
                            _announce.metro_id = titleMetroName.Value;
                            break;
                        }
                    }
                }
                if (_htmlDocument.DocumentNode.SelectSingleNode(".//h1[@class='title']").InnerText.EndsWith(" r."))
                {
                    var dictionaryRegionsNames = regionsNames.GetRegionsNamesAll();
                    foreach (var regionName in dictionaryRegionsNames)
                    {
                        if (_htmlDocument.DocumentNode.SelectSingleNode(".//h1[@class='title']").InnerText.Contains(regionName.Key))
                        {
                            _announce.region_id = regionName.Value;
                            break;
                        }
                    }
                }
                var marksNamesOriginal = marksNames.GetMarksNamesAll();

                foreach (var markNameOriginal in marksNamesOriginal)
                {
                    if (_htmlDocument.DocumentNode.SelectSingleNode(".//h1[@class='title']").InnerText.Contains(markNameOriginal.Key))
                    {
                        _announce.mark = markNameOriginal.Value;
                        break;
                    }
                }
                var citiesNames = unitOfWork.CitiesRepository.GetAll();
                foreach (var cityName in citiesNames)
                {
                    if (_htmlDocument.DocumentNode.SelectSingleNode(".//h1[@class='title']").InnerText.Contains(cityName.name))
                    {
                        _announce.city_id = cityName.id;
                        break;
                    }
                }
                var settlementsNames = settlementNames.GetSettlementsNamesAll();
                foreach (var settlementName in settlementsNames)
                {
                    if (_htmlDocument.DocumentNode.SelectSingleNode(".//h1[@class='title']").InnerText.Contains(settlementName.Key))
                    {
                        _announce.settlement_id = settlementName.Value;
                        break;
                    }
                }
            }
        }
        protected override void HandlePropertySpecifications()
        {
            foreach (var item in _htmlDocument.DocumentNode.SelectNodes(".//dl[@class='technical-characteristics']//dd"))
            {
                if (item != null)
                {
                    if (item.InnerText.StartsWith("Əmlakın növü"))
                        _announce.property_type = typeOfProperty.GetTypeOfProperty(item.InnerText);
                    if (item.InnerText.StartsWith("Sahə"))
                    {
                        Regex regex = new Regex("\\d+");
                        var space = regex.Match(item.InnerText).ToString();
                        _announce.space = space;

                    }
                    if (item.InnerText.StartsWith("Otaqların sayı"))
                        _announce.floor_count = Int32.Parse(item.LastChild.InnerText);
                    if (item.InnerText.StartsWith("Otaqların sayı"))
                        _announce.room_count = Int32.Parse(item.LastChild.InnerText);
                    if (item.InnerText.StartsWith("Yerləşdiyi mərtəbə"))
                        _announce.current_floor = Int32.Parse(item.LastChild.InnerText);
                    if (item.InnerText.StartsWith("Mərtəbə sayı"))
                        _announce.floor_count = Int32.Parse(item.LastChild.InnerText);
                    if (item.InnerText.StartsWith("Təmiri"))
                    {
                        if (item.LastChild.InnerText == "Təmirsiz")
                            _announce.repair = false;
                        else
                            _announce.repair = true;
                    }
                    if (item.InnerText.StartsWith("Sənədin tipi"))
                    {
                        if (item.LastChild.InnerText == "Çıxarış (Kupça)")
                            _announce.document = 1;
                    }

                }
            }


            if (_htmlDocument.DocumentNode.SelectSingleNode(".//span[@class='m']") != null)
                _announce.price = Int32.Parse(_htmlDocument.DocumentNode.SelectSingleNode(".//span[@class='m']").InnerText.Replace(" ", string.Empty));
        }
        protected override void HandleAnnouncementType()
        {
            if (_htmlDocument.DocumentNode.SelectSingleNode(".//h1[@class='title']") != null)
            {
                _announce.announce_type = typeOfProperty.GetTitleOfProperty(_htmlDocument.DocumentNode.SelectSingleNode(".//h1[@class='title']").InnerText);
            }
        }
        protected override void HandleSellerName()
        {
            _announce.name = _htmlDocument.DocumentNode.SelectSingleNode(".//p[@class='name-seller']").FirstChild.InnerText.Trim();
        }
        protected override void HandleOriginalDate()
        {
            if (_htmlDocument.DocumentNode.SelectSingleNode(".//span[@class='date']//strong") != null)
                _announce.original_date = _htmlDocument.DocumentNode.SelectSingleNode(".//span[@class='date']//strong").InnerText;
        }
        protected override async Task HandleImagesAsync(int id)
        {
            _announce.cover = $@"EmlakAz/{DateTime.Now.Year}/{DateTime.Now.Month}/{id}/Thumb.jpg";

            var filePath = $@"EmlakAz/{DateTime.Now.Year}/{DateTime.Now.Month}/{id}/";
            var images = _uploader.ImageDownloaderAsync(_htmlDocument, id.ToString(), filePath);
            _announce.logo_images = JsonSerializer.Serialize(await images);
        }
        protected override string[] HandleMobile()
        {
            string mobileregex = _htmlDocument.DocumentNode.SelectSingleNode(".//p[@class='phone']").InnerText;

            StringBuilder mobile = new StringBuilder(mobileregex);
            var charsToRemove = new string[] { "(", ")", "-", ".", " " };
            foreach (var c in charsToRemove)
            {
                mobile = mobile.Replace(c, string.Empty);
            }

            _announce.mobile = mobile.ToString();

            return mobile.ToString().Split(',');
        }
        protected override bool ShouldScrapingStop(int _duration)
        {
            return (_duration >= maxRequest) ? true : false;
        }
        protected override void StopScraping(int id)
        {
            _model.last_id = id - maxRequest;
            unitOfWork.ParserAnnounceRepository.Update(_model);
            _duration = 0;
            TelegramBotService.Sender($"emlak.az limited");
        }

        private bool VerifyOwnerTypeAndCheckIfRieltor()
        {
            bool result = true;
            var type = _htmlDocument.DocumentNode.SelectNodes(".//p[@class='name-seller']//span")[0].InnerText;
            if (type == "(Vasitəçi)")
            {
                _announce.announcer = 2;
            }
            else if (type == "(Mülkiyyətçi)")
            {
                _announce.announcer = 1;
                result = false;
            }

            return result;
        }
        public void HandleMap(int _announceId)
        {
            string mapCordinats = null;
            if (_htmlDocument.GetElementbyId("google_map").GetAttributeValue("value", "") != null)
            {
                var charsToRemoveMapCordinats = new string[] { "(", ")", " " };
                mapCordinats = _htmlDocument.GetElementbyId("google_map").GetAttributeValue("value", "");

                foreach (var c in charsToRemoveMapCordinats)
                {
                    mapCordinats = mapCordinats.Replace(c, string.Empty);
                }

                var coordinatesArray = mapCordinats.Split(",");

                string latitude = coordinatesArray[0];
                string longitude = coordinatesArray[1];

                var _announceCoordinates = new AnnounceCoordinates
                {
                    longitude = Convert.ToDecimal(longitude),
                    latitude = Convert.ToDecimal(latitude),
                    announce_id = _announceId
                };

                unitOfWork.MarkRepository.CreateAnnounceCoordinates(_announceCoordinates);


            }
            if (mapCordinats != null)
            {
                var marks = unitOfWork.MarkRepository.GetMarks(mapCordinats);
                foreach (var item in marks)
                {
                    var _announceMark = new AnnounceMark { announce_id = _announceId, mark_id = item.id };
                    unitOfWork.MarkRepository.Create(_announceMark);
                }

            }

        }

        ~EmlakAzParser()
        {
            _httpClient.Dispose();
            unitOfWork.Dispose();
        }
    } // class end

}
