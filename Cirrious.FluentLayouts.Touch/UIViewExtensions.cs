using System.Linq;
using System.Collections.Generic;
using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace Cirrious.FluentLayouts.Touch
{
	public static class UIViewExtensions
	{
		public static void SubviewsDoNotTranslateAutoresizingMaskIntoConstraints(this UIView view)
		{
			foreach (var subview in view.Subviews)
			{
				subview.TranslatesAutoresizingMaskIntoConstraints = false;
			}
		}

		public static void AddConstraints<T>(this UIView view, params IFluentLayout<T>[] fluentLayouts)
			where T: NSObject
		{
			view.AddConstraints(fluentLayouts
				.Where(fluent => fluent != null)
				.SelectMany(fluent => fluent.ToLayoutConstraints())
				.ToArray());
		}

		public static void AddConstraints<T>(this UIView view, IEnumerable<IFluentLayout<T>> fluentLayouts)
			where T: NSObject
		{
			view.AddConstraints(fluentLayouts
				.Where(fluent => fluent != null)
				.SelectMany(fluent => fluent.ToLayoutConstraints())
				.ToArray());
		}
	}

	public static class FluentLayoutExtensions
	{
		public static IEnumerable<NSLayoutConstraint> ToLayoutConstraints(this IFluentLayout<NSObject> fluentLayout)
		{
			var constraint = NSLayoutConstraint.Create(
				fluentLayout.View,
				(NSLayoutAttribute)((int)fluentLayout.Attribute),
				(NSLayoutRelation)((int)fluentLayout.Relation),
				fluentLayout.SecondItem.View,
				(NSLayoutAttribute)((int)fluentLayout.SecondItem.Attribute),
				fluentLayout.Multiplier,
				fluentLayout.Constant);

			constraint.Priority = fluentLayout.Priority;

			yield return constraint;
		}
	}
}

