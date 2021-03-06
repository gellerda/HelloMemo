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
using Android.Widget;


namespace HelloMemo.Droid
{
    [Activity(Label = "HelloMemo", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);

            GlobalVars.PathApp = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            //Скопируем след. файлы из Assets в локальную папку приложения (PathApp), если их там еще нет:
            FileAccessHelperAndroid.CopyFileFromAssets(GlobalVars.LocalDbFileName, false);
            FileAccessHelperAndroid.CopyFileFromAssets(GlobalVars.MyConfigFileName, false);

            // LoadApplication(new App()); - Невероятно, но так не работает (автоматически сгенерированный код).
            HelloMemo.App myApp = new App(); 
            LoadApplication(myApp);

            // Параметры аутентификации платформ-специфичны:
            Clouds.GD.InitAuthParameters( "530287818664-s3j72akh3flg65r2sqghobqjka9d9aqi.apps.googleusercontent.com", 
                                          "com.hellomemo:/oauth2redirect", "HelloMemo.Android",
                                          () => {
                                                    var intent = Clouds.GD.Auth.GetUI(this);
                                                    Xamarin.Auth.CustomTabsConfiguration.CustomTabsClosingMessage = null;
                                                    StartActivity(intent);
                                                }   
                                        );

            Clouds.YD.InitAuthParameters( "dae6e4457a6a45dcb6a2fce138fb5dad",
                                          "com.hellomemo://oauth.yandex.ru/verification_code", "com.hellomemo", 
                                          "659199eafc2f4d618f5d5888e04eb265",
                                          () => {
                                              var intent = Clouds.YD.Auth.GetUI(this);
                                              Xamarin.Auth.CustomTabsConfiguration.CustomTabsClosingMessage = null;
                                              StartActivity(intent);
                                          }
                                        );

        }
    }
}

