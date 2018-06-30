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
        public async Task<Stream> GetDBFileReadingStreamAsync()
        {
            string appPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            string dbPath = System.IO.Path.Combine(appPath, "hellonerd.db");

            return new System.IO.FileStream(dbPath, System.IO.FileMode.Open);
        }
        //---------------------------------------------------------------------------------------
        public async Task<Stream> GetDBFileWritingStreamAsync()
        {
            string appPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            string dbPath = System.IO.Path.Combine(appPath, "hellonerd.db");

            return new System.IO.FileStream(dbPath, System.IO.FileMode.Create); // FileMode.Create - создать файл. Если уже существует, то переписать.
        }
        //---------------------------------------------------------------------------------------
        //---------------------------------------------------------------------------------------
        //---------------------------------------------------------------------------------------
    }
}