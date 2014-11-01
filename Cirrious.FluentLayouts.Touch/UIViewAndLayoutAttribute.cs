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
			return new UIFluentLayout(View, nsAttribute, NSLayoutRelation.Equal, constant);
		}

		public override IFluentLayout<NSObject> GreaterThanOrEqualTo(float constant = 0f)
		{
			return new UIFluentLayout(View, nsAttribute, NSLayoutRelation.GreaterThanOrEqual, constant);
		}

		public override IFluentLayout<NSObject> LessThanOrEqualTo(float constant = 0f)
		{
			return new UIFluentLayout(View, nsAttribute, NSLayoutRelation.LessThanOrEqual, constant);
		}
	}
}