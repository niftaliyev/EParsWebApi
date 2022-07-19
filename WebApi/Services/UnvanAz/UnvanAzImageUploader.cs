using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WebApi.Helpers;

namespace WebApi.Services.UnvanAz
{
    public class UnvanAzImageUploader
    {

        private readonly FileUploadHelper _fileUploadHelper;
        public UnvanAzImageUploader(FileUploadHelper fileUploadHelper)
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

                    var photos = doc.DocumentNode.SelectNodes(".//div[@id='picsopen']//div//a");
                    //var photos = doc.DocumentNode.SelectNodes("/html/body/div[5]/div/div[1]/section/div/div[2]/div[2]/div[2]/div[1]/a");

                    for (int i = 0; i < photos.Count; i++)
                    {
                        var link = $"https://unvan.az{photos[i].Attributes["href"].Value}";

                        if (i == 0)
                        {
                            var thumbLink = doc.DocumentNode.SelectNodes(".//div[@id='picsopen']//div//a//img")[0].Attributes["src"].Value;
                            var absoluteThumbLink = $"https://unvan.az{thumbLink}";
                            //var thumbLink = doc.DocumentNode.SelectSingleNode("/html/body/div[5]/div/div[1]/section/div/div[2]/div[2]/div[2]/div[1]/a[1]/img").Attributes["src"].Value;
                            await _fileUploadHelper.DownloadImageAsync(filePath, "Thumb", new Uri(absoluteThumbLink), httpClient);

                        }

                        //if (!Directory.Exists(filePath))
                        //{
                        //    Directory.CreateDirectory(filePath);
                        //}

                        var filename = Guid.NewGuid().ToString();
                        var uri = new Uri(link);

                        var uriWithoutQuery = uri.GetLeftPart(UriPartial.Path);
                        var fileExtension = Path.GetExtension(uriWithoutQuery);

                        await _fileUploadHelper.DownloadImageAsync(filePath, filename, uri, httpClient);

                        //var indexStartUpload = filePath.IndexOf("UploadFile");
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
