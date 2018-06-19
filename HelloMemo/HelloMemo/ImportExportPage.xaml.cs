using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics; // Debug.WriteLine("Some text");
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Auth;


/*using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;*/
using System.IO;
using System.Threading;

using System.Reflection;

namespace HelloMemo
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ImportExportPage : ContentPage
	{
		public ImportExportPage ()
		{
			InitializeComponent ();
            BindingContext = VocabVM.Instance;
        }
    }
}