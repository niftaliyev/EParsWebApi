using HtmlAgilityPack;
using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using WebApi.Models;
using WebApi.Repository;
using WebApi.Services.TapAz.Interfaces;

namespace WebApi.Services.TapAz
{
    public class TapAzParser
    {
        private readonly EmlakBaza _emlakBaza;
        private readonly TapAzImageUploader _uploader;
        private readonly HttpClientCreater clientCreater;
        private static bool isActive = false;
        private readonly UnitOfWork unitOfWork;
        private readonly ITypeOfPropertyTapAz typeOfPropertyTapAz;
        private readonly TapAzMetrosNames metrosNames;
        private readonly TapAzSettlementsNames settlementsNames;
        private readonly TapAzRegionsNames regions;

        private HttpClient _httpClient;
        HttpResponseMessage header;
        public int maxRequest = 200;
        static string[] proxies = SingletonProxyServersIp.Instance;

        public TapAzParser(EmlakBaza emlakBaza, 
            TapAzImageUploader uploader, 
            HttpClientCreater clientCreater, 
            UnitOfWork unitOfWork,
            ITypeOfPropertyTapAz typeOfPropertyTapAz,
            TapAzMetrosNames metrosNames,
            TapAzSettlementsNames settlementsNames,
            TapAzRegionsNames regions)
        {
            this._emlakBaza = emlakBaza;
            _uploader = uploader;
            this.clientCreater = clientCreater;
            this.unitOfWork = unitOfWork;
            this.typeOfPropertyTapAz = typeOfPropertyTapAz;
            this.metrosNames = metrosNames;
            this.settlementsNames = settlementsNames;
            this.regions = regions;
            _httpClient = clientCreater.Create(proxies[0]);
            Console.WriteLine(proxies[0]);
        }
        public async Task TapAzPars()
        {
            var model = unitOfWork.ParserAnnounceRepository.GetBySiteName("https://tap.az");            
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
                            Console.WriteLine(x);

                            try
                            {
                                Console.WriteLine(model.site);

                                Uri myUri = new Uri($"{model.site}/elanlar/-/-/{++id}", UriKind.Absolute);
                                header = await _httpClient.GetAsync(myUri);
                                var url = header.RequestMessage.RequestUri.AbsoluteUri;
                                count++;
                                HtmlDocument doc = new HtmlDocument();

                                if (header.RequestMessage.RequestUri.ToString().StartsWith("https://tap.az/elanlar/dasinmaz-emlak/"))
                                {
                                    var response = await _httpClient.GetAsync(url);
                                    Console.WriteLine(response.StatusCode.ToString());


                                    if (response.IsSuccessStatusCode)
                                    {
                                        var html = await response.Content.ReadAsStringAsync();
                                        if (!string.IsNullOrEmpty(html))
                                        {
                                            doc.LoadHtml(html);
                                            Announce announce = new Announce();

                                            if (doc.DocumentNode.SelectSingleNode(".//a[@class='phone']") != null)
                                            {
                                                if (doc.DocumentNode.SelectSingleNode(".//span[@class='price-val']") != null)
                                                    announce.price = Int32.Parse(doc.DocumentNode.SelectSingleNode(".//span[@class='price-val']").InnerText.Replace(" ", ""));

                                                //if (doc.DocumentNode.SelectSingleNode(".//div[@class='title-container']//h1") != null)
                                                //announce.name = doc.DocumentNode.SelectSingleNode(".//div[@class='title-container']//h1").InnerText;
                                                if (doc.DocumentNode.SelectSingleNode(".//div[@class='lot-text']//p") != null)
                                                    announce.text = doc.DocumentNode.SelectSingleNode(".//div[@class='lot-text']//p").InnerText;


                                                string mobileregex = doc.DocumentNode.SelectSingleNode(".//a[@class='phone']").InnerText;

                                                if (mobileregex != null)
                                                {
                                                    var charsToRemove = new string[] { "(", ")", "-", ".", " " };
                                                    foreach (var c in charsToRemove)
                                                    {
                                                        mobileregex = mobileregex.Replace(c, string.Empty);
                                                    }

                                                    if (mobileregex != null)
                                                        announce.mobile = mobileregex;


                                                }
                                                Console.WriteLine(doc.DocumentNode.SelectSingleNode(".//div[@class='title-container']//h1").InnerText);
                                                Console.WriteLine(mobileregex);

                                                if (doc.DocumentNode.SelectNodes(".//div[@class='lot-info']/p")[2] != null)
                                                {
                                                    if (doc.DocumentNode.SelectNodes(".//div[@class='lot-info']/p")[2].InnerText.Contains("Bugün"))
                                                        announce.original_date = DateTime.Now.ToShortDateString();

                                                    if (doc.DocumentNode.SelectNodes(".//div[@class='lot-info']/p")[2].InnerText.Contains("Dünən"))
                                                        announce.original_date = DateTime.Now.AddDays(-1).ToShortDateString();
                                                    else
                                                        if (doc.DocumentNode.SelectNodes(".//div[@class='lot-info']/p")[2].InnerText.Replace("Yeniləndi: ", "") != null)
                                                        announce.original_date = doc.DocumentNode.SelectNodes(".//div[@class='lot-info']/p")[2].InnerText.Replace("Yeniləndi: ", "");
                                                }


                                                var countPropertyName = doc.DocumentNode.SelectNodes(".//td[@class='property-name']").Count;
                                                for (int i = 0; i < countPropertyName; i++)
                                                {
                                                    if (doc.DocumentNode.SelectNodes(".//td[@class='property-name']")[i].InnerText == "Otaq sayı")
                                                        announce.room_count = Int32.Parse(doc.DocumentNode.SelectNodes(".//td[@class='property-value']")[i].InnerText);
                                                    ///////////// dev
                                                    if (doc.DocumentNode.SelectNodes(".//td[@class='property-name']")[i].InnerText == "Binanın tipi" || doc.DocumentNode.SelectNodes(".//td[@class='property-name']")[i].InnerText == "Əmlakın növü")
                                                        announce.property_type = typeOfPropertyTapAz.GetTitleOfProperty(doc.DocumentNode.SelectNodes(".//td[@class='property-value']")[i].InnerText);
                                                    if (doc.DocumentNode.SelectNodes(".//td[@class='property-name']")[i].InnerText == "Elanın tipi")
                                                        announce.announce_type = typeOfPropertyTapAz.GetTypeOfProperty(doc.DocumentNode.SelectNodes(".//td[@class='property-value']")[i].InnerText);
                                                    if (doc.DocumentNode.SelectNodes(".//td[@class='property-name']")[i].InnerText == "Şəhər")
                                                    {
                                                        var cities = unitOfWork.CitiesRepository.GetAll();
                                                        foreach (var city in cities)
                                                        {
                                                            if (doc.DocumentNode.SelectNodes(".//td[@class='property-value']")[i].InnerText.Contains(city.name))
                                                            {
                                                                announce.city_id = city.id;
                                                                break;

                                                            }
                                                        }

                                                    }

                                                    if (doc.DocumentNode.SelectNodes(".//td[@class='property-name']")[i].InnerText == "Yerləşdirmə yeri" || doc.DocumentNode.SelectNodes(".//td[@class='property-name']")[i].InnerText == "Yerləşmə yeri")
                                                    {
                                                        Console.WriteLine("Yerləşdirmə");
                                                        announce.address = doc.DocumentNode.SelectNodes(".//td[@class='property-value']")[i].InnerText;
                                                        if (doc.DocumentNode.SelectNodes(".//td[@class='property-value']")[i].InnerText.EndsWith(" m."))
                                                        {
                                                            var resultMetrosNames = metrosNames.GetMetroNameAll();

                                                            foreach (var metroName in resultMetrosNames)
                                                            {
                                                                if (doc.DocumentNode.SelectNodes(".//td[@class='property-value']")[i].InnerText == metroName.Key)
                                                                {
                                                                    announce.metro_id = metroName.Value;
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                        else if (doc.DocumentNode.SelectNodes(".//td[@class='property-value']")[i].InnerText.EndsWith(" qəs."))
                                                        {
                                                            var settlements = settlementsNames.GetSettlementsNamesAll();
                                                            foreach (var item in settlements)
                                                            {
                                                                if (item.Key == doc.DocumentNode.SelectNodes(".//td[@class='property-value']")[i].InnerText)
                                                                {
                                                                    announce.settlement_id = item.Value;
                                                                    break;
                                                                }
                                                            }
                                                            Console.WriteLine("qesebe******************");
                                                        }
                                                        else if (doc.DocumentNode.SelectNodes(".//td[@class='property-value']")[i].InnerText.EndsWith(" ş."))
                                                        {
                                                            var cities = unitOfWork.CitiesRepository.GetAll();
                                                            
                                                            foreach (var item in cities)
                                                            {
                                                                if (doc.DocumentNode.SelectNodes(".//td[@class='property-value']")[i].InnerText == item.name)
                                                                {
                                                                    announce.city_id = item.id;
                                                                    break;
                                                                }
                                                                if ("Xırdalan ş." == doc.DocumentNode.SelectNodes(".//td[@class='property-value']")[i].InnerText)
                                                                {
                                                                    announce.city_id = 26;
                                                                    break;
                                                                }
                                                            }
                                                            Console.WriteLine("Seher******************");
                                                        }
                                                        else if (doc.DocumentNode.SelectNodes(".//td[@class='property-value']")[i].InnerText.EndsWith(" r."))
                                                        {
                                                            var regionsNames = regions.GetRegionsNamesAll();

                                                            foreach (var item in regionsNames)
                                                            {
                                                                if (item.Key == doc.DocumentNode.SelectNodes(".//td[@class='property-value']")[i].InnerText)
                                                                {
                                                                    announce.region_id = item.Value;
                                                                    break;
                                                                }
                                                            }
                                                            Console.WriteLine("Rayon******************");
                                                        }
                                                        else if (doc.DocumentNode.SelectNodes(".//td[@class='property-value']")[i].InnerText.EndsWith(" pr."))
                                                        {
                                                            //Console.WriteLine(doc.DocumentNode.SelectNodes(".//td[@class='property-value']")[i].InnerText);
                                                            announce.address = doc.DocumentNode.SelectNodes(".//td[@class='property-value']")[i].InnerText;
                                                            Console.WriteLine("Prospekt******************");
                                                        }
                                                    }


                                                    if (doc.DocumentNode.SelectNodes(".//td[@class='property-name']")[i].InnerText.StartsWith("Sahə"))
                                                        announce.space = Int32.Parse(doc.DocumentNode.SelectNodes(".//td[@class='property-value']")[i].InnerText);

                                                    /////yerlesme yeri
                                                    //if (doc.DocumentNode.SelectNodes(".//td[@class='property-name']")[i].InnerText == ("Yerləşmə yeri"))
                                                    //    announce.space = Int32.Parse(doc.DocumentNode.SelectNodes(".//td[@class='property-value']")[i].InnerText);
                                                }
                                                announce.original_id = id;
                                                announce.view_count = Int32.Parse(doc.DocumentNode.SelectNodes(".//div[@class='lot-info']/p")[1].InnerText.Replace("Baxışların sayı: ", ""));
                                                announce.parser_site = model.site;
                                                announce.announce_date = DateTime.Now;

                                                Console.WriteLine(proxies[x]);

                                                ///////////////////IMAGES
                                                var filePath = $@"TapAz/{DateTime.Now.Year}/{DateTime.Now.Month}/{id}/";
                                                var images = _uploader.ImageDownloader(doc, id.ToString(), filePath, _httpClient);

                                                var uri = new Uri(doc.DocumentNode.SelectNodes(".//div[@class='thumbnails']//a")[0].Attributes["href"].Value);

                                                var uriWithoutQuery = uri.GetLeftPart(UriPartial.Path);
                                                var fileExtension = Path.GetExtension(uriWithoutQuery);
                                                announce.cover = $@"TapAz/{DateTime.Now.Year}/{DateTime.Now.Month}/{id}/Thumb{fileExtension}";

                                                announce.logo_images = JsonSerializer.Serialize(await images);
                                                duration = 0;

                                                bool checkedNumber = false;
                                                var numberList = mobileregex.Split(',');
                                                var checkNumberRieltorResult = unitOfWork.CheckNumberRepository.CheckNumberForRieltor(numberList);
                                                if (checkNumberRieltorResult > 0)
                                                {
                                                    Console.WriteLine("FIND IN RIELTOR bASE TAP.AZ");

                                                    announce.announcer = checkNumberRieltorResult;
                                                    announce.number_checked = true;
                                                    checkedNumber = true;
                                                    Console.WriteLine("Checked");

                                                }

                                                await unitOfWork.Announces.Create(announce);

                                                if (checkedNumber == false)
                                                {
                                                    Console.WriteLine("Find in emlak-baza tAP.aZ");

                                                    //EMLAK - BAZASI
                                                    await _emlakBaza.CheckAsync(_httpClient, id, numberList);
                                                }
                                            }

                                        }
                                    }

                                }// emlak if end

                                Console.WriteLine(header.StatusCode);
                                if (header.StatusCode.ToString() == "NotFound")
                                {
                                    duration++;
                                }
                                if (header.StatusCode.ToString() == "OK")
                                {
                                    if (doc.DocumentNode.SelectSingleNode(".//a[@class='phone']") == null)
                                    {
                                        duration++;
                                    }
                                }
                                if (duration >= maxRequest)
                                {
                                    model.last_id = (id - maxRequest);
                                    Console.WriteLine("******** END **********");
                                    isActive = false;
                                    unitOfWork.ParserAnnounceRepository.Update(model);
                                    duration = 0;
                                    break;
                                }
                            }
                            catch (Exception e)
                            {

                                Console.WriteLine($"no connection {e.Message}");
                                count = 10;
                            }
                        } // while end
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }

                }

            }

        }
    }
}
