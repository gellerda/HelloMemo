﻿using System;
using Android.App;
using Android.Content.PM;
using Android.OS;
using System.Diagnostics; // Debug.WriteLine("Some text");
using Xamarin.Auth;
using System.Threading.Tasks;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using System.Threading;

namespace HelloMemo.Droid
{
    public interface ITokenIssuer<T>
    {
        T GetToken();
    }


    [Activity(Label = "HelloMemo", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            MyConfig.PathApp = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            FileAccessHelper.CopyDBFile();
            LoadApplication(new App());

            Clouds.GD.InitAuth("530287818664-s3j72akh3flg65r2sqghobqjka9d9aqi.apps.googleusercontent.com", new Uri("com.hellomemo:/oauth2redirect"));

            VocabVM.AuthGoAsync = () =>
            {
                return Task.Run( () =>
                    {
                        Clouds.GD.AuthCompletedHandle = new AutoResetEvent(false);

                        // Display the activity handling the authentication
                        var intent = Clouds.GD.Auth.GetUI(this);
                        Xamarin.Auth.CustomTabsConfiguration.CustomTabsClosingMessage = null;
                        StartActivity(intent);

                        Clouds.GD.AuthCompletedHandle.WaitOne();
                    });
            };

        }
    }
}

