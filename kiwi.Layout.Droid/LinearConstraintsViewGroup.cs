using Android.Content;
using Android.Views;
using Android.Widget;
using System.Collections.Generic;
using System.Linq;

namespace kiwi.Layout.Droid
{
	using Fluent;

	public class LinearConstraintsViewGroup : ViewGroup
	{
		protected LayoutEngine<View> fluentEngine;
        
        protected IEnumerable<View> ChildViews { 
			get
			{
				for (int c = 0; c < this.ChildCount; c++) {
					yield return this.GetChildAt (c);
				}
			}
		}

        /// <summary>
        /// ChildViews of type TextView and all subtypes, without Buttons
        /// </summary>
        /// <value>The text views.</value>
        protected IEnumerable<TextView> TextViews
        {
            get
            { 
                return ChildViews.OfType<TextView>().Where(v => !(v is Button));
            }
        }

        /// <summary>
        /// ChildViews of real type TextView.
        /// </summary>
        /// <value>The text views.</value>
        protected IEnumerable<TextView> RealTextViews
        {
            get
            { 
                return ChildViews.OfType<TextView>().Where(v => v.GetType() == typeof(TextView));
            }
        }

		public LinearConstraintsViewGroup(Context context)
			:base(context)
		{
			fluentEngine = new LayoutEngine<View>(this, new AndroidViewEngine());
		}

		public override void SetPadding(int left, int top, int right, int bottom)
		{
			base.SetPadding(left, top, right, bottom);
			fluentEngine.SetPadding(PaddingLeft, PaddingTop, PaddingRight, PaddingBottom);
		}

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
			var parentWidth = MeasureSpec.GetSize(widthMeasureSpec);

            //var sw = new System.Diagnostics.Stopwatch();
            //sw.Start();
            var goneViews = ChildViews.Where(v => v.Visibility == ViewStates.Gone);
            var textFieldsWithNoText = RealTextViews.Where(tv => string.IsNullOrEmpty(tv.Text));

            fluentEngine.SetGoneViews(goneViews.Union(textFieldsWithNoText));
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

            var childViewsWithHorizontalContentHugging = TextViews
                                                            .Where(tv => tv.Visibility != ViewStates.Gone)
                                                            .Where(cv => cv.Tag != null)
                                                            .ToArray();

            var editConstraints = TextViews.Except(childViewsWithHorizontalContentHugging)
                                            .Where(tv => tv.Visibility != ViewStates.Gone)
                                            .Select (tv => tv.Height ().GreaterThanOrEqualTo (MeasureHeight (tv)))
                                            .ToList();
            
            foreach (var childView in childViewsWithHorizontalContentHugging)
            {
                if (childView.Visibility == ViewStates.Gone)
                    continue;

                var widthSpec = MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified);
                var heightSpec = MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified);
                childView.Measure(widthSpec, heightSpec);

                editConstraints.Add(childView.Width().EqualTo(childView.MeasuredWidth).SetPriority((int)childView.Tag));
                editConstraints.Add(childView.Height().EqualTo(childView.MeasuredHeight));
            }


			fluentEngine.SetEditedValues(editConstraints);
		}

		protected float MeasureHeight(View tv)
		{
			if (tv.Visibility == ViewStates.Gone)
				return 0;
			
            var width = (int)fluentEngine.MeasuredWidth(tv) - PaddingLeft - PaddingRight;

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

        public override void AddView(View v)
        {
            base.AddView(v);

			fluentEngine.AddView (v);
        }

		public void AddConstraints(params IFluentLayout<View>[] fluentLayouts)
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

