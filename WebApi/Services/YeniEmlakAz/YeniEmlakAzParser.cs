using Google.Protobuf.WellKnownTypes;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebApi.Models;
using WebApi.Repository;
using WebApi.Services.YeniEmlakAz.Interfaces;
using WebApi.ViewModels;

namespace WebApi.Services.YeniEmlakAz
{
    public class YeniEmlakAzParser : SiteParser
    {
        private ParserAnnounce _model;
        private HtmlDocument _htmlDocument;
        private Announce _announce;
        public int maxRequest = 50;
        private int _duration = 0;
        private int count = 6;

        private HttpClient _httpClient;
        private readonly EmlakBazaWithProxy _emlakBaza;
        private readonly UnitOfWork unitOfWork;
        private readonly HttpClientCreater clientCreater;
        private readonly YeniEmlakAzParserImageUploader uploader;
        private readonly YeniEmlakAzMetrosNames metrosNames;
        private readonly YeniEmlakAzSettlementNames settlementNames;
        private readonly ITypeOfPropertyYeniEmlakAz propertyType;
        private readonly YeniEmlakAzCountryNames countryNames;
        private readonly YeniEmlakAzRegionsNames regionsNames;
        static string[] proxies = SingletonProxyServersIp.Instance;

        public YeniEmlakAzParser(EmlakBazaWithProxy emlakBaza,
                                UnitOfWork unitOfWork,
                                HttpClientCreater clientCreater,
                                YeniEmlakAzParserImageUploader uploader,
                                YeniEmlakAzMetrosNames metrosNames,
                                YeniEmlakAzSettlementNames settlementNames,
                                YeniEmlakAzCountryNames countryNames,
                                YeniEmlakAzRegionsNames regionsNames,
                                ITypeOfPropertyYeniEmlakAz propertyType
                                // HttpClient httpClient
                                )
        {
            _emlakBaza = emlakBaza;
            this.unitOfWork = unitOfWork;
            this.clientCreater = clientCreater;
            this.uploader = uploader;
            this.metrosNames = metrosNames;
            this.settlementNames = settlementNames;
            this.countryNames = countryNames;
            this.regionsNames = regionsNames;
            this.propertyType = propertyType;
            Random rnd = new Random();
            _httpClient = this.clientCreater.Create(proxies[rnd.Next(0, 99)]);
            //this.httpClient = httpClient;

        }

        public override async Task ParseSite()
        {
            try
            {
                _model = unitOfWork.ParserAnnounceRepository.GetBySiteName("https://yeniemlak.az");

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

                        var siteUri = new Uri($"{_model.site}/elan/{id}");
                        using var responseMessage = await _httpClient.GetAsync(siteUri);

                        count++;

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
                TelegramBotService.Sender($"Main - yeniemlak.az - {line} - {e.Message}");
            }

        }

        protected override void CreateHttpClientByIpAddressFromProxyList(int count, ref int x)
        {
            if (count >= 5)
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
            }

            return false;
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
                if (_htmlDocument.DocumentNode.SelectSingleNode(".//div[@class='text']") != null &&
              _htmlDocument.DocumentNode.SelectSingleNode(".//table[@class='msg']//h3") == null)
                {
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
                            TelegramBotService.Sender($"yeniemlak.az - modify correct announcer for id = {id}");
                        }


                        TelegramBotService.Sender($"yeniemlak.az - announce modification completed");
                    }

                    int createdAnnounceId = await unitOfWork.Announces.CreateAsync(_announce);

                    if(!isRieltor)
                    {
                        //EMLAK - BAZASI
                        await _emlakBaza.CheckAsync(createdAnnounceId, mobileList);
                    }

                 
                    #endregion

