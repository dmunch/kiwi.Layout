using System;
using System.Collections.Generic;
using Cassowary;
using Cirrious.FluentLayouts;

namespace FluentLayout.Cassowary
{
	public class ViewEngine : IViewEngine<View>
	{
		#region IViewEngine implementation

		public string GetViewName (View view)
		{
			return view.Color.ToString();
		}

		public float GetAttribute (View v, LayoutAttribute attribute)
		{
			switch (attribute) {
			case LayoutAttribute.Left:
				return v.Left;
				break;
			case LayoutAttribute.Right:
				return v.Right;
				break;
			case LayoutAttribute.Top:
				return v.Top;
				break;
			case LayoutAttribute.Bottom:
				return v.Bottom;
				break;
            case LayoutAttribute.Width:
                return v.Width;
            case LayoutAttribute.Height:
                return v.Height;
			default:
				throw new NotImplementedException(string.Format("Attribute not implemented: {0}", attribute));
			}
		}

		public void SetAttribute (View v, LayoutAttribute attribute, float value)
		{
			int intVal = (int)value;

			switch (attribute) {
			case LayoutAttribute.Left:
				v.Left = intVal;
				break;
			case LayoutAttribute.Right:
				v.Right = intVal;
				break;
			case LayoutAttribute.Top:
				v.Top = intVal;
				break;
			case LayoutAttribute.Bottom:
				v.Bottom = intVal;
				break;
            case LayoutAttribute.Width:
            case LayoutAttribute.Height:
                break;
			default:
				throw new NotImplementedException (string.Format ("Attribute not implemented: {0}", attribute));
			}
		}
		#endregion
	}

	public static class ViewExtensions
	{
		static FluentEngine<View> fluentEngine = new FluentEngine<View>(new ViewEngine());

		public static string AddConstraints<T> (this View view, params IFluentLayout<T>[] fluentLayouts)
			where T:View
		{
			fluentEngine.AddConstraints (view, fluentLayouts);
			return fluentEngine.Solve (view);
		}

        public static void UpdateConstraints<T>(this View view, IEnumerable<IFluentLayout<T>> constraints, IEnumerable<double> values)
            where T:View
        {
            fluentEngine.SetEditedValues(view, constraints, values);
        }

        public static void SetValues()
        {
            fluentEngine.SetValues();
        }
	}
}

