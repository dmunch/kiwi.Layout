// UIViewAndLayoutAttribute.cs
// (c) Copyright Cirrious Ltd. http://www.cirrious.com
// MvvmCross is licensed using Microsoft Public License (Ms-PL)
// Contributions and inspirations noted in readme.md and license.txt
// 
// Project Lead - Stuart Lodge, @slodge, me@slodge.com

using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace Cirrious.FluentLayouts.Touch
{
	public enum LayoutAttribute
	{
		NoAttribute,
		Left,
		Right,
		Top,
		Bottom,
		Leading,
		Trailing,
		Width,
		Height,
		CenterX,
		CenterY,
		Baseline,
		LastBaseline = 11,
		FirstBaseline,
		LeftMargin,
		RightMargin,
		TopMargin,
		BottomMargin,
		LeadingMargin,
		TrailingMargin,
		CenterXWithinMargins,
		CenterYWithinMargins
	}
		
	public abstract class ViewAndLayoutAttribute<T>
	{
		public ViewAndLayoutAttribute(T view, LayoutAttribute attribute)
		{
			Attribute = attribute;
			View = view;
		}

		public T View { get; private set; }
		public LayoutAttribute Attribute { get; private set; }

		public abstract IFluentLayout<T> EqualTo (float constant = 0f);
		public abstract IFluentLayout<T> GreaterThanOrEqualTo (float constant = 0f);
		public abstract IFluentLayout<T> LessThanOrEqualTo(float constant = 0f);
	}

	public class UIViewAndLayoutAttribute : ViewAndLayoutAttribute<NSObject>
    {
        public UIViewAndLayoutAttribute(NSObject view, LayoutAttribute attribute)
			:base(view, attribute)
		{
			nsAttribute = (NSLayoutAttribute)((int)attribute);
        }
		NSLayoutAttribute nsAttribute;

		public override IFluentLayout<NSObject> EqualTo(float constant = 0f)
        {
            return new FluentLayout(View, nsAttribute, NSLayoutRelation.Equal, constant);
        }

		public override IFluentLayout<NSObject> GreaterThanOrEqualTo(float constant = 0f)
        {
			return new FluentLayout(View, nsAttribute, NSLayoutRelation.GreaterThanOrEqual, constant);
        }

		public override IFluentLayout<NSObject> LessThanOrEqualTo(float constant = 0f)
        {
            return new FluentLayout(View, nsAttribute, NSLayoutRelation.LessThanOrEqual, constant);
        }
    }
}