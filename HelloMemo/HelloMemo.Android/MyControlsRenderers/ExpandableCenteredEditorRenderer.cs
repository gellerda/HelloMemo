using Android.Content;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(HelloMemo.MyControls.ExpandableCenteredEditor), typeof(HelloMemo.Droid.MyControlsRenderers.ExpandableCenteredEditorRenderer))]
namespace HelloMemo.Droid.MyControlsRenderers
{
    public class ExpandableCenteredEditorRenderer : EditorRenderer
    {
        public ExpandableCenteredEditorRenderer(Context context) : base(context)
        {
            //AutoPackage = false;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Editor> args)
        {
            base.OnElementChanged(args);

            if (Control != null)
            {
                Control.TextAlignment = Android.Views.TextAlignment.Center;
                Control.Gravity = Android.Views.GravityFlags.Center;
                Control.Hint = "Hello motherfucker";
            }
        }
    }
}