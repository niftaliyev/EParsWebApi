using HtmlAgilityPack;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using WebApi.Helpers;

namespace WebApi.Services.TapAz
{
    public class TapAzImageUploader
    {
        private readonly FileUploadHelper _fileUploadHelper;
        public TapAzImageUploader(FileUploadHelper fileUploadHelper)
        {
            _fileUploadHelper = fileUploadHelper;
        }
        public void ImageDownloader(HtmlDocument doc, string id, string filePath, HttpClient httpClient)
        {
            Task.Run(async () =>
            {
                try
                {
                    var count = doc.DocumentNode.SelectNodes(".//div[@class='thumbnails']//a").Count;

                    for (int i = 0; i < count; i++)
                    {
                        var link = doc.DocumentNode.SelectNodes(".//div[@class='thumbnails']//a")[i].Attributes["href"].Value;

                        if (!Directory.Exists(filePath))
                        {
                            Directory.CreateDirectory(filePath);

                        }
                        await _fileUploadHelper.DownloadImageAsync(filePath, Guid.NewGuid().ToString(), new Uri(link), httpClient);
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
        }
    }
}
