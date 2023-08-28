﻿using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using WebApi.Helpers;

namespace WebApi.Services.LalafoAz
{
    public class LalafoImageUploader
    {
        private readonly FileUploadHelper _fileUploadHelper;
        private readonly HttpClient httpClient;

        public LalafoImageUploader(FileUploadHelper fileUploadHelper, HttpClient httpClient)
        {
            _fileUploadHelper = fileUploadHelper;
            this.httpClient = httpClient;
        }
        public async Task<List<string>> ImageDownloaderAsync(HtmlDocument doc, string id, string filePath)
        {
            List<string> list = new List<string>();
            bool turn = true;
            try
            {
                list = await Task.Run(async () =>
                {
                    List<string> images = new List<string>();

                    if (doc.DocumentNode.SelectNodes(".//ul[@class='desktop css-h8ujnu']//li//a")[2] != null)
                    {
                        if (doc.DocumentNode.SelectNodes(".//div[@class='carousel__custom-paging-img-wrap']//picture") != null)
                        {
                            var images2 = doc.DocumentNode.SelectNodes(".//div[@class='carousel__custom-paging-img-wrap']//picture//img");
                            foreach (var item in images2)
                            {

                                if (turn)
                                {
                                    var thumbUri = new Uri(item.Attributes["src"].Value);
                                    await _fileUploadHelper.DownloadImageAsync(filePath, "Thumb", thumbUri, httpClient);
                                    turn = false;
                                }

                                var filename = Guid.NewGuid().ToString();
                                var uri = new Uri(item.Attributes["src"].Value);

                                await _fileUploadHelper.DownloadImageAsync(filePath, filename, uri, httpClient);

                                var uriWithoutQuery = uri.GetLeftPart(UriPartial.Path);
                                var fileExtension = Path.GetExtension(uriWithoutQuery);

                                var fileEndPath = $"{filePath}{filename}{fileExtension}";
                                images.Add(fileEndPath);

                            }

                        }
                        else if (doc.DocumentNode.SelectNodes("/html/body/div[1]/div/div[1]/div/div[3]/div[1]/div[1]/div/div/ul") != null)
                        {
                            var images2 = doc.DocumentNode.SelectNodes("/html/body/div[1]/div/div[1]/div/div[3]/div[1]/div[1]/div/div/ul/li/a/img");
                            foreach (var item in images2)
                            {

                                if (turn)
                                {
                                    var thumbUri = new Uri(item.Attributes["src"].Value);
                                    await _fileUploadHelper.DownloadImageAsync(filePath, "Thumb", thumbUri, httpClient);
                                    turn = false;
                                }

                                var filename = Guid.NewGuid().ToString();
                                var uri = new Uri(item.Attributes["src"].Value);

                                await _fileUploadHelper.DownloadImageAsync(filePath, filename, uri, httpClient);

                                var uriWithoutQuery = uri.GetLeftPart(UriPartial.Path);
                                var fileExtension = Path.GetExtension(uriWithoutQuery);

                                var fileEndPath = $"{filePath}{filename}{fileExtension}";
                                images.Add(fileEndPath);

                            }
                        }
                    }
                    if (images.Count == 0)
                    {
                        TelegramBotService.Sender($"Image count is 0 , {id}");
                    }
                    return images;
                });
            }
            catch (Exception ex)
            {
                TelegramBotService.Sender($"Image upload error -lalafo - {ex.Message}");
            }
            return list;
        }
    }
}
