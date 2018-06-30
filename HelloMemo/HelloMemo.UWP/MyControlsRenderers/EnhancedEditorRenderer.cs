using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(HelloMemo.MyControls.EnhancedEditor), typeof(HelloMemo.UWP.MyControlsRenderers.EnhancedEditorRenderer))]
namespace HelloMemo.UWP.MyControlsRenderers
{
    public class EnhancedEditorRenderer : EditorRenderer
    {
        //------------------------------------------------------------------------------------------
        protected override void OnElementChanged(ElementChangedEventArgs<Editor> args)
        {
            base.OnElementChanged(args);

            if (Control != null && args.NewElement != null)
            {
                var element = args.NewElement as MyControls.EnhancedEditor;

                if (element.HorizontalTextAlignment == "Center")
                    Control.TextAlignment = Windows.UI.Xaml.TextAlignment.Center;

                Control.PlaceholderText = element.Placeholder;
                //Control.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch;
                //Control.HorizontalContentAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch;
            }
        }
        //------------------------------------------------------------------------------------------
        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == HelloMemo.MyControls.EnhancedEditor.PlaceholderProperty.PropertyName)
            {
                var element = this.Element as HelloMemo.MyControls.EnhancedEditor;

                if (element.HorizontalTextAlignment == "Center")
                    Control.TextAlignment = Windows.UI.Xaml.TextAlignment.Center;

                this.Control.PlaceholderText = element.Placeholder;
            }
        }
        //------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------
    }
}
