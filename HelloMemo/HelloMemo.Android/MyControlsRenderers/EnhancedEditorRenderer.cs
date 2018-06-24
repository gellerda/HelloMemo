using System.ComponentModel;
using Android.Content;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(HelloMemo.MyControls.EnhancedEditor), typeof(HelloMemo.Droid.MyControlsRenderers.EnhancedEditorRenderer))]
namespace HelloMemo.Droid.MyControlsRenderers
{
    public class EnhancedEditorRenderer : EditorRenderer
    {
        //------------------------------------------------------------------------------------------
        public EnhancedEditorRenderer(Context context) : base(context)
        {
            //AutoPackage = false;
        }
        //------------------------------------------------------------------------------------------
        protected override void OnElementChanged(ElementChangedEventArgs<Editor> args)
        {
            base.OnElementChanged(args);

            if (Control!=null && args.NewElement!=null)
            {
                var element = args.NewElement as MyControls.EnhancedEditor;

                if (element.HorizontalTextAlignment == "Center")
                {
                    Control.TextAlignment = Android.Views.TextAlignment.Center;
                    Control.Gravity = Android.Views.GravityFlags.Center;
                }

                Control.Hint = element.Placeholder;
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
                {
                    Control.TextAlignment = Android.Views.TextAlignment.Center;
                    Control.Gravity = Android.Views.GravityFlags.Center;
                }
                this.Control.Hint = element.Placeholder;
            }
        }
        //------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------
    }
}