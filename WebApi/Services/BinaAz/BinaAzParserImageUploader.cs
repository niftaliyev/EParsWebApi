using HtmlAgilityPack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
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
        //public async Task<List<string>> ImageDownloader(HtmlDocument doc, string id, string filePath, HttpClient httpClient)
        //{
        //    List<string> list = new List<string>();

        //    list = await Task.Run(async () =>
        //    {
        //        List<string> images = new List<string>();

        //        var turn = true;
        //        var tasks = new List<Task>();
        //        foreach (var item in doc.DocumentNode.SelectNodes(".//div[@class='thumbnail']"))
        //        {
        //            var link = item.Attributes["data-mfp-src"].Value.Replace("full", "large");

        //            if (turn)
        //            {
        //                //await _fileUploadHelper.DownloadImageAsync(filePath, "Thumb", new Uri($"{link.Replace("large", "thumbnail")}"), httpClient);
        //                tasks.Add(_fileUploadHelper.DownloadImageAsync(filePath, "Thumb", new Uri($"{link.Replace("large", "thumbnail")}"), httpClient));
        //                turn = false;
        //            }

        //            var filename = Guid.NewGuid().ToString();
        //            var uri = new Uri(link);

        //            var uriWithoutQuery = uri.GetLeftPart(UriPartial.Path);
        //            var fileExtension = Path.GetExtension(uriWithoutQuery);

        //            //await _fileUploadHelper.DownloadImageAsync(filePath, filename, uri, httpClient);
        //            tasks.Add(_fileUploadHelper.DownloadImageAsync(filePath, filename, uri, httpClient));
        //            //var indexStartUpload = filePath.IndexOf("UploadFile");
        //            var fileEndPath = $"{filePath}{filename}{fileExtension}";
        //            images.Add(fileEndPath);
        //        }

        //        await Task.WhenAll(tasks);

        //        return images;

        //    });
        //    return list;

        //}

        public async Task<List<string>> ImageDownloader(HtmlDocument doc, string id, string filePath, HttpClient httpClient)
        {
            var tasks = new List<Task>();
            var images = new List<string>();

            //var nodes = doc.DocumentNode.SelectNodes(".//div[@class='thumbnail']");

            var nodes = doc.DocumentNode.SelectNodes(".//div[@class='product-photos__slider-nav-i_picture']");

            // Get the style attribute value containing the image URL


            await Task.Run(() => Parallel.ForEach(nodes, node =>
            {
                string styleAttributeValue = node.GetAttributeValue("style", "");

                // Extract the image URL from the style attribute value

                int startIndex = styleAttributeValue.IndexOf("&#39;") + 5;

                // Find the ending index of the URL within the style attribute value
                int endIndex = styleAttributeValue.LastIndexOf("&#39;");

                // Extract the URL
                string link = styleAttributeValue.Substring(startIndex, endIndex - startIndex).Replace("full", "large");

                // var link = node.Attributes["data-mfp-src"].Value.Replace("full", "large");
                var filename = Guid.NewGuid().ToString();
                var uri = new Uri(link);
                var uriWithoutQuery = uri.GetLeftPart(UriPartial.Path);
                var fileExtension = Path.GetExtension(uriWithoutQuery);

                if (node == nodes[0])
                {
                    tasks.Add(_fileUploadHelper.DownloadImageAsync(filePath, "Thumb", new Uri($"{link.Replace("large", "thumbnail")}"), httpClient));
                }

                tasks.Add(_fileUploadHelper.DownloadImageAsync(filePath, filename, uri, httpClient));

                var fileEndPath = $"{filePath}{filename}{fileExtension}";
                images.Add(fileEndPath);
            }));

            await Task.WhenAll(tasks);

            return images;
        }

    }
}
