// FluentLayoutExtensions.cs
// (c) Copyright Cirrious Ltd. http://www.cirrious.com
// MvvmCross is licensed using Microsoft Public License (Ms-PL)
// Contributions and inspirations noted in readme.md and license.txt
// 
// Project Lead - Stuart Lodge, @slodge, me@slodge.com

using System.Collections.Generic;
using System.Linq;

namespace kiwi.Layout.Fluent
{
    public static class FluentLayoutExtensions
    {
		public static IViewAndLayoutAttribute<T> Left<T>(this T view)
        {
            return view.WithLayoutAttribute(LayoutAttribute.Left);
        }

		public static IViewAndLayoutAttribute<T> Right<T>(this T view)
        {
            return view.WithLayoutAttribute(LayoutAttribute.Right);
        }

		public static IViewAndLayoutAttribute<T> Top<T>(this T view)
        {
            return view.WithLayoutAttribute(LayoutAttribute.Top);
        }

		public static IViewAndLayoutAttribute<T> Bottom<T>(this T view)
        {
            return view.WithLayoutAttribute(LayoutAttribute.Bottom);
        }

		public static IViewAndLayoutAttribute<T> Baseline<T>(this T view)
        {
            return view.WithLayoutAttribute(LayoutAttribute.Baseline);
        }

		public static IViewAndLayoutAttribute<T> Trailing<T>(this T view)
        {
            return view.WithLayoutAttribute(LayoutAttribute.Trailing);
        }

		public static IViewAndLayoutAttribute<T> Leading<T>(this T view)
        {
            return view.WithLayoutAttribute(LayoutAttribute.Leading);
        }

		public static IViewAndLayoutAttribute<T> CenterX<T>(this T view)
        {
            return view.WithLayoutAttribute(LayoutAttribute.CenterX);
        }

		public static IViewAndLayoutAttribute<T> CenterY<T>(this T view)
        {
            return view.WithLayoutAttribute(LayoutAttribute.CenterY);
        }

		public static IViewAndLayoutAttribute<T> Height<T>(this T view)
        {
            return view.WithLayoutAttribute(LayoutAttribute.Height);
        }

		public static IViewAndLayoutAttribute<T> Width<T>(this T view)
        {
            return view.WithLayoutAttribute(LayoutAttribute.Width);
        }

		public static IViewAndLayoutAttribute<T> WithLayoutAttribute<T>(this T view, LayoutAttribute attribute)
        {
			return new ViewAndLayoutAttribute<T> (view, attribute);
        }
    }
}
