using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WebApi.Helpers;

namespace WebApi.Services.VipEmlakAz
{
    public class VipEmlakAzImageUploader
    {
        private readonly FileUploadHelper _fileUploadHelper;
        public VipEmlakAzImageUploader(FileUploadHelper fileUploadHelper)
        {
            _fileUploadHelper = fileUploadHelper;
        }
        public async Task<List<string>> ImageDownloader(HtmlDocument doc, string id, string filePath, HttpClient httpClient)
        {
           
            var list = await Task.Run(async () =>
            {
                List<string> images = new List<string>();


                var photos = doc.DocumentNode.SelectNodes(".//div[@id='picsopen']//div//a");

                for (int i = 0; i < photos.Count; i++)
                {
                    var link = $"https://vipemlak.az{photos[i].Attributes["href"].Value}";

                    if (i == 0)
                    {
                        var thumbLink = doc.DocumentNode.SelectNodes(".//div[@id='picsopen']//div//a//img")[0].Attributes["src"].Value;
                        var absoluteThumbLink = $"https://vipemlak.az{thumbLink}";
                        await _fileUploadHelper.DownloadImageAsync(filePath, "Thumb", new Uri(absoluteThumbLink), httpClient);
                    }

                    var filename = Guid.NewGuid().ToString();
                    var uri = new Uri(link);

                    var uriWithoutQuery = uri.GetLeftPart(UriPartial.Path);
                    var fileExtension = Path.GetExtension(uriWithoutQuery);

                    await _fileUploadHelper.DownloadImageAsync(filePath, filename, uri, httpClient);


                    var fileEndPath = $"{filePath}{filename}{fileExtension}";
                    images.Add(fileEndPath);

                }
            
                return images;

             });
            return list;

        }
}
}
