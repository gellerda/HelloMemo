using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Diagnostics; // Debug.WriteLine("Some text");


[assembly: XamlCompilation (XamlCompilationOptions.Compile)]
namespace HelloMemo
{
	public partial class App : Application
	{
		public App ()
		{
            try
            {
                InitializeComponent();
            }
            catch (Exception e)
            {
                Debug.WriteLine("ERROR in LIB Initialize :" + e.Message);
            }

            try
            {
                MainPage = new MainPage();
            }
            catch (Exception e)
            {
                Debug.WriteLine("ERROR in LIB MainPage :" + e.Message);
            }
        }

        protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}
