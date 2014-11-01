using System.Linq;
using System.Collections.Generic;
using MonoTouch.UIKit;

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

		public static void AddConstraints(this UIView view, params UIFluentLayout[] fluentLayouts)
		{
			view.AddConstraints(fluentLayouts
				.Where(fluent => fluent != null)
				.SelectMany(fluent => fluent.ToLayoutConstraints())
				.ToArray());
		}

		public static void AddConstraints(this UIView view, IEnumerable<UIFluentLayout> fluentLayouts)
		{
			view.AddConstraints(fluentLayouts
				.Where(fluent => fluent != null)
				.SelectMany(fluent => fluent.ToLayoutConstraints())
				.ToArray());
		}
	}
}

