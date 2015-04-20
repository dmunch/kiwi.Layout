using System;

using FluentLayout.Cassowary;
using View = Android.Views;
using Cirrious.FluentLayouts;

namespace FluentLayout.Android
{
	public class AndroidViewEngine : IViewEngine<View.View>
	{
		#region IViewEngine implementation

		public string GetViewName (View.View view)
		{
            /*
            if(view.Id == 0)
            {
                throw new ArgumentException("Id can't be 0", "Id");
            }*/
			return view.Id.ToString();
		}

		public float GetAttribute (View.View v, LayoutAttribute attribute)
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
            case LayoutAttribute.Height:
                return v.Height;
            case LayoutAttribute.Width:
                return v.Width;                
			default:
				throw new NotImplementedException(string.Format("Attribute not implemented: {0}", attribute));
			}
		}

		public void SetAttribute (View.View v, LayoutAttribute attribute, float value)
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
            case LayoutAttribute.Height:
            case LayoutAttribute.Width:
                break;

			default:
				throw new NotImplementedException (string.Format ("Attribute not implemented: {0}", attribute));
			}
		}

		#endregion
	}

}

