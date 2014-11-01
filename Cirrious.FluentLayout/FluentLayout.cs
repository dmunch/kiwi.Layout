// FluentLayout.cs
// (c) Copyright Cirrious Ltd. http://www.cirrious.com
// MvvmCross is licensed using Microsoft Public License (Ms-PL)
// Contributions and inspirations noted in readme.md and license.txt
// 
// Project Lead - Stuart Lodge, @slodge, me@slodge.com

using System;
using System.Collections.Generic;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace Cirrious.FluentLayouts.Touch
{
    public class FluentLayout : FluentLayoutGeneric<NSObject>
    {
        public FluentLayout(
            NSObject view,
            NSLayoutAttribute attribute,
            NSLayoutRelation relation,
            NSObject secondItem,
            NSLayoutAttribute secondAttribute)
        {
            View = view;
            Attribute = attribute;
            Relation = relation;
            SecondItem = secondItem;
            SecondAttribute = secondAttribute;
            Multiplier = 1f;
            Priority = (float) UILayoutPriority.Required;
        }

        public FluentLayout(NSObject view,
                            NSLayoutAttribute attribute,
                            NSLayoutRelation relation,
                            float constant = 0f)
        {
            View = view;
            Attribute = attribute;
            Relation = relation;
            Multiplier = 1f;
            Constant = constant;
            Priority = (float) UILayoutPriority.Required;
        }

        public NSObject View { get; private set; }
        public NSLayoutAttribute Attribute { get; private set; }
        public NSLayoutRelation Relation { get; private set; }
        public NSObject SecondItem { get; private set; }
        public NSLayoutAttribute SecondAttribute { get; private set; }
       
        public FluentLayout SetPriority(UILayoutPriority priority)
        {
            Priority = (float) priority;
            return this;
        }

		public override IFluentLayout<NSObject> LeftOf(NSObject view2)
        {
            return SetSecondItem(view2, NSLayoutAttribute.Left);
        }

		public override IFluentLayout<NSObject> RightOf(NSObject view2)
        {
            return SetSecondItem(view2, NSLayoutAttribute.Right);
        }

		public override IFluentLayout<NSObject> TopOf(NSObject view2)
        {
            return SetSecondItem(view2, NSLayoutAttribute.Top);
        }

		public override IFluentLayout<NSObject> BottomOf(NSObject view2)
        {
            return SetSecondItem(view2, NSLayoutAttribute.Bottom);
        }

		public override IFluentLayout<NSObject> BaselineOf(NSObject view2)
        {
            return SetSecondItem(view2, NSLayoutAttribute.Baseline);
        }

		public override IFluentLayout<NSObject> TrailingOf(NSObject view2)
        {
            return SetSecondItem(view2, NSLayoutAttribute.Trailing);
        }

		public override IFluentLayout<NSObject> LeadingOf(NSObject view2)
        {
            return SetSecondItem(view2, NSLayoutAttribute.Leading);
        }

		public override IFluentLayout<NSObject> CenterXOf(NSObject view2)
        {
            return SetSecondItem(view2, NSLayoutAttribute.CenterX);
        }

		public override IFluentLayout<NSObject> CenterYOf(NSObject view2)
        {
            return SetSecondItem(view2, NSLayoutAttribute.CenterY);
        }

		public override IFluentLayout<NSObject> HeightOf(NSObject view2)
        {
            return SetSecondItem(view2, NSLayoutAttribute.Height);
        }

		public override IFluentLayout<NSObject> WidthOf(NSObject view2)
        {
            return SetSecondItem(view2, NSLayoutAttribute.Width);
        }

        private FluentLayout SetSecondItem(NSObject view2, NSLayoutAttribute attribute2)
        {
            ThrowIfSecondItemAlreadySet();
            SecondAttribute = attribute2;
            SecondItem = view2;
            return this;
        }

        private void ThrowIfSecondItemAlreadySet()
        {
            if (SecondItem != null)
                throw new Exception("You cannot set the second item in a layout relation more than once");
        }

        public IEnumerable<NSLayoutConstraint> ToLayoutConstraints()
        {
            var constraint = NSLayoutConstraint.Create(
                View,
                Attribute,
                Relation,
                SecondItem,
                SecondAttribute,
                Multiplier,
                Constant);
            constraint.Priority = Priority;

            yield return constraint;
        }
    }
}