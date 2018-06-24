using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace HelloMemo.MyControls
{
    // Контрол, порожденный от Editor, который автоматически увеличивает высоту в зависимости от количества строк.
    // https://forums.xamarin.com/discussion/92439/automatic-expandable-editor
    // For iOS, create a custom renderer for disable scroll inside control.This will permit editor expands:
    /*
    using Core;
    using Core.iOS;
    using Xamarin.Forms;
    using Xamarin.Forms.Platform.iOS;

    [assembly: ExportRenderer(typeof(ExpandableEditor), typeof(ExpandableEditorRenderer))]
    namespace Core.iOS
        {
            public class ExpandableEditorRenderer : EditorRenderer
            {
                protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Editor> e)
                {
                    base.OnElementChanged(e);

                    if (Control != null)
                        Control.ScrollEnabled = false;
                }
            }
        }*/

    //For Android no custom renderer is need.

    public class ExpandableEditor : Editor
    {
        public ExpandableEditor()
        {
            TextChanged += OnTextChanged;
        }

        ~ExpandableEditor()
        {
            TextChanged -= OnTextChanged;
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            InvalidateMeasure();
        }
    }
}
