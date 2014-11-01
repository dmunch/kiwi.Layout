using Cirrious.FluentLayouts;
using Cirrious.FluentLayouts.Touch;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Touch.Views;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.UIKit;

using QuickLayout.Core.ViewModels;

namespace QuickLayout.Touch.Views
{
    [Register("FormView")]
    public class FormView : MvxViewController
    {
        public override void ViewDidLoad()
        {
            View.BackgroundColor = UIColor.White;
            base.ViewDidLoad();

            // ios7 layout
            if (RespondsToSelector(new Selector("edgesForExtendedLayout")))
                EdgesForExtendedLayout = UIRectEdge.None;

            var fNameLabel = new UILabel {Text = "First"};
            Add(fNameLabel);

            var sNameLabel = new UILabel {Text = "Last"};
            Add(sNameLabel);

            var numberLabel = new UILabel {Text = "#"};
            Add(numberLabel);

            var streetLabel = new UILabel {Text = "Street"};
            Add(streetLabel);

            var townLabel = new UILabel {Text = "Town"};
            Add(townLabel);

            var zipLabel = new UILabel {Text = "Zip"};
            Add(zipLabel);

            var fNameField = new UITextField() { BackgroundColor = UIColor.LightGray, BorderStyle = UITextBorderStyle.RoundedRect };
            Add(fNameField);

            var sNameField = new UITextField() { BackgroundColor = UIColor.LightGray, BorderStyle = UITextBorderStyle.RoundedRect };
            Add(sNameField);

            var numberField = new UITextField() { BackgroundColor = UIColor.LightGray, BorderStyle = UITextBorderStyle.RoundedRect };
            Add(numberField);

            var streetField = new UITextField() { BackgroundColor = UIColor.LightGray, BorderStyle = UITextBorderStyle.RoundedRect };
            Add(streetField);

            var townField = new UITextField() { BackgroundColor = UIColor.LightGray, BorderStyle = UITextBorderStyle.RoundedRect };
            Add(townField);

            var zipField = new UITextField() { BackgroundColor = UIColor.LightGray, BorderStyle = UITextBorderStyle.RoundedRect };
            Add(zipField);

            var debug = new UILabel() { BackgroundColor = UIColor.White, Lines = 0 };
            Add(debug);

            var set = this.CreateBindingSet<FormView, FormViewModel>();
            set.Bind(fNameField).To(vm => vm.FirstName);
            set.Bind(sNameField).To(vm => vm.LastName);
            set.Bind(numberField).To(vm => vm.Number);
            set.Bind(streetField).To(vm => vm.Street);
            set.Bind(townField).To(vm => vm.Town);
            set.Bind(zipField).To(vm => vm.Zip);
            set.Bind(debug).To("FirstName  + ' ' + LastName + ', '  + Number + ' ' + Street + ' ' + Town + ' ' + Zip");
            set.Apply();

            View.SubviewsDoNotTranslateAutoresizingMaskIntoConstraints();

            var hMargin = 10;
            var vMargin = 10;


			View.AddConstraints(

                fNameLabel.AtTopOf(View, vMargin),
                fNameLabel.AtLeftOf(View, hMargin),
                fNameLabel.ToLeftOf(sNameLabel, hMargin),

                sNameLabel.WithSameTop(fNameLabel),
                sNameLabel.AtRightOf(View, hMargin),
                sNameLabel.WithSameWidth(fNameLabel),

				fNameField.WithSameWidth<NSObject>(fNameLabel),
				fNameField.WithSameLeft<NSObject>(fNameLabel),
				fNameField.Below<NSObject>(fNameLabel, vMargin),

				sNameField.WithSameLeft<NSObject>(sNameLabel),
				sNameField.WithSameWidth<NSObject>(sNameLabel),
                sNameField.WithSameTop(fNameField),

                numberLabel.WithSameLeft(fNameLabel),
                numberLabel.ToLeftOf(streetLabel, hMargin),
				numberLabel.Below<NSObject>(fNameField, vMargin),
                numberLabel.WithRelativeWidth(streetLabel, 0.3f),

                streetLabel.WithSameTop(numberLabel),
                streetLabel.AtRightOf(View, hMargin),

				numberField.WithSameLeft<NSObject>(numberLabel),
				numberField.WithSameWidth<NSObject>(numberLabel),
				numberField.Below<NSObject>(numberLabel, vMargin),

				streetField.WithSameLeft<NSObject>(streetLabel),
				streetField.WithSameWidth<NSObject>(streetLabel),
				streetField.WithSameTop<NSObject>(numberField),

                townLabel.WithSameLeft(fNameLabel),
                townLabel.WithSameRight(streetLabel),
				townLabel.Below<NSObject>(numberField, vMargin),

				townField.WithSameLeft<NSObject>(townLabel),
				townField.WithSameWidth<NSObject>(townLabel),
				townField.Below<NSObject>(townLabel, vMargin),

                zipLabel.WithSameLeft(fNameLabel),
                zipLabel.WithSameWidth(townLabel),
				zipLabel.Below<NSObject>(townField, vMargin),

				zipField.WithSameLeft<NSObject>(townLabel),
				zipField.WithSameWidth<NSObject>(zipLabel),
				zipField.Below<NSObject>(zipLabel, vMargin),

                debug.WithSameLeft(townLabel),
                debug.WithSameWidth(zipLabel),
                debug.AtBottomOf(View, vMargin)

                );
        }
    }
}