// FluentLayoutExtensions.cs
// (c) Copyright Cirrious Ltd. http://www.cirrious.com
// MvvmCross is licensed using Microsoft Public License (Ms-PL)
// Contributions and inspirations noted in readme.md and license.txt
// 
// Project Lead - Stuart Lodge, @slodge, me@slodge.com

using System.Collections.Generic;
using System.Linq;
using MonoTouch.UIKit;

namespace Cirrious.FluentLayouts.Touch
{
	public static class FluentLayoutExtensionsNative
	{
		public static void SubviewsDoNotTranslateAutoresizingMaskIntoConstraints(this UIView view)
		{
			foreach (var subview in view.Subviews)
			{
				subview.TranslatesAutoresizingMaskIntoConstraints = false;
			}
		}

		public static void AddConstraints(this UIView view, params FluentLayout[] fluentLayouts)
		{
			view.AddConstraints(fluentLayouts
				.Where(fluent => fluent != null)
				.SelectMany(fluent => fluent.ToLayoutConstraints())
				.ToArray());
		}

		public static void AddConstraints(this UIView view, IEnumerable<FluentLayout> fluentLayouts)
		{
			view.AddConstraints(fluentLayouts
				.Where(fluent => fluent != null)
				.SelectMany(fluent => fluent.ToLayoutConstraints())
				.ToArray());
		}
	}


    public static class FluentLayoutExtensions
    {
    
        public static ViewAndLayoutAttribute<T> Left<T>(this T view)
        {
            return view.WithLayoutAttribute(LayoutAttribute.Left);
        }

		public static ViewAndLayoutAttribute<T> Right<T>(this T view)
        {
            return view.WithLayoutAttribute(LayoutAttribute.Right);
        }

        public static ViewAndLayoutAttribute<T> Top<T>(this T view)
        {
            return view.WithLayoutAttribute(LayoutAttribute.Top);
        }

		public static ViewAndLayoutAttribute<T> Bottom<T>(this T view)
        {
            return view.WithLayoutAttribute(LayoutAttribute.Bottom);
        }

		public static ViewAndLayoutAttribute<T> Baseline<T>(this T view)
        {
            return view.WithLayoutAttribute(LayoutAttribute.Baseline);
        }

		public static ViewAndLayoutAttribute<T> Trailing<T>(this T view)
        {
            return view.WithLayoutAttribute(LayoutAttribute.Trailing);
        }

		public static ViewAndLayoutAttribute<T> Leading<T>(this T view)
        {
            return view.WithLayoutAttribute(LayoutAttribute.Leading);
        }

		public static ViewAndLayoutAttribute<T> CenterX<T>(this T view)
        {
            return view.WithLayoutAttribute(LayoutAttribute.CenterX);
        }

		public static ViewAndLayoutAttribute<T> CenterY<T>(this T view)
        {
            return view.WithLayoutAttribute(LayoutAttribute.CenterY);
        }

		public static ViewAndLayoutAttribute<T> Height<T>(this T view)
        {
            return view.WithLayoutAttribute(LayoutAttribute.Height);
        }

		public static ViewAndLayoutAttribute<T> Width<T>(this T view)
        {
            return view.WithLayoutAttribute(LayoutAttribute.Width);
        }

		public static ViewAndLayoutAttribute<T> WithLayoutAttribute<T>(this T view, LayoutAttribute attribute)
        {
			//TODO ViewAndLayoutAttribute factory or dependency injection?
			return null;
            //return new UIViewAndLayoutAttribute(view, attribute);
        }
    }
}