                    unitOfWork.Dispose();
                }
                else if (_htmlDocument.DocumentNode.SelectSingleNode(".//table[@class='msg']//h1") != null)
                {

                    if (_htmlDocument.DocumentNode.SelectSingleNode(".//table[@class='msg']//h1").InnerText.Trim() == "Bu kodlu elan mövcud deyil!")
                    {
                        _duration++;
                    }
                }
            }
            catch (Exception e)
            {
                int line = (new StackTrace(e, true)).GetFrame(0).GetFileLineNumber();
                TelegramBotService.Sender($"Scraping - yeniemlak.az - {line} - {e.Message}");
            }
           
        }
        protected override void HandleText()
        {
            _announce.text = _htmlDocument.DocumentNode.SelectSingleNode(".//div[@class='text']").InnerText;
        }
        protected override void HandleAddressRelatedStuff()
        {
            var address = _htmlDocument.DocumentNode.SelectNodes(".//div[@class='text']")[1];
            if (address is not null)
            {
                _announce.address = address.InnerText;
            }
            if (_htmlDocument.DocumentNode.SelectNodes(".//div[@class='params']") != null)
            {
                foreach (var item in _htmlDocument.DocumentNode.SelectNodes(".//div[@class='params']"))
                {
                    if (item.FirstChild.InnerText == "metro. ")
                    {
                        var metroNames = metrosNames.GetMerosNamesAll();
                        foreach (var metroName in metroNames)
                        {
                            if (metroName.Key == item.LastChild.InnerText)
                            {
                                _announce.metro_id = metroName.Value;
                                break;
                            }
                        }
                    }

                }
                bool cityCheck = true;
                bool settlementCheck = true;
                foreach (var item in _htmlDocument.DocumentNode.SelectNodes(".//div[@class='params']//b"))
                {
                    if (item.InnerText == "Abşeron")
                    {
                        _announce.region_id = 1;
                        _announce.city_id = 1;
                        cityCheck = false;
                        break;


                    }
                    var settlements = settlementNames.GetSettlementNamesAll();
                    if (settlementCheck)
                    {
                        foreach (var settlemenName in settlements)
                        {
                            if (item.InnerText == settlemenName.Key)
                            {
                                _announce.settlement_id = settlemenName.Value;
                                settlementCheck = false;
                                break;
                            }
                        }

                    }

                    if (cityCheck == true)
                    {
                        var namesCountry = countryNames.GetCountryNames();
                        foreach (var countryName in namesCountry)
                        {

                            if (item.InnerText == countryName.Key)
                            {
                                _announce.city_id = countryName.Value;
                                cityCheck = false;

                                break;
                            }
                        }
                    }
                    ////////// REGION
                    if (item.InnerText.Contains("rayon"))
                    {
                        var regions = regionsNames.GetRegionsNamesAll();
                        foreach (var region in regions)
                        {
                            if (region.Key == item.InnerText)
                            {
                                _announce.region_id = region.Value;
                                break;
                            }
                        }
                    }
                    if (settlementCheck)
                    {
                        if (item.InnerText.Contains("qəs.") || item.InnerText.Contains("qəsəbəsi") || item.InnerText.Contains("mikrorayon"))
                        {
                            foreach (var settlemenName in settlements)
                            {
                                if (settlemenName.Key == item.InnerText)
                                {
                                    _announce.settlement_id = settlemenName.Value;
                                    break;
                                }
                            }
                        }
                    }

                    if (item.InnerText.Contains(" ş."))
                    {
                        _announce.city_id = 26;
                    }
                }
            }
        }
        protected override void HandlePropertySpecifications()
        {
            if (_htmlDocument.DocumentNode.SelectSingleNode(".//emlak") != null)
            {
                var type = _htmlDocument.DocumentNode.SelectSingleNode(".//emlak").InnerText;
                if (type.Trim() != "Bina evi menzil")
                {
                    int propType = propertyType.GetTypeOfProperty(type);
                    _announce.property_type = propType;
                }
                else
                {
                    var propTypeText = _htmlDocument.DocumentNode.SelectSingleNode(".//emlak//following-sibling::text()[1]").InnerText.Split(" / ")[1];

                    int propType = propertyType.GetTypeOfProperty(propTypeText);
                    _announce.property_type = propType;
                }
            }

            //price
            if (_htmlDocument.DocumentNode.SelectSingleNode(".//div[@class='title']//price") != null)
            {
                _announce.price = Int32.Parse(_htmlDocument.DocumentNode.SelectSingleNode(".//div[@class='title']//price").InnerText);
            }

            //room,space,document(Kupça)
            if (_htmlDocument.DocumentNode.SelectNodes(".//div[@class='params']") != null)
            {
                foreach (var item in _htmlDocument.DocumentNode.SelectNodes(".//div[@class='params']"))
                {
                    if (item.InnerText.EndsWith(" otaq"))
                    {
                        _announce.room_count = Int32.Parse(item.FirstChild.InnerText);
                    }
                    if (item.InnerText.EndsWith(" m2"))
                    {
                        _announce.space = item.FirstChild.InnerText;
                    }

                    if (item.InnerText == "Kupça")
                    {
                        _announce.document = 1;
                    }
                }
            }

            //floor
            var floorInfo = _htmlDocument.DocumentNode.SelectNodes(".//div[@class='params'][3]//b");
            if (floorInfo.Count > 1)
            {
                bool isFloorCountEvaluated = false;
                foreach (var floor in floorInfo)
                {
                    if (!isFloorCountEvaluated)
                    {
                        _announce.floor_count = int.Parse(floor.InnerText);
                        isFloorCountEvaluated = true;
                    }
                    else
                    {
                        _announce.current_floor = int.Parse(floor.InnerText);
                    }

                }
            }

            //credit type
            if (_htmlDocument.DocumentNode.SelectSingleNode(".//div[@class='title']//kredit") != null)
            {
                _announce.kredit = 1;
            }
        }
        protected override void HandleAnnouncementType()
        {
            if (_htmlDocument.DocumentNode.SelectSingleNode(".//div[@class='title']//tip") != null)
            {
                if (_htmlDocument.DocumentNode.SelectSingleNode(".//div[@class='title']//tip").InnerText == "Günlük")
                {
                    _announce.announce_type = 3;
                }
                else if (_htmlDocument.DocumentNode.SelectSingleNode(".//div[@class='title']//tip").InnerText == "Kirayə")
                {
                    _announce.announce_type = 1;
                }
                else if (_htmlDocument.DocumentNode.SelectSingleNode(".//div[@class='title']//tip").InnerText == "Satılır")
                {
                    _announce.announce_type = 2;
                }
            }
        }
        protected override string[] HandleMobile()
        {
            StringBuilder numbers = new StringBuilder();
            if (_htmlDocument.DocumentNode.SelectNodes(".//div[@class='tel']") != null)
            {
                int counPhoneImages = _htmlDocument.DocumentNode.SelectNodes(".//div[@class='tel']//img").Count;
                string delimiter = "";

                for (int i = 0; i < counPhoneImages; i++)
                {
                    var numbersArr = _htmlDocument.DocumentNode.SelectNodes(".//div[@class='tel']//img")[i].Attributes["src"].Value.Split('/');
                    var index = numbersArr.Length;
                    numbers.Append(delimiter);
                    numbers.Append(numbersArr[index - 1]);
                    delimiter = ",";
                }
                _announce.mobile = numbers.ToString();

                return numbers.ToString().Split(',');
            }
            return Array.Empty<string>();
        }
        protected override void HandleSellerName()
        {
            _announce.name = _htmlDocument.DocumentNode.SelectSingleNode(".//div[@class='ad']").InnerText;
        }
        protected override async Task HandleImagesAsync(int id)
        {
            var filePath = $@"YeniemlakAz/{DateTime.Now.Year}/{DateTime.Now.Month}/{id}/";
            var images = uploader.ImageDownloader(_htmlDocument, filePath, _httpClient);

            _announce.logo_images = JsonSerializer.Serialize(await images);

            var uri = new Uri(_htmlDocument.DocumentNode.SelectNodes(".//div[@class='imgbox']//a")[0].Attributes["href"].Value);
            var uriWithoutQuery = uri.GetLeftPart(UriPartial.Path);
            var fileExtension = Path.GetExtension(uriWithoutQuery);

            _announce.cover = $@"YeniemlakAz/{DateTime.Now.Year}/{DateTime.Now.Month}/{id}/Thumb{fileExtension}";
        }
        protected override void HandleOriginalDate()
        {
            if (_htmlDocument.DocumentNode.SelectNodes(".//div[@class='title']//titem") != null)
            {
                foreach (var item1 in _htmlDocument.DocumentNode.SelectNodes(".//div[@class='title']//titem"))
                {

                    if (item1.InnerText.StartsWith("Tarix: "))
                    {
                        _announce.original_date = item1.LastChild.InnerText;
                    }                }
            }
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
            TelegramBotService.Sender($"yeniemlak.az limited");
        }
        private bool VerifyOwnerTypeAndCheckIfRieltor()
        {
            bool result = true;
            var type = _htmlDocument.DocumentNode.SelectSingleNode("//div[@class='elvrn']").InnerText.Trim();
            if (type == "Vasitəçi / Rieltor")
            {
                _announce.announcer = 2;
            }
            else if (type == "Əmlak sahibi")
            {
                _announce.announcer = 1;
                result = false;
            }

            return result;
        }
        ~YeniEmlakAzParser() 
        {
            _httpClient.Dispose();
            unitOfWork.Dispose();
        }
    }
}
