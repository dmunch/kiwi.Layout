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

        bool measured = false;
        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {

            var widthSpec = MeasureSpec.GetMode(widthMeasureSpec);
            var heightSpec = MeasureSpec.GetMode(heightMeasureSpec);

            var parentWidth = MeasureSpec.GetSize(widthMeasureSpec);
            var parentHeight = MeasureSpec.GetSize(heightMeasureSpec);

            if (measured)
            {
                this.SetMeasuredDimension(parentWidth, (int)fluentEngine.MeasureHeight(this));
                return;
            }
                
            measured = true;
            
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            fluentEngine.MeasureHeight(this, parentWidth);
            MeasureChildViews();
            
            var height = (int)fluentEngine.MeasureHeight(this);
            this.SetMeasuredDimension(parentWidth, height);

            sw.Stop();
            System.Diagnostics.Debug.WriteLine("OnMeasure elapsed time {0}", sw.ElapsedMilliseconds);
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
            if (!changed) return;
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            foreach(var child in ChildViews)
            {
                int vl = (int)fluentEngine.GetValue(child, LayoutAttribute.Left);
                int vr = (int)fluentEngine.GetValue(child, LayoutAttribute.Right);
                int vt = (int)fluentEngine.GetValue(child, LayoutAttribute.Top);
                int vb = (int)fluentEngine.GetValue(child, LayoutAttribute.Bottom);
                
                child.Layout(vl, vt, vr, vb);
            }
            sw.Stop();
            System.Diagnostics.Debug.WriteLine("OnLayout elapsed time {0}", sw.ElapsedMilliseconds);
		}

        protected void MeasureChildViews()
        {
            //measure height for each text view after first layout cycle, once we know their widths
            
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();            
            fluentEngine.SetEditedValues(ChildViews/*.OfType<TextView>()*/.Except(_heightConstraints.Keys).Select(tv => tv.Height().GreaterThanOrEqualTo(MeasureHeight(tv))));
            sw.Stop();

            System.Diagnostics.Debug.WriteLine("MeasureChildViews:Constraints elapsed time {0}", sw.ElapsedMilliseconds);
        }

        public override void AddView(View.View v)
        {
            base.AddView(v);
            fluentEngine.AddConstraint(v.Height().GreaterThanOrEqualTo(0));
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

