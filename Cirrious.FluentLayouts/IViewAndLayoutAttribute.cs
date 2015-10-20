// UIViewAndLayoutAttribute.cs
// (c) Copyright Cirrious Ltd. http://www.cirrious.com
// MvvmCross is licensed using Microsoft Public License (Ms-PL)
// Contributions and inspirations noted in readme.md and license.txt
// 
// Project Lead - Stuart Lodge, @slodge, me@slodge.com

namespace Cirrious.FluentLayouts
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
		
	public interface IViewAndLayoutAttribute<out T>
	{
		T View { get; }
		LayoutAttribute Attribute { get; }

		IFluentLayout<T> EqualTo (float constant = 0f);
		IFluentLayout<T> GreaterThanOrEqualTo (float constant = 0f);
		IFluentLayout<T> LessThanOrEqualTo(float constant = 0f);
	}

	public class ViewAndLayoutAttribute<T> : IViewAndLayoutAttribute<T>
	{
		public ViewAndLayoutAttribute(T view, LayoutAttribute layoutAttribute)
		{
			this.View = view;
			this.Attribute = layoutAttribute;
		}

		public T View { get; protected set; }
		public LayoutAttribute Attribute { get; set; }
	
		public IFluentLayout<T> EqualTo(float constant = 0f)
		{
			return new FluentLayout<T>(View, Attribute, LayoutRelation.Equal, constant);
		}

		public IFluentLayout<T> GreaterThanOrEqualTo(float constant = 0f)
		{
			return new FluentLayout<T>(View, Attribute, LayoutRelation.GreaterThanOrEqual, constant);
		}

		public IFluentLayout<T> LessThanOrEqualTo(float constant = 0f)
		{
			return new FluentLayout<T>(View, Attribute, LayoutRelation.LessThanOrEqual, constant);
		}
	}
}