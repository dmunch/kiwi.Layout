using System.Collections.Generic;
using Android.Content;
using Android.Views;

using Cirrious.FluentLayouts;
using FluentLayout.Cassowary;
using View = Android.Views;

using System.Linq;
using Android.Widget;
using System;

namespace FluentLayout.Android
{
	public class FluentLayout : ViewGroup
	{
		protected FluentEngine<View.View> fluentEngine = new FluentEngine<View.View>(new AndroidViewEngine());		
        protected Dictionary<TextView, IFluentLayout<TextView>> _heightConstraints = new Dictionary<TextView, IFluentLayout<TextView>>();

        public IEnumerable<TextView> TextViews { get; set; }

		public FluentLayout(Context context)
			:base(context)
		{
            TextViews = Enumerable.Empty<TextView>();
		}

		protected override void OnLayout (bool changed, int l, int t, int r, int b)
		{
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
			var tableau = fluentEngine.Solve (this);
            var timeFirstPass = sw.Elapsed;
            sw.Restart();
	
            Func<TextView, float> measureHeight = tv =>
            {
                var left  = fluentEngine.GetValue(tv, LayoutAttribute.Left);
                var right = fluentEngine.GetValue(tv, LayoutAttribute.Right);
                var width = right - left;
                var widthSpec = MeasureSpec.MakeMeasureSpec((int)width, MeasureSpecMode.Exactly);
                var heightSpec = MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified);
                tv.Measure(widthSpec, heightSpec);

                return tv.MeasuredHeight;
            };

            //measure height for each text view after first layout cycle, once we know their widths
            //add constraints for new textviews if any
            var newConstraints = TextViews.Except(_heightConstraints.Keys).Select(tv => tv.Height().EqualTo(measureHeight(tv))).ToArray();
            fluentEngine.AddConstraints(this, newConstraints);
            
            //update constraints for existing textviews
            fluentEngine.SetEditedValues(this, _heightConstraints.Values, _heightConstraints.Values.Select(hc => (double)measureHeight(hc.View)));

            foreach(var newConstraint in newConstraints)
            {
                _heightConstraints.Add(newConstraint.View, newConstraint);
            }
            
            var timeMeasurement = sw.Elapsed;
            sw.Restart();

            //solve the system again with the updated height constraints
            //fluentEngine.Solve(this);

            fluentEngine.SetValues();
            var timeSecondPass = sw.Elapsed;
		}        

		public void AddConstraints(params IFluentLayout<View.View>[] fluentLayouts)
		{
            this.fluentEngine.AddConstraints(this, fluentLayouts);
		}
	}
}

