using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WebApi.Helpers;

namespace WebApi.Services.BinaAz
{
    public class BinaAzParserImageUploader
    {
        private readonly FileUploadHelper _fileUploadHelper;
        public BinaAzParserImageUploader(FileUploadHelper fileUploadHelper)
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
                    var turn = true;

                    foreach (var item in doc.DocumentNode.SelectNodes(".//div[@class='thumbnail']"))
                    {
                        var link = item.Attributes["data-mfp-src"].Value.Replace("full", "large");

                        if (turn)
                        {
                            await _fileUploadHelper.DownloadImageAsync(filePath, "Thumb", new Uri($"{link.Replace("large", "thumbnail")}"), httpClient);
                            turn = false;
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
                    Console.WriteLine(e);
                }
                return images;

            });
            return list;

        }
    }
}
