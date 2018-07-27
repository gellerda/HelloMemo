using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

[assembly: Xamarin.Forms.Dependency(typeof(HelloMemo.Droid.ToastAndroid))]
[assembly: Xamarin.Forms.Dependency(typeof(HelloMemo.Droid.LocalFilesAndroid))]
namespace HelloMemo.Droid
{
    public class ToastAndroid : IToast
    {
        //---------------------------------------------------------------------------------------
        public void LongToast(string message)
        {
            Toast.MakeText(Application.Context, message, ToastLength.Long).Show();
        }
        //---------------------------------------------------------------------------------------
        public void ShortToast(string message)
        {
            Toast.MakeText(Application.Context, message, ToastLength.Short).Show();
        }
        //---------------------------------------------------------------------------------------
        //---------------------------------------------------------------------------------------
    }
    //******************************************************************************************************************************************
    public class LocalFilesAndroid : ILocalFiles
    {
        //---------------------------------------------------------------------------------------
        // fileName - имя файла без пути. Путь к файлу - текущая папка приложения.
        public async Task<Stream> GetLocalFileReadingStreamAsync(string fileName)
        {
            string appPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            string dbPath = System.IO.Path.Combine(appPath, fileName);

            return new System.IO.FileStream(dbPath, System.IO.FileMode.Open);
        }
        //---------------------------------------------------------------------------------------
        // fileName - имя файла без пути. Путь к файлу - текущая папка приложения.
        public async Task<Stream> GetLocalFileWritingStreamAsync(string fileName)
        {
            string appPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            string dbPath = System.IO.Path.Combine(appPath, fileName);

            // Важно не создавать файл заново, если данный файл уже существует т.к. при перезаписи может случиться ошибка, а старый файл уже затерт.
            return new System.IO.FileStream(dbPath, System.IO.FileMode.OpenOrCreate); 
        }
        //---------------------------------------------------------------------------------------
        //---------------------------------------------------------------------------------------
        //---------------------------------------------------------------------------------------
    }
}