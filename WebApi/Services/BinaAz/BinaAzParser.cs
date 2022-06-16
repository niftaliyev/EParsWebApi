using HtmlAgilityPack;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebApi.Models;
using WebApi.Repository;

namespace WebApi.Services.BinaAz
{
    public class BinaAzParser
    {
        private readonly EmlakBaza _emlakBaza;
        private static bool isActive = false;
        private readonly UnitOfWork unitOfWork;
        private readonly HttpClientCreater clientCreater;

        private HttpClient _httpClient;
        private readonly BinaAzParserImageUploader imageUploader;
        HttpResponseMessage header;
        public int maxRequest = 50;
        static string[] proxies = SingletonProxyServersIp.Instance;

        public BinaAzParser(EmlakBaza emlakBaza,
            HttpClientCreater clientCreater,
            UnitOfWork unitOfWork,
            BinaAzParserImageUploader imageUploader,
            HttpClient httpClient)
        {
            this._emlakBaza = emlakBaza;
            this.clientCreater = clientCreater;
            this.unitOfWork = unitOfWork;
            this.imageUploader = imageUploader;
            _httpClient = clientCreater.Create(proxies[0]);
            //_httpClient = httpClient;
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
                                if (x >= 350)
                                    x = 0;

                                _httpClient = clientCreater.Create(proxies[x]);
                                count = 0;
                            }

                            try
                            {

                                Uri myUri = new Uri($"{model.site}/items/{++id}", UriKind.Absolute);



                                header = await _httpClient.GetAsync(myUri);

                                var url = header.RequestMessage.RequestUri.AbsoluteUri;
                                count++;
                                HtmlDocument doc = new HtmlDocument();

                                var response = await _httpClient.GetAsync(url);

                                Console.WriteLine(0);
                                if (response.IsSuccessStatusCode)
                                {
                                    Console.WriteLine(1);
                                    var html = await response.Content.ReadAsStringAsync();
                                    if (!string.IsNullOrEmpty(html))
                                    {
                                        //Console.WriteLine(doc.DocumentNode.SelectSingleNode(".//section[@class='contacts']").FirstChild.SelectSingleNode(".//div[@class=name']//span").InnerText);
                                        doc.LoadHtml(html);
                                        Announce announce = new Announce();

                                        var address = doc.DocumentNode.SelectSingleNode(".//div[@class='map_address']").InnerText.Split("Ünvan: ")[1];
                                        announce.address = address;
                                        announce.original_id = id;
                                        announce.parser_site = model.site;
                                        announce.price = Int32.Parse(doc.DocumentNode.SelectSingleNode(".//span[@class='price-val']").InnerText.Replace(" ", ""));
                                        announce.announce_date = DateTime.Now;
                                        //announce.name = doc.DocumentNode.SelectSingleNode(".//div[@class='name']").FirstChild?.InnerText;
                                        announce.text = doc.DocumentNode.SelectSingleNode(".//div[@class='side']//article//p").InnerText;
                                        announce.view_count = Int32.Parse(doc.DocumentNode.SelectNodes(".//div[@class='item_info']//p")[1].InnerText.Split(": ")[1]);
                                        announce.original_date = doc.DocumentNode.SelectNodes(".//div[@class='item_info']//p")[2].InnerText;


                                        ///////////////////////IMAGES
                                        var filePath = $@"BinaAz/{DateTime.Now.Year}/{DateTime.Now.Month}/{id}/";
                                        var images = imageUploader.ImageDownloader(doc, id.ToString(), filePath, _httpClient);

                                        var uri = new Uri(doc.DocumentNode.SelectNodes(".//div[@class='thumbnail']")[0].Attributes["data-mfp-src"].Value);

                                        var uriWithoutQuery = uri.GetLeftPart(UriPartial.Path);
                                        var fileExtension = Path.GetExtension(uriWithoutQuery);
                                        announce.cover = $@"BinaAz/{DateTime.Now.Year}/{DateTime.Now.Month}/{id}/Thumb{fileExtension}";
                                        announce.logo_images = JsonSerializer.Serialize(await images);
                                        /////////////////////

                                        if (doc.DocumentNode.SelectSingleNode(".//div[@class='services-container']//h1").InnerText.Contains("İcarəyə verilir"))
                                            announce.announce_type = 1;
                                        else if (doc.DocumentNode.SelectSingleNode(".//div[@class='services-container']//h1").InnerText.Contains("Satılır"))
                                            announce.announce_type = 2;

                                        foreach (var item in doc.DocumentNode.SelectNodes(".//table[@class='parameters']//tr"))
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
                                            if (item.FirstChild.InnerText == "Kupça")
                                            {
                                                if (item.LastChild.InnerText == "yoxdur")
                                                    announce.document = 0;
                                                if (item.LastChild.InnerText == "var")
                                                    announce.document = 1;
                                            }
                                        }


                                        /////city
                                        var cities = unitOfWork.CitiesRepository.GetAll();
                                        foreach (var city in cities)
                                        {
                                            if (address.Contains(city.name))
                                            {
                                                announce.city_id = city.id;
                                                break;
                                            }
                                        }
                                        foreach (var item in doc.DocumentNode.SelectNodes(".//ul[@class='locations']//li"))
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
                                            if (item.InnerText.Contains(" r."))
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
                                            if (item.InnerText.Contains(" q."))
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


                                        ///////////// phone
                                        var phoneResponse = await _httpClient.GetAsync($"{url}/phones");
                                        StringBuilder numbers = new StringBuilder();
                                        if (phoneResponse != null)
                                        {
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
                                        }

                                        bool checkedNumber = false;
                                        var numberList = numbers.ToString().Split(',');
                                        var checkNumberRieltorResult = unitOfWork.CheckNumberRepository.CheckNumberForRieltor(numberList);
                                        if (checkNumberRieltorResult > 0)
                                        {

                                            announce.announcer = checkNumberRieltorResult;
                                            announce.number_checked = true;
                                            checkedNumber = true;

                                        }

                                        await unitOfWork.Announces.Create(announce);
                                        unitOfWork.Dispose();
                                        if (checkedNumber == false)
                                        {

                                            //EMLAK - BAZASI
                                            await _emlakBaza.CheckAsync(_httpClient, id, numberList);
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
                                    unitOfWork.ParserAnnounceRepository.Update(model);
                                    duration = 0;
                                    TelegramBotService.Sender($"bina.az limited {maxRequest}");

                                    break;
                                }
                            }
                            catch (Exception e)
                            {

                                TelegramBotService.Sender($"bina.az exception {e.Message}");

                                count = 10;

                            }
                           await Task.Delay(5000);
                        }
                    }
                    catch (Exception e)
                    {

                        TelegramBotService.Sender($"no connection bina.az {e.Message}");

                    }
                }
            }
        }
    }
}