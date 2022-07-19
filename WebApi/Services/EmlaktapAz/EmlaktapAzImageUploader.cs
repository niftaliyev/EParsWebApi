using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WebApi.Helpers;

namespace WebApi.Services.EmlaktapAz
{
    public class EmlaktapAzImageUploader
    {
        private readonly FileUploadHelper _fileUploadHelper;
        public EmlaktapAzImageUploader(FileUploadHelper fileUploadHelper)
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


                    var thumbnails = doc.DocumentNode.SelectNodes(".//div[@class='fotorama']//a");

                    for (int i = 0; i < thumbnails.Count; i++)
                    {
                        var link = $"https://emlaktap.az{thumbnails[i].Attributes["href"].Value}";
                        if (i == 0)
                        {
                            var thumbLink = doc.DocumentNode.SelectNodes(".//div[@class='fotorama']//a//img")[0].Attributes["src"].Value;
                            var abosulteLink = $"https://emlaktap.az{thumbLink}";
                            await _fileUploadHelper.DownloadImageAsync(filePath, "Thumb", new Uri(abosulteLink), httpClient);

                        }

                       

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
