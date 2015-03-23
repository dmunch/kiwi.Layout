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
	[Activity (Label = "FluentLayout.Android", MainLauncher = true)]
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


