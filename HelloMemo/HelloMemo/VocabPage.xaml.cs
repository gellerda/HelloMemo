using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace HelloMemo
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class VocabPage : ContentPage
	{
		public VocabPage ()
		{
			InitializeComponent ();
            BindingContext = VocabVM.Instance;
        }
    }
}