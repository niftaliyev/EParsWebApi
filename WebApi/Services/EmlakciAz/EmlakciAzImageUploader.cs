using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WebApi.Helpers;
using WebApi.Repository;

namespace WebApi.Services.EmlakciAz
{
    public class EmlakciAzImageUploader
    {
        private readonly FileUploadHelper _fileUploadHelper;
        private readonly HttpClient httpClient;
        public EmlakciAzImageUploader(FileUploadHelper fileUploadHelper, HttpClient httpClient)
        {
            _fileUploadHelper = fileUploadHelper;
            this.httpClient = httpClient;
        }
        public async Task<List<string>> ImageDownloaderAsync(HtmlDocument doc, string filePath)
        {


                return await Task.Run(async () =>
                {
                    var count = doc.DocumentNode.SelectNodes(".//img").Count;
                    bool turn = true;
                    List<string> images = new List<string>();
                    List<string> check = new List<string>();


                    for (int i = 0; i < count; i++)
                    {
                        if (doc.DocumentNode.SelectNodes(".//img")[i].Attributes["src"].Value.StartsWith("/images/estates/view/") || doc.DocumentNode.SelectNodes(".//img")[i].Attributes["src"].Value.StartsWith("/images/estates/original/"))
                        {
                                var link = doc.DocumentNode.SelectNodes(".//img")[i].Attributes["src"].Value;
                                if (!check.Contains(link))
                                {
                                    if (!Directory.Exists(filePath))
                                    {
                                        Directory.CreateDirectory(filePath);

                                    }
                                    if (turn)
                                    {
                                        var link2 = doc.DocumentNode.SelectNodes(".//img")[i].Attributes["src"].Value;

                                        await _fileUploadHelper.DownloadImageAsync(filePath, "Thumb", new Uri($"https://emlakci.az{link2}"), httpClient);
                                        turn = false;
                                    }
                                    var filename = Guid.NewGuid().ToString();
                                    var uri = new Uri($"https://emlakci.az{link}");

                                    var uriWithoutQuery = uri.GetLeftPart(UriPartial.Path);
                                    var fileExtension = Path.GetExtension(uriWithoutQuery);

                                    await _fileUploadHelper.DownloadImageAsync(filePath, filename, uri, httpClient);

                                    var fileEndPath = $"{filePath}{filename}{fileExtension}";

                                    images.Add(fileEndPath);
                                    check.Add(link);
                            }

                        }
                    }

                    return images;
                });

        }
    }
}
