using HtmlAgilityPack;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebApi.Helpers;
using WebApi.Models;
using WebApi.Repository;
using WebApi.ViewModels;

namespace WebApi.Services.VipEmlakAz
{
    public class VipEmlakAzParser : SiteParser
    {
        private HttpClient _httpClient;
        private readonly EmlakBazaWithProxy _emlakBaza;
        private HtmlDocument _htmlDocument;
        private readonly UnitOfWork _unitOfWork;
        private readonly ITypeOfPropertyVipEmlakAz _propertyType;
        private readonly VipEmlakAzMetroNames _metrosNames;
        private readonly VipEmlakAzSettlementNames _settlementNames;
        private readonly VipEmlakAzImageUploader _imageUploader;
        static string[] proxies = SingletonProxyServersIp.Instance;
        private readonly HttpClientCreater clientCreater;
        public const int maxRequest = 10;
        private int duration = 0;
        private int count = 0;
        private Announce announce;
        private ParserAnnounce _model;

        public VipEmlakAzParser(
                                UnitOfWork unitOfWork,
                                EmlakBazaWithProxy emlakBaza,
                                ITypeOfPropertyVipEmlakAz propertyType,
                                VipEmlakAzSettlementNames settlementNames,
                                VipEmlakAzMetroNames metrosNames,
                                VipEmlakAzImageUploader imageUploader,
                                HttpClientCreater httpClientCreater
                               //HttpClient httpClient
                               )
        {
            _unitOfWork = unitOfWork;
            _emlakBaza = emlakBaza;
            _propertyType = propertyType;
            _settlementNames = settlementNames;
            _metrosNames = metrosNames;
            _imageUploader = imageUploader;
            clientCreater = httpClientCreater;
            //_httpClient = httpClient;
            Random rnd = new Random();
            _httpClient = this.clientCreater.Create(proxies[rnd.Next(0, 99)]);

        }

        public override async Task ParseSite()
        {
            try
            {
                _model = _unitOfWork.ParserAnnounceRepository.GetBySiteName("https://vipemlak.az");

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

                        var siteUri = new Uri($"{_model.site}/masazir-qesebesi-{id}.html", UriKind.Absolute);
                        using var responseMessage = await _httpClient.GetAsync(siteUri);

                        //Increase same IP usage count
                        count++;

                        if (responseMessage.IsSuccessStatusCode)
                        {
                            await PopulateDocumentWithResponseMessageAsync(responseMessage);
                            
                            await HandleScrapingAsync(id);
                        }
                        else if (responseMessage.StatusCode == HttpStatusCode.NotFound)
                        {
                            duration++;
                        }

                        if (ShouldScrapingStop(duration))
                        {
                            StopScraping(id);
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                int line = (new StackTrace(e, true)).GetFrame(0).GetFileLineNumber();
                TelegramBotService.Sender($"vipemlak.az - {line} - {e.Message}");
            }
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
        protected override async Task<bool> IsAnnouncementAvailableInDbAsync(string siteUrl, int id)
        {
            var searchViewModel = new AnnounceSearchViewModel
            {
                ParserSite = siteUrl,
                OriginalId = id
            };

            if (await _unitOfWork.Announces.IsAnnounceValidAsync(searchViewModel))
            {
                return true;
            };

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
                if (_htmlDocument.DocumentNode.SelectSingleNode(".//div[@class='breadcrumb']//a[1]") != null)
                {
                    if (_htmlDocument.DocumentNode.SelectSingleNode(".//div[@class='breadcrumb']//a[1]").InnerText == "Dasinmaz emlak")
                    {
                        //Reset duration
                        duration = 0;

                        #region Populating announcement ('announce' shortly)
                        announce = new Announce();
                        announce.original_id = announcementId;
                        announce.announce_date = DateTime.Now;
                        announce.parser_site = _model.site;
                        HandleText();
                        HandleAddressRelatedStuff();
                        HandlePropertySpecifications();
                        HandleAnnouncementType();
                        HandleSellerName();
                        HandleOriginalDate();

                        await HandleImagesAsync(announcementId);

                        string[] mobileList = HandleMobile();


                        int checkNumberRieltorResult = await _unitOfWork.CheckNumberRepository.CheckNumberForRieltorAsync(mobileList);

                        // Mobile exists in rieltor table
                        if (checkNumberRieltorResult > 0)
                        {
                            announce.announcer = checkNumberRieltorResult;
                            announce.number_checked = true;
                        }

                        int createdAnnounceId = await _unitOfWork.Announces.CreateAsync(announce);

                        // Mobile doesn't exists in rieltor table
                        if (checkNumberRieltorResult < 0)
                        {
                            //EMLAK - BAZASI
                            await _emlakBaza.CheckAsync(createdAnnounceId, mobileList);
                        }
                        #endregion

                        _unitOfWork.Dispose();
                    }
                }
                else
                {
                    duration++;
                }
            }
            catch (Exception e)
            {

                int line = (new StackTrace(e, true)).GetFrame(0).GetFileLineNumber();
                TelegramBotService.Sender($"Scraping - vipemlak.az - {line} - {e.Message}");
            }
          
        }

        #region ScrapingMethods
        protected override void HandleText()
        {
            var text = _htmlDocument.DocumentNode.SelectSingleNode(".//div[@id='openhalf']//div[2]").InnerText;
            announce.text = text;
        }

        protected override void HandleAddressRelatedStuff()
        {
            var address = _htmlDocument.DocumentNode.SelectNodes("//*[@id='openhalf']/div[11]/text()");

            for (int i = 0; i < address.Count; i++)
            {
                //Address
                if (i == address.Count - 1)
                {
                    announce.address = address[i].InnerText;
                }
                //City
                else if (address[i].InnerText.ToLower().Contains("şəhəri"))
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

                //Region
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

                //Settlement
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

                //Metro
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
        }

        protected override void HandlePropertySpecifications()
        {
            var allPropertiesKeys = _htmlDocument.DocumentNode.SelectNodes(".//div[@class='infotd']//b");
            var allPropertiesValues = _htmlDocument.DocumentNode.SelectNodes(".//div[@class='infotd2']");


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
                    var price = _htmlDocument.DocumentNode.SelectSingleNode(".//span[@class='pricecolor']").InnerText.Replace(" Azn", "").Replace(" ", "");
                    announce.price = Int32.Parse(price);
                }
            }
        }

