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
		protected FluentEngine<View.View> fluentEngine;
        protected Dictionary<View.View, IFluentLayout<View.View>> _heightConstraints = new Dictionary<View.View, IFluentLayout<View.View>>();

        protected List<View.View> ChildViews { get; set; }

		public FluentLayout(Context context)
			:base(context)
		{
            ChildViews = new List<View.View>();
            fluentEngine = new FluentEngine<View.View>(this, new AndroidViewEngine());
		}

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            var widthSpec = MeasureSpec.GetMode(widthMeasureSpec);
            var heightSpec = MeasureSpec.GetMode(heightMeasureSpec);

            var width = (this.Parent as ViewGroup).Width - 50;            
            var height = (int) fluentEngine.MeasureHeight(this, width);

            MeasureChildViews();            
            height = (int)fluentEngine.MeasureHeight(this);

            this.SetMeasuredDimension(width, height);
        }

        protected float MeasureHeight(View.View tv)
        {
            
            var width = (int)fluentEngine.MeasuredWidth(tv);

            var widthSpec = MeasureSpec.MakeMeasureSpec(width, MeasureSpecMode.Exactly);
            var heightSpec = MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified);
            tv.Measure(widthSpec, heightSpec);
            
            return tv.MeasuredHeight;
        }

		protected override void OnLayout (bool changed, int l, int t, int r, int b)
		{
            //SolveAndMeasure();
            if (!changed) return;
            fluentEngine.SetValues();
		}

        protected void MeasureChildViews()
        {
            //measure height for each text view after first layout cycle, once we know their widths
            //add constraints for new textviews if any
            
            var newConstraints = ChildViews.Except(_heightConstraints.Keys).Select(tv => tv.Height().GreaterThanOrEqualTo(MeasureHeight(tv))).ToArray();
            fluentEngine.AddConstraints(newConstraints);

            //update constraints for existing textviews
            fluentEngine.SetEditedValues(_heightConstraints.Values, _heightConstraints.Values.Select(hc => (double)MeasureHeight(hc.View)));

            foreach (var newConstraint in newConstraints)
            {
                _heightConstraints.Add(newConstraint.View, newConstraint);
            }
        }

		public void AddConstraints(params IFluentLayout<View.View>[] fluentLayouts)
		{
            this.fluentEngine.AddConstraints(fluentLayouts);
		}

        public void RemoveAllConstraints()
        {
            foreach(var cv in ChildViews)
            {
                cv.Dispose();
            }
            this.ChildViews.Clear();

            this._heightConstraints.Clear();
            this.fluentEngine.RemoveAllConstraints();
        }
	}
}

