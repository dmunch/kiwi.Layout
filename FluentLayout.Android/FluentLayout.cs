using System.Collections.Generic;
using Android.Content;
using Android.Views;

using Cirrious.FluentLayouts;
using FluentLayout.Cassowary;
using View = Android.Views;

namespace FluentLayout.Android
{
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
}

