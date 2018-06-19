using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;


namespace HelloMemo.Behaviors
{
    public class EditorValidationBehavior : Behavior<Editor>
    {
        public string PropertyName { get; set; }
        //--------------------------------------------------------------------------------------------------
        protected override void OnAttachedTo(Editor editor)
        {
            editor.TextChanged += OnEditorTextChanged;
            base.OnAttachedTo(editor);
        }
        //--------------------------------------------------------------------------------------------------
        protected override void OnDetachingFrom(Editor editor)
        {
            editor.TextChanged -= OnEditorTextChanged;
            base.OnDetachingFrom(editor);
        }
        //--------------------------------------------------------------------------------------------------
        void OnEditorTextChanged(object sender, TextChangedEventArgs args)
        {
            double result;
            bool isValid = double.TryParse(args.NewTextValue, out result);
            VocabVM vm = (VocabVM)(((Editor)sender).BindingContext);

            if (vm != null && !String.IsNullOrEmpty(PropertyName))
            {
                var errors = vm.GetErrors(PropertyName);
                if (errors != null)
                {
                    ((Editor)sender).BackgroundColor = Color.FromHex("#F4ABBA");
                }
                else ((Editor)sender).BackgroundColor = Color.Default;
            }
        }
        //--------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------
    }
}
