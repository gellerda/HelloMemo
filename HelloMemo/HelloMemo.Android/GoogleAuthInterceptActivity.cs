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

namespace HelloMemo.Droid
{
    [Activity(Label = "GoogleAuthInterceptActivity")] //, LaunchMode =Android.Content.PM.LaunchMode.SingleTask
    [
        IntentFilter
        (
            actions: new[] { Intent.ActionView },
            Categories = new[]
            {
                    Intent.CategoryDefault,
                    Intent.CategoryBrowsable
            },
            DataSchemes = new[]
            {
                // First part of the redirect url (Package name)
                "com.hellomemo"
            },
            DataPaths = new[]
            {
                // Second part of the redirect url (Path)
                "/oauth2redirect"
            }
        )
    ]
    public class GoogleAuthInterceptActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Android.Net.Uri uri_android = Intent.Data;

            // Convert iOS NSUrl to C#/netxf/BCL System.Uri - common API
            Uri uri_netfx = new Uri(uri_android.ToString());

            // Send the URI to the Authenticator for continuation
            Clouds.GD.Auth?.OnPageLoading(uri_netfx);

            var intent = new Intent(this, typeof(MainActivity));
            intent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);
            StartActivity(intent);

            Finish();
        }
    }
}