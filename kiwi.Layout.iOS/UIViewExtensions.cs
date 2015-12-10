using System.Linq;
using System.Collections.Generic;
using UIKit;
using Foundation;
using System;

namespace kiwi.Layout.Fluent
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
            where T: UIView
        {
            view.AddConstraints(fluentLayouts
                .Where(fluent => fluent != null)
                .Select(fluent => fluent.ToLayoutConstraint())
                .ToArray());
        }

        public static void AddConstraints<T>(this UIView view, IEnumerable<IFluentLayout<T>> fluentLayouts)
            where T: UIView
        {
            view.AddConstraints(fluentLayouts
                .Where(fluent => fluent != null)
                .Select(fluent => fluent.ToLayoutConstraint())
                .ToArray());
        }
    }

    public static class FluentLayoutExtensions
    {
        public static NSLayoutConstraint ToLayoutConstraint(this IFluentLayout<UIView> fluentLayout)
        {
            var constraint = NSLayoutConstraint.Create (
                fluentLayout.View,  
                (NSLayoutAttribute)fluentLayout.Attribute,
                (NSLayoutRelation)fluentLayout.Relation,
                fluentLayout.SecondItem != null ? fluentLayout.SecondItem.View : null, // the beauty of C# 6 
                //no idea however how to deal with value types (e.g. enum)
                (NSLayoutAttribute)(fluentLayout.SecondItem != null ? fluentLayout.SecondItem.Attribute : LayoutAttribute.NoAttribute),
                fluentLayout.Multiplier,
                fluentLayout.Constant);

            //clamp on 1000
            constraint.Priority = Math.Min(fluentLayout.Priority, 1000);

            return constraint;
        }
    }
}