using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WebApi.Helpers;

namespace WebApi.Services.YeniEmlakAz
{
    public class YeniEmlakAzParserImageUploader
    {
        private readonly FileUploadHelper _fileUploadHelper;
        public YeniEmlakAzParserImageUploader(FileUploadHelper fileUploadHelper)
        {
            _fileUploadHelper = fileUploadHelper;
        }

        public  Task<List<string>> ImageDownloader(HtmlDocument doc, string filePath, HttpClient httpClient)
        {
            List<string> list = new List<string>();
            bool isOpen = true;
            return Task.Run(async () =>
            {
                List<string> images = new List<string>();
                try
                {
                    if (doc.DocumentNode.SelectNodes(".//div[@class='imgbox']//a") != null)
                    {
                        foreach (var item in doc.DocumentNode.SelectNodes(".//div[@class='imgbox']//a"))
                        {
                            if (isOpen)
                            {
                                await _fileUploadHelper.DownloadImageAsync(filePath, "Thumb", new Uri(item.Attributes["href"].Value), httpClient);
                                isOpen = false;
                            }

                            var filename = Guid.NewGuid().ToString();
                            var uri = new Uri(item.Attributes["href"].Value);

                            var uriWithoutQuery = uri.GetLeftPart(UriPartial.Path);
                            var fileExtension = Path.GetExtension(uriWithoutQuery);

                            await _fileUploadHelper.DownloadImageAsync(filePath, filename, uri, httpClient);

                            var fileEndPath = $"{filePath}{filename}{fileExtension}";
                            images.Add(fileEndPath);
                        }
                    }
                }
                catch (Exception e)
                {
                }
                return images;

            });
        }
    }

}
