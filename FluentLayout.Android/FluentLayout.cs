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
		protected FluentEngineKiwi<View.View> fluentEngine;
        
        protected IEnumerable<View.View> ChildViews { 
			get
			{
				for (int c = 0; c < this.ChildCount; c++) {
					yield return this.GetChildAt (c);
				}
			}
		}

		public FluentLayout(Context context)
			:base(context)
		{
            fluentEngine = new FluentEngineKiwi<View.View>(this, new AndroidViewEngine());
		}

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            var parentWidth = MeasureSpec.GetSize(widthMeasureSpec);

            //var sw = new System.Diagnostics.Stopwatch();
            //sw.Start();

            fluentEngine.MeasureHeight(this, parentWidth);

			//var measureFirstStep = sw.Elapsed; sw.Restart ();

            MeasureChildViews();
            
			//var measureSecondStep = sw.Elapsed; sw.Restart ();

			this.SetMeasuredDimension(parentWidth, (int)fluentEngine.MeasureHeight(this, parentWidth));

            //sw.Stop();
			//System.Diagnostics.Debug.WriteLine("OnMeasure elapsed time first step {0}, second step {1}, SetMeasuredDimensions {2}", measureFirstStep.TotalMilliseconds, measureSecondStep.TotalMilliseconds, sw.Elapsed.TotalMilliseconds);
        }

		protected void MeasureChildViews()
		{
			//measure height for each text view after first layout cycle, once we know their widths

			var editConstraints = ChildViews.OfType<TextView>().Select (tv => tv.Height ().GreaterThanOrEqualTo (MeasureHeight (tv)));
			fluentEngine.SetEditedValues(editConstraints);
		}

		protected float MeasureHeight(View.View tv)
		{
			if (tv.Visibility == ViewStates.Gone)
				return 0;
			
			var width = (int)fluentEngine.MeasuredWidth(tv);

			var widthSpec = MeasureSpec.MakeMeasureSpec(width, MeasureSpecMode.Exactly);
			var heightSpec = MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified);
			tv.Measure(widthSpec, heightSpec);

			return tv.MeasuredHeight;
		}

		protected override void OnLayout (bool changed, int l, int t, int r, int b)
		{
            if (!changed) return;

			fluentEngine.UpdateVariables();

			foreach(var child in ChildViews)
            {
                int vl = (int)fluentEngine.GetValue(child, LayoutAttribute.Left);
                int vr = (int)fluentEngine.GetValue(child, LayoutAttribute.Right);
                int vt = (int)fluentEngine.GetValue(child, LayoutAttribute.Top);
                int vb = (int)fluentEngine.GetValue(child, LayoutAttribute.Bottom);
                
                child.Layout(vl, vt, vr, vb);
            }
		}

        public override void AddView(View.View v)
        {
            base.AddView(v);

			fluentEngine.AddView (v);
        }

		public void AddConstraints(params IFluentLayout<View.View>[] fluentLayouts)
		{
            this.fluentEngine.AddConstraints(fluentLayouts);
		}

        public void RemoveAllConstraints()
        {
			foreach(var cv in ChildViews.ToList())
            {
				this.RemoveView (cv);
                cv.Dispose();
            }
            
            this.fluentEngine.RemoveAllConstraints();
        }

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);

			if (disposing) {
				fluentEngine.Dispose ();
			}
		}
	}
}

