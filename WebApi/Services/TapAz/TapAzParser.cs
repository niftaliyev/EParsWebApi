using HtmlAgilityPack;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using WebApi.Models;
using WebApi.Repository;

namespace WebApi.Services.TapAz
{
    public class TapAzParser
    {
        private readonly EmlakBaza _emlakBaza; //если не используется заче он здесь. мусор
        private readonly TapAzImageUploader _uploader;
        private readonly HttpClientCreater clientCreater;
        private readonly UnitOfWork unitOfWork;
        private HttpClient _httpClient;
        HttpResponseMessage header;
        static string[] proxies; // лучше добавить ентер
        public TapAzParser(EmlakBaza emlakBaza, TapAzImageUploader uploader, HttpClientCreater clientCreater, UnitOfWork unitOfWork)
        {
            this._emlakBaza = emlakBaza;
            _uploader = uploader;
            this.clientCreater = clientCreater;
            this.unitOfWork = unitOfWork;
            proxies = File.ReadAllLines("proxies.txt");
            _httpClient = clientCreater.Create(proxies[0]);

            Console.WriteLine(proxies[0]);
        }

        public async Task TapAzPars()
        {
            var model = unitOfWork.ParserAnnounceRepository.GetBySiteName("https://tap.az");

            if (!model.isActive)
            {
                try
                {
                    var id = model.last_id;

                    model.isActive = true;
                    unitOfWork.ParserAnnounceRepository.Update(model);
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
                            Uri myUri = new Uri($"{model.site}/elanlar/-/-/{++id}", UriKind.Absolute);
                            header = await _httpClient.GetAsync(myUri);
                            var url = header.RequestMessage.RequestUri.AbsoluteUri;
                            count++;

                            if (header.RequestMessage.RequestUri.ToString().StartsWith("https://tap.az/elanlar/dasinmaz-emlak/"))
                            {
                                var response = await _httpClient.GetAsync(url);
                                Console.WriteLine(response.StatusCode.ToString());

                                if (response.IsSuccessStatusCode)
                                {
                                    var html = await response.Content.ReadAsStringAsync();
                                    if (!string.IsNullOrEmpty(html))
                                    {
                                        HtmlDocument doc = new HtmlDocument();
                                        doc.LoadHtml(html);
                                        Announce announce = new Announce();
                                        duration = 0;
                                        if (doc.DocumentNode.SelectSingleNode(".//span[@class='price-val']") != null)
                                            announce.price = doc.DocumentNode.SelectSingleNode(".//span[@class='price-val']").InnerText.Replace(" ", "");

                                        if (doc.DocumentNode.SelectSingleNode(".//div[@class='title-container']//h1") != null)
                                            announce.name = doc.DocumentNode.SelectSingleNode(".//div[@class='title-container']//h1").InnerText;
                                        if (doc.DocumentNode.SelectSingleNode(".//div[@class='lot-text']//p") != null)
                                            announce.text = doc.DocumentNode.SelectSingleNode(".//div[@class='lot-text']//p").InnerText;

                                        if (doc.DocumentNode.SelectSingleNode(".//a[@class='phone']") != null)
                                        {
                                            string mobileregex = doc.DocumentNode.SelectSingleNode(".//a[@class='phone']").InnerText;

                                            if (mobileregex != null)
                                            {
                                                var charsToRemove = new string[] { "(", ")", "-", ".", " " };
                                                foreach (var c in charsToRemove)
                                                {
                                                    mobileregex = mobileregex.Replace(c, string.Empty);
                                                }
                                                if (mobileregex.Contains(','))
                                                {
                                                    var numberList = mobileregex.Split(',');

                                                }

                                                //EMLAK - BAZASI
                                                // _ = _emlakBaza.CheckAsync(_httpClient, numberList);  // не пуш с комментариями - мусор
                                            }
                                            announce.mobile = mobileregex;
                                            Console.WriteLine(doc.DocumentNode.SelectSingleNode(".//div[@class='title-container']//h1").InnerText);
                                            Console.WriteLine(mobileregex);
                                        }
                                        if (doc.DocumentNode.SelectNodes(".//div[@class='lot-info']/p")[2].InnerText.Contains("Bugün"))
                                            announce.original_date = DateTime.Now.ToShortDateString();

                                        if (doc.DocumentNode.SelectNodes(".//div[@class='lot-info']/p")[2].InnerText.Contains("Dünən"))
                                            announce.original_date = DateTime.Now.AddDays(-1).ToShortDateString();
                                        else
                                            if (doc.DocumentNode.SelectNodes(".//div[@class='lot-info']/p")[2].InnerText.Replace("Yeniləndi: ", "") != null)
                                            announce.original_date = doc.DocumentNode.SelectNodes(".//div[@class='lot-info']/p")[2].InnerText.Replace("Yeniləndi: ", "");

                                        var countPropertyName = doc.DocumentNode.SelectNodes(".//td[@class='property-name']").Count;
                                        for (int i = 0; i < countPropertyName; i++)
                                        {
                                            if (doc.DocumentNode.SelectNodes(".//td[@class='property-name']")[i].InnerText == "Otaq sayı")
                                                announce.room_count = doc.DocumentNode.SelectNodes(".//td[@class='property-value']")[i].InnerText;
                                            if (doc.DocumentNode.SelectNodes(".//td[@class='property-name']")[i].InnerText == "Yerləşdirmə yeri")
                                                announce.address = doc.DocumentNode.SelectNodes(".//td[@class='property-value']")[i].InnerText;

                                            if (doc.DocumentNode.SelectNodes(".//td[@class='property-value']")[i].InnerText == "Satılır")
                                                announce.announce_type = 1;
                                            if (doc.DocumentNode.SelectNodes(".//td[@class='property-value']")[i].InnerText == "İcarəyə verilir")
                                                announce.announce_type = 2;
                                        }
                                        announce.original_id = id;
                                        announce.cover = "1";
                                        announce.images = $@"\UploadFile\TapAz\{DateTime.Now.Year}\{DateTime.Now.Month}\{id}";
                                        announce.cities_regions_id = 5;
                                        announce.view_count = doc.DocumentNode.SelectNodes(".//div[@class='lot-info']/p")[1].InnerText.Replace("Baxışların sayı: ", "");
                                        announce.announce_date = DateTime.Now.ToShortDateString();
                                        announce.parser_announce = model.site;

                                        Console.WriteLine(proxies[x]);

                                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), $@"wwwroot\UploadFile\TapAz\{DateTime.Now.Year}\{DateTime.Now.Month}\{id}");
                                        _uploader.ImageDownloader(doc, id.ToString(), filePath, _httpClient);

                                        //context.announces.Add(announce);  // не пуш с комментариями - мусор
                                        //context.SaveChanges();
                                        unitOfWork.Announces.Create(announce);
                                    }
                                }
                                //Console.WriteLine(response.StatusCode.ToString());  // не пуш с комментариями - мусор
                                //Console.WriteLine(id);
                            } // if end
                              //Console.WriteLine(header.IsSuccessStatusCode);
                            if (!header.IsSuccessStatusCode)
                            {
                                duration++;
                            }
                            if (duration >= 10)
                            {
                                model.last_id = (id - 10);
                                model.isActive = false;
                                unitOfWork.ParserAnnounceRepository.Update(model);
                                duration = 0;
                                break;
                            }
                        }
                        catch (Exception e)
                        {

                            Console.WriteLine("no connection");
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
