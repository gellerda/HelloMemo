using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace HelloMemo.MyControls
{
    // Контрол, порожденный от Editor, которыйЖ
    // 1. автоматически увеличивает высоту в зависимости от количества строк.
    // 2. задает выравнивание текста по центру.

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

    //For Android, look at ExpandableCenteredEditorRenderer class in Android project.

    public class ExpandableCenteredEditor : Editor
    {
        public ExpandableCenteredEditor()
        {
            TextChanged += OnTextChanged;
        }

        ~ExpandableCenteredEditor()
        {
            TextChanged -= OnTextChanged;
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            InvalidateMeasure();
        }
    }
}
