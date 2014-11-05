using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;


using FluentLayout.Cassowary;
using View = Android.Views;
using Cirrious.FluentLayouts;
using System.Collections.Generic;


namespace FluentLayout.Android
{
	public class AndroidViewEngine : IViewEngine<View.View>
	{
		#region IViewEngine implementation

		public string GetViewName (View.View view)
		{
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
			default:
				throw new NotImplementedException (string.Format ("Attribute not implemented: {0}", attribute));
			}
		}

		#endregion
	}

	public class FluentLayout : ViewGroup
	{
		protected FluentEngine<View.View> fluentEngine = new FluentEngine<View.View>(new AndroidViewEngine());
		protected List<IFluentLayout<View.View>> fluentLayouts = new List<IFluentLayout<View.View>>();

		public FluentLayout(Context context)
			:base(context)
		{

		}
		protected override void OnLayout (bool changed, int l, int t, int r, int b)
		{
			fluentEngine.AddConstraints (this, fluentLayouts);
			var tableau = fluentEngine.Solve (this);


			System.Diagnostics.Debug.WriteLine (tableau);
			System.Diagnostics.Debug.WriteLine ("l {0} t {1} r {2} b {3}", l, t, r, b);
			/*
			for (int childIdx = 0; childIdx < ChildCount; childIdx++) {
				var child = GetChildAt (childIdx);

				child.Left = 0;
				child.Top = 0;
				child.Right = 300;
				child.Bottom = 300;
			}*/
		}

		public void AddConstraints(params IFluentLayout<View.View>[] fluentLayouts)
		{
			this.fluentLayouts.AddRange (fluentLayouts);
		}
	}

	[Activity (Label = "FluentLayout.Android", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity
	{
		int count = 1;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);


			var button = new Button (this);
			var button2 = new Button (this);
			var fluentLayout = new FluentLayout (this);

			button.Text = "Button ";
			button.Id = 1;
			button2.Text = "Button 1";
			button.Id = 2;

			fluentLayout.AddView (button);
			fluentLayout.AddView (button2);

			// Set our view from the "main" layout resource
			SetContentView (fluentLayout);

			fluentLayout.AddConstraints (
				button.AtTopOf<View.View>(fluentLayout),
				button.WithSameWidth<View.View>(fluentLayout),
				button.Height().EqualTo(300),
				button2.Below(button),
				button2.WithRelativeWidth(button, 0.5f),
				button2.WithSameHeight(button)
			);
			/*
			SetContentView (Resource.Layout.Main);

			// Get our button from the layout resource,
			// and attach an event to it
			Button button = FindViewById<Button> (Resource.Id.myButton);
			*/
			button.Click += delegate {
				button.Text = string.Format ("{0} clicks!", count++);
			};
		}
	}
}


