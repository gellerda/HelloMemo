using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.IO;

namespace HelloMemo.Droid
{
    public class FileAccessHelperAndroid
    {
        public static void CopyDBFile()
        {
            string appPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            string dbPath = Path.Combine(appPath, "hellonerd.db");
            if (!File.Exists(dbPath))
            {
                //string[] x = Directory.GetFiles(appPath);
                using (var br = new BinaryReader(Application.Context.Assets.Open("hellonerd.db")))
                {
                    using (var bw = new BinaryWriter(new FileStream(dbPath, FileMode.Create)))
                    {
                        byte[] buffer = new byte[2048];
                        int length = 0;
                        while ((length = br.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            bw.Write(buffer, 0, length);
                        }
                    }
                }
            }
        }
    }
}