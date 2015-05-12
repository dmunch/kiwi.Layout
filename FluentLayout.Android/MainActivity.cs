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
			button2.Id = 2;

			fluentLayout.AddView (button);
			fluentLayout.AddView (button2);

			fluentLayout.AddConstraints (
				button.WithSameTop<View.View>(fluentLayout),
                button.WithSameLeft<View.View>(fluentLayout),
				button.WithSameWidth<View.View>(fluentLayout),                
				button.Height().EqualTo(300),
				button2.Below(button),
				button2.WithRelativeWidth(button, 0.5f),
                button2.WithRelativeHeight(button, 0.5f),
                button2.WithSameLeft(button)				
			);
            SetContentView(fluentLayout);
			
			button.Click += delegate {
				button.Text = string.Format ("{0} clicks!", count++);
			};
		}
	}
}


