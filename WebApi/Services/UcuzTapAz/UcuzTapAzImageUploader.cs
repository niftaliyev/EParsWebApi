using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WebApi.Helpers;

namespace WebApi.Services.UcuzTapAz
{
    public class UcuzTapAzImageUploader
    {
        private readonly FileUploadHelper _fileUploadHelper;
        public UcuzTapAzImageUploader(FileUploadHelper fileUploadHelper)
        {
            _fileUploadHelper = fileUploadHelper;
        }
        public async Task<List<string>> ImageDownloader(HtmlDocument doc, string id, string filePath, HttpClient httpClient)
        {
            List<string> list = new List<string>();

            list = await Task.Run(async () =>
            {
                List<string> images = new List<string>();
                try
                {
                    var photos = doc.DocumentNode.SelectNodes("/html/body/div[5]/div/div[1]/section/div/div[2]/div[2]/div[2]/div[1]/a");

                    for (int i = 0; i < photos.Count; i++)
                    {
                        var link = photos[i].Attributes["href"].Value;

                        if (i == 0)
                        {
                           
                          
                            var thumbLink = doc.DocumentNode.SelectSingleNode("/html/body/div[5]/div/div[1]/section/div/div[2]/div[1]/div/a/img")
                                                            .Attributes["src"].Value;
                            await _fileUploadHelper.DownloadImageAsync(filePath, "Thumb", new Uri(thumbLink), httpClient);

                        }

                        var filename = Guid.NewGuid().ToString();
                        var uri = new Uri(link);

                        var uriWithoutQuery = uri.GetLeftPart(UriPartial.Path);
                        var fileExtension = Path.GetExtension(uriWithoutQuery);

                        await _fileUploadHelper.DownloadImageAsync(filePath, filename, uri, httpClient);

                        var fileEndPath = $"{filePath}{filename}{fileExtension}";
                        images.Add(fileEndPath);

                    }
                }
                catch (Exception e)
                {
                }
                return images;

            });
            return list;

        }
    }
}
