using Android.Views;
using System;

namespace kiwi.Layout.Droid
{
	using Fluent;

	public class AndroidViewEngine : IViewEngine<View>
	{
		#region IViewEngine implementation

		public string GetViewName (View view)
		{
            /*
            if(view.Id == 0)
            {
                throw new ArgumentException("Id can't be 0", "Id");
            }*/
			return view.Id.ToString();
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
            case LayoutAttribute.Height:
                return v.Height;
            case LayoutAttribute.Width:
                return v.Width;
            case LayoutAttribute.CenterX:
                return v.Left + v.Width / 2;
            case LayoutAttribute.CenterY:
                return v.Top + v.Height / 2;
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
            case LayoutAttribute.Height:
            case LayoutAttribute.Width:
            case LayoutAttribute.CenterX:
            case LayoutAttribute.CenterY:
                break;

			default:
				throw new NotImplementedException (string.Format ("Attribute not implemented: {0}", attribute));
			}
		}

		#endregion
	}

}