        protected override void HandleAnnouncementType()
        {
            if (_htmlDocument.DocumentNode.SelectSingleNode(".//span[@class='pricecolor']//following-sibling::text()[1]") != null)
            {
                var announceTypeText = _htmlDocument.DocumentNode.SelectSingleNode(".//span[@class='pricecolor']//following-sibling::text()[1]").InnerText.Split("/ ")[1];
                if (announceTypeText == "Gün" || announceTypeText == "Ay")
                {
                    announce.announce_type = 1;
                }

            }
            else
            {
                announce.announce_type = 2;
            }
        }

        protected override string[] HandleMobile()
        {
            var mobileField = _htmlDocument.DocumentNode.SelectSingleNode(".//div[@id='telshow']");

            if (mobileField == null) 
            {
                return Array.Empty<string>();
            }

            StringBuilder mobile = new StringBuilder(_htmlDocument.DocumentNode.SelectSingleNode(".//div[@id='telshow']").InnerText);
            var mobileResult = MobileEditorHelper.Edit(mobile);

            announce.mobile = mobileResult;

            return mobileResult.Split(',');
        }

        protected override void HandleSellerName()
        {
            var name = _htmlDocument.DocumentNode.SelectSingleNode(".//div[@class='infocontact']").InnerText.Trim().Split("\r\n\t\t ")[1];
            if (name != null)
                announce.name = name.Trim();
        }

        protected override async Task HandleImagesAsync(int id)
        {
            var filePath = $@"VipEmlakAz/{DateTime.Now.Year}/{DateTime.Now.Month}/{id}/";
            var images = _imageUploader.ImageDownloader(_htmlDocument, id.ToString(), filePath, _httpClient);

            var thumbLink = _htmlDocument.DocumentNode.SelectNodes(".//div[@id='picsopen']//div//a//img")[0].Attributes["src"].Value;
            var absoluteThumbLink = $"https://vipemlak.az{thumbLink}";

            var uri = new Uri(absoluteThumbLink);

            var uriWithoutQuery = uri.GetLeftPart(UriPartial.Path);
            var fileExtension = Path.GetExtension(uriWithoutQuery);

            announce.cover = $@"VipEmlakAz/{DateTime.Now.Year}/{DateTime.Now.Month}/{id}/Thumb{fileExtension}";
            announce.logo_images = JsonSerializer.Serialize(await images);
        }

        protected override void HandleOriginalDate()
        {
            var original_date = _htmlDocument.DocumentNode.SelectSingleNode(".//span[@class='viewsbb clear']").InnerText.Trim();
            announce.original_date = original_date.Split(" ")[1];
        }

        protected override bool ShouldScrapingStop(int duration)
        {
            return (duration >= maxRequest) ? true : false;
        }

        protected override void StopScraping(int id)
        {
            _model.last_id = id - maxRequest;
            _unitOfWork.ParserAnnounceRepository.Update(_model);
            duration = 0;
            _httpClient.Dispose();
            _unitOfWork.Dispose();
            TelegramBotService.Sender($"vipemlak.az limited");
        }
        #endregion

    }
}
