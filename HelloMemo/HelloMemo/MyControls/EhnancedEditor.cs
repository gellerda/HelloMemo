using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace HelloMemo.MyControls
{
    // Контрол, порожденный от Editor, который:
    // 1. автоматически увеличивает высоту в зависимости от количества строк.
    // 2. Имеет свойство HorizontalTextAlignment, которое может принимать значение "Center"
    // 3. Имеет свойство string Placeholder, как у класса Entry.

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

    //For Android, look at EnhancedEditorRenderer class in Android project.

    public class EnhancedEditor : Editor
    {
        public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create("Placeholder", typeof(string), typeof(EnhancedEditor), String.Empty);

        public string Placeholder
        {
            get { return (string)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }
        //------------------------------------------------------------------------------------------
        public static readonly BindableProperty HorizontalTextAlignmentProperty = BindableProperty.Create("HorizontalTextAlignment", typeof(string), typeof(EnhancedEditor), String.Empty);

        public string HorizontalTextAlignment
        {
            get { return (string)GetValue(HorizontalTextAlignmentProperty); }
            set { SetValue(HorizontalTextAlignmentProperty, value); }
        }
        //------------------------------------------------------------------------------------------
        public EnhancedEditor()
        {
            TextChanged += OnTextChanged;
        }
        //------------------------------------------------------------------------------------------
        ~EnhancedEditor()
        {
            TextChanged -= OnTextChanged;
        }
        //------------------------------------------------------------------------------------------
        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            InvalidateMeasure();
        }
        //------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------
    }
}
