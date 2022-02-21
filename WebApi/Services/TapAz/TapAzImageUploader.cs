using HtmlAgilityPack;
using System;
using System.Collections.Generic;
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
        public async Task<List<string>> ImageDownloader(HtmlDocument doc, string id, string filePath, HttpClient httpClient)
        {
            List<string> list = new List<string>();

            list = await Task.Run(async () =>
            {
                List<string> images = new List<string>();
                try
                {


                    var count = doc.DocumentNode.SelectNodes(".//div[@class='thumbnails']//a").Count;

                    for (int i = 0; i < count; i++)
                    {
                        var link = doc.DocumentNode.SelectNodes(".//div[@class='thumbnails']//a")[i].Attributes["href"].Value;

                        if (i == 0)
                        {
                            await _fileUploadHelper.DownloadImageAsync(filePath, "Thumb", new Uri($"{link.Replace("full", "thumbnail")}"), httpClient);

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
