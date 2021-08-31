using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WebApi.Helpers;

namespace WebApi.Services.EmlakAz
{
    public class EmlakAzImageUploader
    {
        private readonly FileUploadHelper _fileUploadHelper;
        private readonly HttpClient httpClient;

        public EmlakAzImageUploader(FileUploadHelper fileUploadHelper, HttpClient httpClient)
        {
            _fileUploadHelper = fileUploadHelper;
            this.httpClient = httpClient;
        }
        public void ImageDownloader(HtmlDocument doc, string id, string filePath)
        {


            try
            {
                Task.Run(async () =>
                {
                    var count = doc.DocumentNode.SelectNodes(".//img").Count;



                    for (int i = 0; i < count; i++)
                    {
                        if (doc.DocumentNode.SelectNodes(".//img")[i].Attributes["src"].Value
                            .StartsWith("/images/announces"))
                        {
                            var split = doc.DocumentNode.SelectNodes(".//img")[i].Attributes["src"].Value.Split('/');
                            if (split[5] == id)
                            {
                                var link = doc.DocumentNode.SelectNodes(".//img")[i].Attributes["src"].Value;


                                if (!Directory.Exists(filePath))
                                {
                                    Directory.CreateDirectory(filePath);

                                }
                                await _fileUploadHelper.DownloadImageAsync(filePath, Guid.NewGuid().ToString(), new Uri($"https://emlak.az{link}"), httpClient);
                            }

                        }
                    }

                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }
    }
}
