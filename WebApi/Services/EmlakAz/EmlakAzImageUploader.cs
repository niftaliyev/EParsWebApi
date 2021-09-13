using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using WebApi.Helpers;
using WebApi.Models;
using WebApi.Repository;

namespace WebApi.Services.EmlakAz
{
    public class EmlakAzImageUploader
    {
        private readonly FileUploadHelper _fileUploadHelper;
        private readonly HttpClient httpClient;
        private readonly UnitOfWork unitOfWork;
        bool turn = true;
        public EmlakAzImageUploader(FileUploadHelper fileUploadHelper, HttpClient httpClient, UnitOfWork unitOfWork)
        {
            _fileUploadHelper = fileUploadHelper;
            this.httpClient = httpClient;
            this.unitOfWork = unitOfWork;
        }
        public async Task<List<string>> ImageDownloaderAsync(HtmlDocument doc, string id, string filePath)
        {
            List<string> randoms = new List<string>();
            List<string> list = new List<string>();
            try
            {
                list =await Task.Run(async () =>
                {
                    var count = doc.DocumentNode.SelectNodes(".//img").Count;
                    bool turn = true;
                    List<string> images = new List<string>();


                    for (int i = 0; i < count; i++)
                    {
                        if (doc.DocumentNode.SelectNodes(".//img")[i].Attributes["src"].Value.StartsWith("/images/announces"))
                        {
                            var split = doc.DocumentNode.SelectNodes(".//img")[i].Attributes["src"].Value.Split('/');
                            if (split[5] == id)
                            {
                                var link = doc.DocumentNode.SelectNodes(".//img")[i].Attributes["src"].Value;

                                

                                if (!Directory.Exists(filePath))
                                {
                                    Directory.CreateDirectory(filePath);

                                }
                                if (turn)
                                {
                                    var link2 = doc.DocumentNode.SelectNodes(".//img")[i].Attributes["src"].Value;
                                    var index = link2.LastIndexOf('-');
                                    var index2 = link2.LastIndexOf('.');
                                    var count2 = index2 - index;
                                    var link0 = link2.Remove(index, count2);
                                    await _fileUploadHelper.DownloadImageAsync(filePath, "Thumb", new Uri($"https://emlak.az{link0}"), httpClient);
                                    turn = false;
                                }
                                var filename = Guid.NewGuid().ToString();
                                var uri = new Uri($"https://emlak.az{link}");

                                var uriWithoutQuery = uri.GetLeftPart(UriPartial.Path);
                                var fileExtension = Path.GetExtension(uriWithoutQuery);

                                await _fileUploadHelper.DownloadImageAsync(filePath, filename, uri, httpClient);
                                var indexStartUpload = filePath.IndexOf("UploadFile");
                                var fileEndPath = $"{filePath.Substring(indexStartUpload)}/{filename}{fileExtension}";
                                images.Add(fileEndPath);
                                //Console.WriteLine($"{filePath.Substring(indexStartUpload)}/{filename}{fileExtension}");
                                
                            }

                        }
                    }
                    return images;
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return list;

        }
    }
}
