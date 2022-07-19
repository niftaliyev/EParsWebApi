using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WebApi.Helpers;

namespace WebApi.Services.DashinmazEmlak
{
    public class DashinmazEmlakImageUploader
    {
        private readonly FileUploadHelper _fileUploadHelper;
        public DashinmazEmlakImageUploader(FileUploadHelper fileUploadHelper)
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
                    
                    var largeImages = doc.DocumentNode.SelectNodes(".//div[@class='elan_read_foto_div_list']//a");
                  
                    bool turn = true;

                    foreach (var largeImage in largeImages)
                    {
                        var link = $"https://www.dashinmazemlak.az{largeImage.Attributes["href"].Value}";
                        if (turn)
                        {
                            //add one image as thumbnail
                            var thumbLink = doc.DocumentNode.SelectNodes(".//div[@class='elan_read_foto_div_list']//a//img")[0].Attributes["src"].Value;
                            var absoluteLink = $"https://dashinmazemlak.az{thumbLink}";
                            await _fileUploadHelper.DownloadImageAsync(filePath, "Thumb", new Uri(absoluteLink), httpClient);
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

                    //var count = doc.DocumentNode.SelectNodes(".//div[@class='thumbnails']//a").Count;

                    //for (int i = 0; i < count; i++)
                    //{
                    //    var link = doc.DocumentNode.SelectNodes(".//div[@class='thumbnails']//a")[i].Attributes["href"].Value;

                    //    if (i == 0)
                    //    {
                    //        await _fileUploadHelper.DownloadImageAsync(filePath, "Thumb", new Uri($"{link.Replace("full", "thumbnail")}"), httpClient);

                    //    }

                    //    //if (!Directory.Exists(filePath))
                    //    //{
                    //    //    Directory.CreateDirectory(filePath);
                    //    //}

                    //    var filename = Guid.NewGuid().ToString();
                    //    var uri = new Uri(link);

                    //    var uriWithoutQuery = uri.GetLeftPart(UriPartial.Path);
                    //    var fileExtension = Path.GetExtension(uriWithoutQuery);

                    //    await _fileUploadHelper.DownloadImageAsync(filePath, filename, uri, httpClient);

                    //    //var indexStartUpload = filePath.IndexOf("UploadFile");
                    //    var fileEndPath = $"{filePath}{filename}{fileExtension}";
                    //    images.Add(fileEndPath);

                    //}
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
