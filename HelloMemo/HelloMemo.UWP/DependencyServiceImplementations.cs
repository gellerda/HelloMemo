using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Windows.Storage;
using Windows.UI.Notifications;


//[assembly: Dependency(typeof(ILocalFiles)) ] -- Для регистрации в UWP ак делать не нужно. 
// Регистрировать нужно вручную в проекте UWP в файле App.Xaml.cs вот так:
// Xamarin.Forms.DependencyService.Register<LocalFilesUWP>();
namespace HelloMemo.UWP
{
    //******************************************************************************************************************************************
    public class ToastUWP : IToast
    {
        //---------------------------------------------------------------------------------------
        public void LongToast(string message)
        {
            //Toast.MakeText(Application.Context, message, ToastLength.Long).Show();
            ShowToastNotification("HelloMemo", message, 4);
        }
        //---------------------------------------------------------------------------------------
        public void ShortToast(string message)
        {
            //Toast.MakeText(Application.Context, message, ToastLength.Short).Show();
            ShowToastNotification("HelloMemo", message, 2);
        }
        //---------------------------------------------------------------------------------------
        private void ShowToastNotification(string title, string stringContent, int showSeconds)
        {
            ToastNotifier ToastNotifier = ToastNotificationManager.CreateToastNotifier();
            Windows.Data.Xml.Dom.XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
            Windows.Data.Xml.Dom.XmlNodeList toastNodeList = toastXml.GetElementsByTagName("text");
            toastNodeList.Item(0).AppendChild(toastXml.CreateTextNode(title));
            toastNodeList.Item(1).AppendChild(toastXml.CreateTextNode(stringContent));
            Windows.Data.Xml.Dom.IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
            //Windows.Data.Xml.Dom.XmlElement audio = toastXml.CreateElement("audio");
            //audio.SetAttribute("src", "ms-winsoundevent:Notification.SMS");

            ToastNotification toast = new ToastNotification(toastXml);
            toast.ExpirationTime = DateTime.Now.AddSeconds(showSeconds);
            ToastNotifier.Show(toast);
        }
        //---------------------------------------------------------------------------------------
    }
    //******************************************************************************************************************************************
    public class LocalFilesUWP : ILocalFiles
    {
        //---------------------------------------------------------------------------------------
        // fileName - имя файла без пути. Путь к файлу - текущая папка приложения.
        public async Task<Stream> GetLocalFileReadingStreamAsync(string fileName)
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFile dbFile = await localFolder.TryGetItemAsync(fileName) as StorageFile;

            if (dbFile != null)
            {
                //return new System.IO.FileStream(dbPath, System.IO.FileMode.Open);
                Stream stream = (await dbFile.OpenAsync(Windows.Storage.FileAccessMode.Read)).AsStream();
                return stream;
            }
            return null;
        }
        //---------------------------------------------------------------------------------------
        // fileName - имя файла без пути. Путь к файлу - текущая папка приложения.
        public async Task<Stream> GetLocalFileWritingStreamAsync(string fileName)
        {
            StorageFile dbFile;
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;

            // Важно не создавать файл заново, если данный файл уже существует т.к. при перезаписи может случиться ошибка, а старый файл уже затерт.
            try
            {
                dbFile = await localFolder.GetFileAsync(fileName);
            }
            catch (FileNotFoundException e)
            {
                dbFile = await localFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting); // Создадим файл. Если уже существует, то заменим.
            }

            if (dbFile != null)
            {
                //return new System.IO.FileStream(dbPath, System.IO.FileMode.Open);
                Stream stream = (await dbFile.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite)).AsStream();
                return stream;
            }
            return null;
        }
        //---------------------------------------------------------------------------------------
        //---------------------------------------------------------------------------------------
        //---------------------------------------------------------------------------------------
    }
    //******************************************************************************************************************************************
    //******************************************************************************************************************************************
    //******************************************************************************************************************************************
}
