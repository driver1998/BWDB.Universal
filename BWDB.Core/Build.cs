using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.AccessCache;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace BWDB.Core
{
    public class Screenshot
    {
        public string Author { get; set; }
        public string Description { get; set; }

        public BitmapImage Thumbnail { get; set; }
        public BitmapImage Image { get; set; }

        public Screenshot() { }
    }

    public class ScreenshotJson
    {
        public string name { get; set; }
        public string author { get; set; }
    }

    public class Build
    {
        public int ProductID { get; set; }
        public int BuildID { get; set; }
        public string Stage { get; set; }
        public string ProductName { get; set; }
        public string Codename { get; set; }
        public string Version { get; set; }
        public string Buildtag { get; set; }
        public string Architecture { get; set; }
        public string Language { get; set; }
        public string SKU { get; set; }
        public string SerialNumber { get; set; }
        public string Fixes { get; set;}
        public string BIOSDate { get; set; }
        public int ScreenshotID { get; set; }

        public async Task<List<Screenshot>> GetSceenshots(StorageFolder screenshotFolder)
        {
            var httpWebRequest = WebRequest.Create($"http://119.29.206.109:8088/screenshot/{ScreenshotID.ToString()}");
            //httpWebRequest.ContentType = "application/json";

            var response = await httpWebRequest.GetResponseAsync();
            var httpStream = response.GetResponseStream();
            var reader = new StreamReader(httpStream);

            var webScreenshotList = JsonConvert.DeserializeObject<List<ScreenshotJson>>(await reader.ReadToEndAsync()).OrderBy(p => p.name);

            var screenshotList = new List<Screenshot>();

            foreach (var webScreen in webScreenshotList)
            {

                var screenRequest = WebRequest.Create($"http://119.29.206.109:8088/screenshot/{ScreenshotID.ToString()}/{webScreen.name}");
                var screenResponse = await screenRequest.GetResponseAsync();
                var screenResponseStream = screenResponse.GetResponseStream();

                var stream = new InMemoryRandomAccessStream();
                var outputStream = stream.GetOutputStreamAt(0);
                await RandomAccessStream.CopyAsync(screenResponseStream.AsInputStream(), outputStream);


                //var uri = new Uri($"http://119.29.206.109:8088/screenshot/{ScreenshotID.ToString()}/{webScreen.name}");
                var thumbnail = new BitmapImage()
                {
                    
                };
                await thumbnail.SetSourceAsync(stream);
                var image = new BitmapImage();
                await thumbnail.SetSourceAsync(stream.CloneStream());

                var screenshot = new Screenshot()
                {
                    Author = webScreen.author,
                    Description = webScreen.name,
                    Thumbnail = thumbnail,
                    Image = thumbnail
                };

                screenshotList.Add(screenshot);
            }
            /*
            //System.Diagnostics.Debug.WriteLine(await reader.ReadToEndAsync());
           
                StorageFolder folder;
                IReadOnlyList<StorageFile> filesList;
                try
                {
                    folder = await screenshotFolder.GetFolderAsync(ScreenshotID.ToString());
                    filesList = await folder.GetFilesAsync();
                }
                catch (Exception)
                {
                    return null;
                }
            
                var screenshotList = new List<Screenshot>();

            foreach (StorageFile file in filesList)
            {
                
                
                var request = System.Net.HttpWebRequest.Create("https://ss0.bdstatic.com/5aV1bjqh_Q23odCf/static/superman/img/logo/bd_logo1_31bdc765.png");
                var response = await request.GetResponseAsync();
                var ResponseStream = response.GetResponseStream();

                var stream = new Windows.Storage.Streams.InMemoryRandomAccessStream();
                var outputStream = stream.GetOutputStreamAt(0);
                await RandomAccessStream.CopyAsync(ResponseStream.AsInputStream(), outputStream);
                
                var stream = await file.OpenReadAsync();

                var thumbnail = new BitmapImage()
                {
                    DecodePixelHeight = 150,
                };
                await thumbnail.SetSourceAsync(stream);

                var image = new BitmapImage();
                await image.SetSourceAsync(stream.CloneStream());

                var screenshot = new Screenshot()
                {
                    Author = "placeholder",
                    Description = file.DisplayName,
                    Thumbnail = thumbnail,
                    Image = image
                };

                screenshotList.Add(screenshot);

                
                }
        */
           
                return screenshotList;
            
        }

    }
}
