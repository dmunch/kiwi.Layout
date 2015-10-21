using System;

namespace kiwi.Layout.Fluent
{
	public enum LayoutRelation
	{
		LessThanOrEqual = -1,
		Equal,
		GreaterThanOrEqual
	}

	public interface IFluentLayout<out T>
	{
		T View { get; }

		LayoutAttribute Attribute { get; }
		LayoutRelation Relation { get; }

		IViewAndLayoutAttribute<T> SecondItem { get; }

		float Multiplier { get; }
		float Constant { get; }
		float Priority { get; }

		IFluentLayout<T> Plus (float constant);
		IFluentLayout<T> Minus (float constant);
		IFluentLayout<T> WithMultiplier (float multiplier);
		IFluentLayout<T> SetPriority (float priority);
	}

	public static class IFluentLayoutExtensions
	{

		public static IFluentLayout<T> LeftOf<T>(this IFluentLayout<T> fl, T view2)
		{
			return fl.SetSecondItem (view2, LayoutAttribute.Left);
		}
		public static IFluentLayout<T> RightOf<T>(this IFluentLayout<T> fl, T view2)
		{
			return fl.SetSecondItem(view2, LayoutAttribute.Right);
		}

		public static IFluentLayout<T> TopOf<T>(this IFluentLayout<T> fl, T view2)
		{
			return fl.SetSecondItem(view2, LayoutAttribute.Top);
		}

		public static IFluentLayout<T> BottomOf<T>(this IFluentLayout<T> fl, T view2)
		{
			return fl.SetSecondItem(view2, LayoutAttribute.Bottom);
		}

		public static IFluentLayout<T> BaselineOf<T>(this IFluentLayout<T> fl, T view2)
		{
			return fl.SetSecondItem(view2, LayoutAttribute.Baseline);
		}

		public static IFluentLayout<T> TrailingOf<T>(this IFluentLayout<T> fl, T view2)
		{
			return fl.SetSecondItem(view2, LayoutAttribute.Trailing);
		}

		public static IFluentLayout<T> LeadingOf<T>(this IFluentLayout<T> fl, T view2)
		{
			return fl.SetSecondItem(view2, LayoutAttribute.Leading);
		}

		public static IFluentLayout<T> CenterXOf<T>(this IFluentLayout<T> fl, T view2)
		{
			return fl.SetSecondItem(view2, LayoutAttribute.CenterX);
		}

		public static IFluentLayout<T> CenterYOf<T>(this IFluentLayout<T> fl, T view2)
		{
			return fl.SetSecondItem(view2, LayoutAttribute.CenterY);
		}

		public static IFluentLayout<T> HeightOf<T>(this IFluentLayout<T> fl, T view2)
		{
			return fl.SetSecondItem(view2, LayoutAttribute.Height);
		}

		public static IFluentLayout<T> WidthOf<T>(this IFluentLayout<T> fl, T view2)
		{
			return fl.SetSecondItem(view2, LayoutAttribute.Width);
		}
			
		private static IFluentLayout<T> SetSecondItem<T>(this IFluentLayout<T> fl, T view2, LayoutAttribute attribute2)
		{
			fl.ThrowIfSecondItemAlreadySet();
		
			var second = new FluentLayout<T> (fl);
			second.SecondItem =  new ViewAndLayoutAttribute<T>(view2, attribute2);;
			return second;
		}

		private static void ThrowIfSecondItemAlreadySet<T>(this IFluentLayout<T> fl)
		{
			if (fl.SecondItem != null)
				throw new Exception("You cannot set the second item in a layout relation more than once");
		}
	}
}

