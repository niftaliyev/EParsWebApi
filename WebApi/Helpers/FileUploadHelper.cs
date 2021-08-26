using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebApi.Helpers
{
    public class FileUploadHelper
    {

        public async
        Task
        DownloadImageAsync(string directoryPath, string fileName, Uri uri, HttpClient httpClient)
        {

            await Task.Run(() =>
            {
                try
                {
                    // Get the file extension
                    var uriWithoutQuery = uri.GetLeftPart(UriPartial.Path);
                    var fileExtension = Path.GetExtension(uriWithoutQuery);

                    // Create file path and ensure directory exists
                    var path = Path.Combine(directoryPath, $"{fileName}{fileExtension}");
                    //Directory.CreateDirectory(directoryPath);

                    // Download the image and write to the file
                    var imageBytes = httpClient.GetByteArrayAsync(uri);
                    File.WriteAllBytesAsync(path, imageBytes.Result);
                }
                catch (AggregateException e)
                {

                    Console.WriteLine(e.Message);

                }

            });
        }


    }
}
