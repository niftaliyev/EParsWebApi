using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebApi.Helpers
{
    public class FileUploadHelper
    {
        public async Task DownloadImageAsync(string directoryPath, string fileName, Uri uri, HttpClient httpClient)
        {
            await DownloadImageS3Async(directoryPath,fileName,uri,httpClient);

            //await Task.Run(() =>
            //{
            //    try
            //    {
            //        // Get the file extension
            //        var uriWithoutQuery = uri.GetLeftPart(UriPartial.Path);
            //        var fileExtension = Path.GetExtension(uriWithoutQuery);

            //        // Create file path and ensure directory exists
            //        var path = Path.Combine(directoryPath, $"{fileName}{fileExtension}");
            //        //Directory.CreateDirectory(directoryPath);

            //        // Download the image and write to the file
            //        var imageBytes = httpClient.GetByteArrayAsync(uri);
            //        File.WriteAllBytesAsync(path, imageBytes.Result);
            //    }
            //    catch (AggregateException e)
            //    {
            //        Console.WriteLine(e.Message);
            //    }
            //});
        }

        public async Task DownloadImageS3Async(string directoryPath, string fileName, Uri uri, HttpClient httpClient)
        {
            await Task.Run(async () =>
            {
                try
                {
                    // Get the file extension
                    var uriWithoutQuery = uri.GetLeftPart(UriPartial.Path);
                    var fileExtension = Path.GetExtension(uriWithoutQuery);
         
                    var imageBytes = await httpClient.GetByteArrayAsync(uri);

                    var s3 = new AmazonS3Client("AKIAXGTYXE7SOLQTSWUO", "j4+pHSZIFdIE/a8yXd3RaFXuzkDDjAx+xmQa0wRN", RegionEndpoint.EUCentral1);

                    using (MemoryStream memoryStream = new MemoryStream(imageBytes))
                    {
                        await s3.PutObjectAsync(new Amazon.S3.Model.PutObjectRequest
                        {
                            InputStream = memoryStream,
                            Key = $"{directoryPath}{fileName}{fileExtension}",
                            BucketName = "emlakcrawler",


                        });
                    }
                }
                catch (AggregateException e)
                {
                    Console.WriteLine(e.Message);
                }
            });
        }
    }
}
