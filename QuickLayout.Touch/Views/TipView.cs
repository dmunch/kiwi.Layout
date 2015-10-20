using System.Drawing;
using Cirrious.FluentLayouts;
using Cirrious.FluentLayouts.Touch;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Touch.Views;
using Foundation;
using UIKit;
using ObjCRuntime;
using QuickLayout.Core.ViewModels;

namespace QuickLayout.Touch.Views
{
    [Register("TipView")]
    public class TipView : MvxViewController
    {
        public override void ViewDidLoad()
        {
            View.BackgroundColor = UIColor.White;
            base.ViewDidLoad();

            // ios7 layout
            if (RespondsToSelector(new Selector("edgesForExtendedLayout")))
                EdgesForExtendedLayout = UIRectEdge.None;

            var subTotal = new UITextField() { BorderStyle = UITextBorderStyle.RoundedRect };
            subTotal.KeyboardType = UIKeyboardType.DecimalPad;
            Add(subTotal);

            var seek = new UISlider()
                {
                    MinValue = 0,
                    MaxValue = 100,
                };
            Add(seek);

            var seekLabel = new UILabel();
            Add(seekLabel);

            var tipLabel = new UILabel();
            Add(tipLabel);

            var totalLabel = new UILabel();
            Add(totalLabel);

            var set = this.CreateBindingSet<TipView, TipViewModel>();
            set.Bind(subTotal).To(vm => vm.SubTotal);
            set.Bind(seek).To(vm => vm.Generosity);
            set.Bind(seekLabel).To(vm => vm.Generosity);
            set.Bind(tipLabel).To(vm => vm.Tip);
            set.Bind(totalLabel).To("SubTotal + Tip");
            set.Apply();

            View.SubviewsDoNotTranslateAutoresizingMaskIntoConstraints();

            var margin = 10;
            View.AddConstraints(
                    subTotal.AtLeftOf(View, margin),
                    subTotal.AtTopOf(View, margin),
                    subTotal.AtRightOf(View, margin),

                    seek.WithSameLeft<NSObject>(subTotal),
					seek.Below<NSObject>(subTotal, margin),
					seek.ToLeftOf<NSObject>(seekLabel, margin),
					seek.WithRelativeWidth<NSObject>(seekLabel, 3),

					seekLabel.WithSameRight<NSObject>(subTotal),
					seekLabel.WithSameTop<NSObject>(seek),

					tipLabel.Below<NSObject>(seek, margin),
					tipLabel.WithSameLeft<NSObject>(seek),
					tipLabel.WithSameWidth<NSObject>(totalLabel),

                    totalLabel.WithSameTop(tipLabel),
                    totalLabel.ToRightOf(tipLabel, margin),
					totalLabel.WithSameRight<NSObject>(subTotal)
                );
        }
    }
}