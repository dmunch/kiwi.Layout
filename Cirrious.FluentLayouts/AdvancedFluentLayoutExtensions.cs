// AdvancedFluentLayoutExtensions.cs
// (c) Copyright Cirrious Ltd. http://www.cirrious.com
// MvvmCross is licensed using Microsoft Public License (Ms-PL)
// Contributions and inspirations noted in readme.md and license.txt
// 
// Project Lead - Stuart Lodge, @slodge, me@slodge.com

using System.Collections.Generic;

namespace Cirrious.FluentLayouts
{
    public static class AdvancedFluentLayoutExtensions
    {
		public static IFluentLayout<T> AtTopOf<T>(this T view, T parentView, float margin = 0f)
        {
            return view.Top().EqualTo().TopOf(parentView).Plus(margin);
        }

		public static IFluentLayout<T> AtLeftOf<T>(this T view, T parentView, float margin = 0f)
        {
            return view.Left().EqualTo().LeftOf(parentView).Plus(margin);
        }

		public static IFluentLayout<T> AtRightOf<T>(this T view, T parentView, float margin = 0f)
        {
            return view.Right().EqualTo().RightOf(parentView).Minus(margin);
        }

		public static IFluentLayout<T> AtBottomOf<T>(this T view, T parentView, float margin = 0f)
        {
            return view.Bottom().EqualTo().BottomOf(parentView).Minus(margin);
        }

		public static IFluentLayout<T> Below<T>(this T view, T previous, float margin = 0f)
        {
            return view.Top().EqualTo().BottomOf(previous).Plus(margin);
        }

		public static IFluentLayout<T> Above<T>(this T view, T previous, float margin = 0f)
        {
            return view.Bottom().EqualTo().TopOf(previous).Minus(margin);
        }

		public static IFluentLayout<T> WithSameLeft<T>(this T view, T previous)
        {
            return view.Left().EqualTo().LeftOf(previous);
        }

		public static IFluentLayout<T> WithSameTop<T>(this T view, T previous)
        {
            return view.Top().EqualTo().TopOf(previous);
        }

		public static IFluentLayout<T> WithSameCenterX<T>(this T view, T previous)
        {
            return view.CenterX().EqualTo().CenterXOf(previous);
        }

		public static IFluentLayout<T> WithSameCenterY<T>(this T view, T previous)
        {
            return view.CenterY().EqualTo().CenterYOf(previous);
        }

		public static IFluentLayout<T> WithSameRight<T>(this T view, T previous)
        {
            return view.Right().EqualTo().RightOf(previous);
        }

		public static IFluentLayout<T> WithSameWidth<T>(this T view, T previous)
        {
            return view.Width().EqualTo().WidthOf(previous);
        }

		public static IFluentLayout<T> WithSameBottom<T>(this T view, T previous)
        {
            return view.Bottom().EqualTo().BottomOf(previous);
        }

		public static IFluentLayout<T> WithRelativeWidth<T>(this T view, T previous, float scale = 1.0f)
        {
            return view.Width().EqualTo().WidthOf(previous).WithMultiplier(scale);
        }

		public static IFluentLayout<T> WithSameHeight<T>(this T view, T previous)
        {
            return view.Height().EqualTo().HeightOf(previous);
        }

		public static IFluentLayout<T> WithRelativeHeight<T>(this T view, T previous, float scale = 1.0f)
        {
            return view.Height().EqualTo().HeightOf(previous).WithMultiplier(scale);
        }

		public static IFluentLayout<T> ToRightOf<T>(this T view, T previous, float margin = 0f)
        {
            return view.Left().EqualTo().RightOf(previous).Plus(margin);
        }

		public static IFluentLayout<T> ToLeftOf<T>(this T view, T previous, float margin = 0f)
        {
            return view.Right().EqualTo().LeftOf(previous).Minus(margin);
        }

		public static IEnumerable<IFluentLayout<T>> FullWidthOf<T>(this T view, T parent, float margin = 0f)
        {
            yield return view.Left().EqualTo().LeftOf(parent).Plus(margin);
            yield return view.Right().EqualTo().RightOf(parent).Minus(margin);
        }

		public static IEnumerable<IFluentLayout<T>> FullHeightOf<T>(this T view, T parent, float margin = 0f)
        {
            yield return view.Top().EqualTo().TopOf(parent).Plus(margin);
            yield return view.Bottom().EqualTo().BottomOf(parent).Minus(margin);
        }

		public static IEnumerable<IFluentLayout<T>> VerticalStackPanelConstraints<T>(this T parentView, Margins margins,
                                                                              params T[] views)
        {
            margins = margins ?? new Margins();

			T previous = default(T);
            foreach (var view in views)
            {
                yield return view.Left().EqualTo().LeftOf(parentView).Plus(margins.Left);
                yield return view.Width().EqualTo().WidthOf(parentView).Minus(margins.Right + margins.Left);
                if (previous != null)
                    yield return view.Top().EqualTo().BottomOf(previous).Plus(margins.VSpacing);
                else
                    yield return view.Top().EqualTo().TopOf(parentView).Plus(margins.Top);
                previous = view;
            }

			/*
            if (parentView is UIScrollView)
                yield return previous.Bottom().EqualTo().BottomOf(parentView).Minus(margins.Bottom);
			*/
		}
    }
}
