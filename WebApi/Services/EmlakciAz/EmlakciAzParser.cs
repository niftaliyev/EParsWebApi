using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebApi.Models;
using WebApi.Repository;
using WebApi.Services.EmlakciAz.Interfaces;

namespace WebApi.Services.EmlakciAz
{
    public class EmlakciAzParser
    {
        private readonly EmlakBazaWithProxy _emlakBaza;
        private static bool isActive = false;
        private readonly UnitOfWork unitOfWork;
        private readonly ITypeOfPropertyEmlakciAz typeOfProperty;
        private HttpClient _httpClient;
        private readonly EmlakciAzRegionsNames regionsNames;
        private readonly EmlakciAzMetrosNames metrosNames;
        private readonly EmlakciAzCountryNames countryNames;
        private readonly EmlakciAzSettlementNames settlementNames;
        private readonly EmlakciAzImageUploader imageUploader;
        HttpResponseMessage header;
        public int maxRequest = 50;

        public EmlakciAzParser(EmlakBazaWithProxy emlakBaza,
            UnitOfWork unitOfWork,
            ITypeOfPropertyEmlakciAz typeOfProperty,
            HttpClient httpClient,
            EmlakciAzRegionsNames regionsNames,
            EmlakciAzMetrosNames metrosNames,
            EmlakciAzCountryNames countryNames,
            EmlakciAzSettlementNames settlementNames,
            EmlakciAzImageUploader imageUploader)
        {
            this._emlakBaza = emlakBaza;
            this.unitOfWork = unitOfWork;
            this.typeOfProperty = typeOfProperty;
            this._httpClient = httpClient;
            this.regionsNames = regionsNames;
            this.metrosNames = metrosNames;
            this.countryNames = countryNames;
            this.settlementNames = settlementNames;
            this.imageUploader = imageUploader;
        }
        public async Task EmlakciAzPars()
        {
            var model = unitOfWork.ParserAnnounceRepository.GetBySiteName("https://emlakci.az");
            if (!isActive)
            {
                if (model.isActive)
                {
                    var id = model.last_id;

                    isActive = true;
                    int x = 0;
                    int count = 0;
                    int duration = 0;

                    while (true)
                    {
                        try
                        {
                            Uri myUri = new Uri($"{model.site}/elanlar/view/{++id}", UriKind.Absolute);
                            header = await _httpClient.GetAsync(myUri);
                            var url = header.RequestMessage.RequestUri.AbsoluteUri;
                            HtmlDocument doc = new HtmlDocument();

                            if (!header.RequestMessage.RequestUri.ToString().StartsWith("https://emlakci.az/index/index"))
                            {
                                var response = await _httpClient.GetAsync(url);


                                if (response.IsSuccessStatusCode)
                                {
                                    var html = await response.Content.ReadAsStringAsync();
                                    if (!string.IsNullOrEmpty(html) && doc.DocumentNode.SelectNodes(".//div[@class='elan_inner_connect_phone_ad']")[0] != null)
                                    {
                                        doc.LoadHtml(html);
                                        Announce announce = new Announce();

                                        announce.parser_site = model.site;
                                        if (doc.DocumentNode.SelectSingleNode(".//div[@class='elan_inner_info']") != null)
                                        {
                                            announce.text = doc.DocumentNode.SelectSingleNode(".//div[@class='elan_inner_info']").InnerText;
                                        }

                                        if (doc.DocumentNode.SelectSingleNode(".//div[@class='elan_inner_left_basliq']") != null)
                                        {
                                            announce.original_id = Int32.Parse(doc.DocumentNode.SelectSingleNode(".//div[@class='elan_inner_left_basliq']//label").InnerText.Replace("#", ""));
                                        }
                                        announce.name = doc.DocumentNode.SelectNodes(".//div[@class='elan_inner_connect_phone_ad']")[0]?.InnerText;
                                        announce.view_count = Int32.Parse(doc.DocumentNode.SelectSingleNode(".//div[@class='elan_inner_left_baxis']").LastChild.InnerText.Split(": ")[1]);
                                        var charsToRemove = new string[] { "(", ")", "-", ".", " " };
                                        List<string> numberList = new List<string>();
                                        StringBuilder numbers = new StringBuilder();
                                        for (int i = 1; i < doc.DocumentNode.SelectNodes(".//div[@class='elan_inner_connect_phone_ad']").Count; i++)
                                        {
                                            var number = doc.DocumentNode.SelectNodes(".//div[@class='elan_inner_connect_phone_ad']")[i]?.InnerText;
                                            foreach (var c in charsToRemove)
                                            {
                                                number = number.Replace(c, string.Empty);

                                            }
                                            if (number != "&nbsp;")
                                            {
                                                numberList.Add(number);
                                                numbers.Append(number);
                                            }
                                        }
                                        announce.mobile = numbers.ToString();

                                        if (doc.DocumentNode.SelectSingleNode(".//div[@class='elan_inner_right']") != null)
                                        {
                                            int xIndex = 0;
                                            foreach (var item in doc.DocumentNode.SelectNodes(".//div[@class='elan_inner_info_param_left']"))
                                            {
                                                if (item.InnerText == "Qiyməti:")
                                                    announce.price = Int32.Parse(doc.DocumentNode.SelectNodes(".//div[@class='elan_inner_info_param_right']")[xIndex].InnerText.Split(" ")[0]);
                                                else if (item.InnerText == "Əmlakın növü:")
                                                    announce.property_type = typeOfProperty.GetTitleOfProperty(doc.DocumentNode.SelectNodes(".//div[@class='elan_inner_info_param_right']")[xIndex].InnerText);
                                                else if (item.InnerText == "Sahəsi:")
                                                    announce.space = doc.DocumentNode.SelectNodes(".//div[@class='elan_inner_info_param_right']")[xIndex].InnerText.Split(" ")[0];
                                                else if (item.InnerText == "Otaq sayı:")
                                                    announce.room_count = Int32.Parse(doc.DocumentNode.SelectNodes(".//div[@class='elan_inner_info_param_right']")[xIndex].InnerText);
                                                else if (item.InnerText == "Ünvanı:")
                                                    announce.address = doc.DocumentNode.SelectNodes(".//div[@class='elan_inner_info_param_right']")[xIndex].InnerText;
                                                else if (item.InnerText == "Mərtəbə:")
                                                {
                                                    announce.current_floor = Int32.Parse(doc.DocumentNode.SelectNodes(".//div[@class='elan_inner_info_param_right']")[xIndex].InnerText.Split("/")[0]);
                                                    announce.floor_count = Int32.Parse(doc.DocumentNode.SelectNodes(".//div[@class='elan_inner_info_param_right']")[xIndex].InnerText.Split("/")[1]);
                                                }
                                                else if (item.InnerText == "Təmiri:")
                                                {
                                                    if (doc.DocumentNode.SelectNodes(".//div[@class='elan_inner_info_param_right']")[xIndex].InnerText == "Təmirli")
                                                        announce.repair = true;
                                                    else
                                                        announce.repair = false;
                                                }
                                                else if (item.InnerText == "Kupçası:")
                                                {
                                                    if (doc.DocumentNode.SelectNodes(".//div[@class='elan_inner_info_param_right']")[xIndex].InnerText == "Var")
                                                        announce.document = 1;
                                                    else
                                                        announce.document = 0;
                                                }
                                                xIndex++;

                                            }
                                        }

                                        announce.announce_date = DateTime.Now;
                                        announce.original_date = doc.DocumentNode.SelectSingleNode(".//div[@class='elan_inner_left_baxis']//span").InnerText;


                                        if (doc.DocumentNode.SelectSingleNode(".//div[@class='elan_inner_left_basliq']//span").InnerText == "SATIŞ")
                                            announce.announce_type = 2;
                                        else if (doc.DocumentNode.SelectSingleNode(".//div[@class='elan_inner_left_basliq']//span").InnerText == "İCARƏ (Aylıq)")
                                            announce.announce_type = 1;

                                        var regions = regionsNames.GetRegionsNamesAll();
                                        var metros = metrosNames.GetMetroNameAll();
                                        var cities = countryNames.GetCountryNames();
                                        var settlements = settlementNames.GetSettlementsNamesAll();

                                        var items = doc.DocumentNode.SelectNodes(".//div[@class='yerlesdiyi_yer_inner']//.//div[@class='unvanlari']");

                                        for (int i = 0; i < items.Count; i++)
                                        {
                                            if (i == 0)
                                            {
                                                foreach (var city in cities)
                                                {
                                                    if (city.Key == items[i].InnerText)
                                                        announce.city_id = city.Value;
                                                }
                                            }

                                            if (items[i].InnerText.EndsWith(" r."))
                                            {
                                                foreach (var region in regions)
                                                {
                                                    if (region.Key == items[i].InnerText)
                                                        announce.region_id = region.Value;
                                                }
                                            }
                                            if (items[i].InnerText.EndsWith(" m."))
                                            {
                                                foreach (var metro in metros)
                                                {
                                                    if (metro.Key == items[i].InnerText)
                                                        announce.metro_id = metro.Value;
                                                }
                                            }
                                            else
                                            {
                                                foreach (var settlement in settlements)
                                                {
                                                    if (settlement.Key == items[i].InnerText)
                                                        announce.settlement_id = settlement.Value;
                                                }
                                            }
                                        }

                                        bool checkedNumber = false;
                                        var checkNumberRieltorResult = unitOfWork.CheckNumberRepository.CheckNumberForRieltor(numberList.ToArray());
                                        if (checkNumberRieltorResult > 0)
                                        {
                                            Console.WriteLine("FIND IN RIELTOR bASE emlakci.AZ");

                                            announce.announcer = checkNumberRieltorResult;
                                            announce.number_checked = true;
                                            checkedNumber = true;
                                            Console.WriteLine("Checked");

                                        }

                                        /////////////////////////// ImageUploader //////////////////////////////
                                        announce.cover = $@"EmlakciAz/{DateTime.Now.Year}/{DateTime.Now.Month}/{id}/Thumb.jpg";

                                        var filePath = $@"EmlakciAz/{DateTime.Now.Year}/{DateTime.Now.Month}/{id}/";
                                        var images = await imageUploader.ImageDownloaderAsync(doc, filePath);
                                  
                                        if (images != null)
                                        {
                                            announce.logo_images = JsonSerializer.Serialize(images);
                                            Console.WriteLine("saved images emlakci");
                                        }


                                        await unitOfWork.Announces.Create(announce);
                                        unitOfWork.Dispose();

                                        if (checkedNumber == false)
                                        {
                                            Console.WriteLine("Find in emlak-baza emlakci.aZ");

                                            //EMLAK - BAZASI
                                            await _emlakBaza.CheckAsync(id, numberList.ToArray());
                                        }


                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine("Empty");
                                duration++;
                            }
                            if (duration >= maxRequest)
                            {
                                model.last_id = (id - maxRequest);
                                Console.WriteLine("******** END emlakci **********");
                                isActive = false;
                                unitOfWork.ParserAnnounceRepository.Update(model);
                                duration = 0;
                                break;
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            TelegramBotService.Sender($"end catch {e.Message}");

                        }
                    }
                }
            }
        }
    }
}
