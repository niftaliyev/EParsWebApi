using HtmlAgilityPack;
using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebApi.Models;
using WebApi.Repository;

namespace WebApi.Services.DashinmazEmlak
{
    public class DashinmazEmlakParser
    {
        private readonly EmlakBaza _emlakBaza;
        private static bool isActive = false;
        private readonly UnitOfWork unitOfWork;
        private readonly DashinmazEmlakImageUploader _uploader;
        private readonly HttpClientCreater clientCreater;

        private HttpClient _httpClient;
        HttpResponseMessage header;
        public int maxRequest = 50;
        static string[] proxies = SingletonProxyServersIp.Instance;

        public DashinmazEmlakParser(EmlakBaza emlakBaza,
            UnitOfWork unitOfWork,
            HttpClient httpClient,
            DashinmazEmlakImageUploader uploader,
            HttpClientCreater clientCreater)
        {
            this._emlakBaza = emlakBaza;
            this.unitOfWork = unitOfWork;
            this.clientCreater = clientCreater;
            _httpClient = httpClient;
            _uploader = uploader;
        }

        public async Task DashinmazEmlakPars()
        {
            
            var model = unitOfWork.ParserAnnounceRepository.GetBySiteName("https://dashinmazemlak.az");

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
                               
                                Announce announce = new Announce();
                                header = await _httpClient.GetAsync($"{model.site}/az/satilir/{++id}.html");
                                string url = header.RequestMessage.RequestUri.AbsoluteUri;
                                count++;
                                var response = await _httpClient.GetAsync(url);
                                if (response.IsSuccessStatusCode)
                                {
                                    var html = await response.Content.ReadAsStringAsync();
                                    if (!string.IsNullOrEmpty(html))
                                    {
                                        HtmlDocument doc = new HtmlDocument();
                                        doc.LoadHtml(html);



                                        if (doc.DocumentNode.SelectSingleNode(".//h1[@class='elan_read_foto_h_right']").InnerText.Replace("Elanın №: ", "").Length > 1)
                                        {
                                            announce.original_id = id;
                                            announce.parser_site = model.site;
                                            announce.announce_date = DateTime.Now;
                                           
                                            var announceText = doc.DocumentNode.SelectSingleNode(".//div[@class='elan_read_content_text']").InnerText;
                                            if (announceText != null)
                                            {
                                                announce.text = announceText;
                                            }

                                            var announceName = doc.DocumentNode.SelectSingleNode(".//div[@class='elan_read_content_div3']//table[1]//tr[1]//td[3]")
                                                                               .InnerText.Split(" ( Vasitəçi )")[0];

                                            if (announceName != null)
                                            {
                                                announce.name = announceName;
                                            }


                                            var mobileNumber = doc.DocumentNode.SelectSingleNode(".//div[@class='elan_read_content_div3']//table[1]//tr[3]//td[3]")
                                                                                .InnerText;

                                            if (mobileNumber != null)
                                            {
                                                var charsToRemove = new string[] { "(", ")", "-", ".", " " };
                                                foreach (var c in charsToRemove)
                                                {
                                                    mobileNumber = mobileNumber.Replace(c, string.Empty);
                                                }

                                                if (mobileNumber != null)
                                                {
                                                    announce.mobile = mobileNumber;
                                                }

                                            }

                                            Regex regex = new Regex(@"(\/(\s([0-9]+\s)+))");
                                            var title = doc.DocumentNode.SelectSingleNode(".//h1[@class='elan_read_foto_h_left']").InnerText;
                                            var announcePrice = regex.Match(title).ToString().Split("/ ")[1].Replace(" ", string.Empty);

                                            announce.price = Int32.Parse(announcePrice);




                                            var allPropertiesKeys = doc.DocumentNode.SelectNodes(".//div[@class='elan_read_content_div2']//b");
                                            var allPropertiesValues = doc.DocumentNode.SelectNodes(".//div[@class='elan_read_content_div2']//tr//td[2]");

                                            for (int i = 0; i < allPropertiesKeys.Count; i++)
                                            {
                                                if (allPropertiesKeys[i].InnerText == "Elanın tarixi:")
                                                {
                                                    announce.original_date = allPropertiesValues[i].InnerText;
                                                }
                                                else if (allPropertiesKeys[i].InnerText == "Baxış sayı:")
                                                {
                                                    announce.view_count = Int32.Parse(allPropertiesValues[i].InnerText);
                                                }
                                                else if (allPropertiesKeys[i].InnerText == "Yerləşdiyi yer:")
                                                {
                                                    var locatedPlaces = doc.DocumentNode.SelectNodes(".//a[@class='yerleshme_yeri_link']");

                                                    // found out located places and add them to particular table (city , settlement , region)
                                                    foreach (var locatedPlace in locatedPlaces)
                                                    {
                                                        if (locatedPlace.InnerText.Contains(" m."))
                                                        {
                                                            var metros = unitOfWork.MetrosRepository.GetAll();

                                                            foreach (var metro in metros)
                                                            {
                                                                if (locatedPlace.InnerText.Contains(metro.name))
                                                                {
                                                                    announce.metro_id = metro.id;
                                                                }

                                                            }

                                                        }
                                                        else if (locatedPlace.InnerText.Contains(" r."))
                                                        {
                                                            var regions = unitOfWork.CitiesRepository.GetAllRegions();

                                                            foreach (var region in regions)
                                                            {
                                                                if (locatedPlace.InnerText.Contains(region.name))
                                                                {
                                                                    announce.region_id = region.id;
                                                                }
                                                            }
                                                        }
                                                        else if (locatedPlace.InnerText.Contains(" q."))
                                                        {
                                                            var settlements = unitOfWork.CitiesRepository.GetAllSettlement();

                                                            foreach (var settlement in settlements)
                                                            {
                                                                if (locatedPlace.InnerText.Contains(settlement.name))
                                                                {
                                                                    announce.settlement_id = settlement.id;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                else if (allPropertiesKeys[i].InnerText == "Kateqoriya:")
                                                {
                                                    //do it later
                                                    //announce.property_type = _typeOfProperty.GetTitleOfProperty(allPropertiesValues[i].InnerText);
                                                }
                                                else if (allPropertiesKeys[i].InnerText == "Mərtəbə:")
                                                {
                                                    announce.floor_count = Int32.Parse(allPropertiesValues[i].InnerText.Replace(" ", "").Split("/")[0]);
                                                    announce.current_floor = Int32.Parse(allPropertiesValues[i].InnerText.Replace(" ", "").Split("/")[1]);
                                                }
                                                else if (allPropertiesKeys[i].InnerText.StartsWith("Sahə"))
                                                {
                                                    announce.space = allPropertiesValues[i].InnerText.Replace(" m2", "");
                                                }
                                                else if (allPropertiesKeys[i].InnerText == "Otaq sayı:")
                                                {
                                                    announce.room_count = Int32.Parse(allPropertiesValues[i].InnerText.Replace(" otaqlı", ""));
                                                }
                                                else if (allPropertiesKeys[i].InnerText == "Kupça:")
                                                {
                                                    if (allPropertiesValues[i].InnerText == "Var")
                                                    {
                                                        announce.document = 1;
                                                    }
                                                    else if (allPropertiesValues[i].InnerText == "Yoxdur")
                                                    {
                                                        announce.document = 0;
                                                    }
                                                }
                                                else if (allPropertiesKeys[i].InnerText == "Ünvan:")
                                                {
                                                    announce.address = allPropertiesValues[i].InnerText;
                                                }

                                            }


                                            bool checkedNumber = false;
                                            var numberList = mobileNumber.Split(',');
                                            var checkNumberRieltorResult =await unitOfWork.CheckNumberRepository.CheckNumberForRieltorAsync(numberList);

                                            if (checkNumberRieltorResult > 0)
                                            {
                                                announce.number_checked = true;
                                                announce.announcer = checkNumberRieltorResult;
                                                checkedNumber = true;
                                            }




                                            var filePath = $@"DashinmazemlakAz/{DateTime.Now.Year}/{DateTime.Now.Month}/{id}/";
                                            var images = _uploader.ImageDownloader(doc, id.ToString(), filePath, _httpClient);

                                            var lnk = doc.DocumentNode.SelectNodes(".//a[@class='elan_foto_thumbnail_list_25']")[0].Attributes["href"].Value;
                                            var absoLink = $"https://dashinmazemlak.az{lnk}";
                                            var uri = new Uri(absoLink);

                                            var uriWithoutQuery = uri.GetLeftPart(UriPartial.Path);
                                            var fileExtension = Path.GetExtension(uriWithoutQuery);
                                            announce.cover = $@"DashinmazemlakAz/{DateTime.Now.Year}/{DateTime.Now.Month}/{id}/Thumb{fileExtension}";

                                            announce.logo_images = JsonSerializer.Serialize(await images);

                                            await unitOfWork.Announces.CreateAsync(announce);
                                            unitOfWork.Dispose();

                                            if (checkedNumber == false)
                                            {
                                                //EMLAK - BAZASI
                                                await _emlakBaza.CheckAsync(_httpClient, id, numberList);
                                            }
                                        }
                                        else
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
                                    isActive = false;
                                    duration = 0;
                                    unitOfWork.ParserAnnounceRepository.Update(model);
                                    TelegramBotService.Sender($"dashinmazemlak.az limited {maxRequest}");

                                    break;
                                }
                            }

                            catch (System.Exception e)
                            {
                                TelegramBotService.Sender($"dashinmazemlak.az exception {e.Message}");
                               
                            }
                        }
                    }
                    catch(Exception e)
                    {
                        TelegramBotService.Sender($"no connection dashinmazemlak.az {e.Message}");
                    }
                }

            }


        }
    }
}
