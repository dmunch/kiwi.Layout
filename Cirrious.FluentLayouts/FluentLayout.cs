using System;

namespace Cirrious.FluentLayouts
{
	public class FluentLayout<T>  : IFluentLayout<T>
	{
		public T View { get; protected set; }

		public LayoutAttribute Attribute { get; protected set; }
		public LayoutRelation Relation { get; protected set; }

		public IViewAndLayoutAttribute<T> SecondItem { get; set;}
		/*
		public T SecondItem { get; protected set;}
		public LayoutAttribute SecondAttribute { get; protected set; }
*/

		public float Multiplier { get; protected set; }
		public float Constant { get; protected set; }
		public float Priority { get; protected set; }

		public FluentLayout(T view, LayoutAttribute attribute, LayoutRelation relation, float constant)
		{
			this.View = view;
			this.Attribute = attribute;
			this.Relation = relation;
			this.Constant = constant;
			this.Multiplier = 1;
			this.Priority = 1000;
		}

		public FluentLayout(IFluentLayout<T> other)
		{
			this.View = other.View;
			this.Attribute = other.Attribute;
			this.Relation = other.Relation;

			this.SecondItem = other.SecondItem;

			this.Multiplier = other.Multiplier;
			this.Constant = other.Constant;
			this.Priority = other.Priority;
		}

		public IFluentLayout<T> Plus(float constant)
		{
			Constant += constant;
			return this;
		}

		public IFluentLayout<T> Minus(float constant)
		{
			Constant -= constant;
			return this;
		}

		public IFluentLayout<T> WithMultiplier(float multiplier)
		{
			Multiplier = multiplier;
			return this;
		}

		public IFluentLayout<T> SetPriority(float priority)
		{
			Priority = priority;
			return this;
		}

		/*
		public FluentLayout SetPriority(UILayoutPriority priority)
		{
			Priority = (float) priority;
			return this;
		}*/

		/*
		public abstract IFluentLayout<T> LeftOf (T view2);
		public abstract IFluentLayout<T> RightOf(T view2);
		public abstract IFluentLayout<T> TopOf(T view2);
		public abstract IFluentLayout<T> BottomOf(T view2);
		public abstract IFluentLayout<T> BaselineOf(T view2);
		public abstract IFluentLayout<T> TrailingOf(T view2);
		public abstract IFluentLayout<T> LeadingOf(T view2);
		public abstract IFluentLayout<T> CenterXOf(T view2);
		public abstract IFluentLayout<T> CenterYOf(T view2);
		public abstract IFluentLayout<T> HeightOf(T view2);
		public abstract IFluentLayout<T> WidthOf(T view2);
		*/
		/*
		public IFluentLayout<T> LeftOf(T view2)
		{
			return SetSecondItem(view2, LayoutAttribute.Left);
		}

		public IFluentLayout<T> RightOf(T view2)
		{
			return SetSecondItem(view2, LayoutAttribute.Right);
		}

		public IFluentLayout<T> TopOf(T view2)
		{
			return SetSecondItem(view2, LayoutAttribute.Top);
		}

		public IFluentLayout<T> BottomOf(T view2)
		{
			return SetSecondItem(view2, LayoutAttribute.Bottom);
		}

		public IFluentLayout<T> BaselineOf(T view2)
		{
			return SetSecondItem(view2, LayoutAttribute.Baseline);
		}

		public IFluentLayout<T> TrailingOf(T view2)
		{
			return SetSecondItem(view2, LayoutAttribute.Trailing);
		}

		public IFluentLayout<T> LeadingOf(T view2)
		{
			return SetSecondItem(view2, LayoutAttribute.Leading);
		}

		public IFluentLayout<T> CenterXOf(T view2)
		{
			return SetSecondItem(view2, LayoutAttribute.CenterX);
		}

		public IFluentLayout<T> CenterYOf(T view2)
		{
			return SetSecondItem(view2, LayoutAttribute.CenterY);
		}

		public IFluentLayout<T> HeightOf(T view2)
		{
			return SetSecondItem(view2, LayoutAttribute.Height);
		}

		public IFluentLayout<T> WidthOf(T view2)
		{
			return SetSecondItem(view2, LayoutAttribute.Width);
		}

		private FluentLayout<T> SetSecondItem(T view2, LayoutAttribute attribute2)
		{
			ThrowIfSecondItemAlreadySet();
			SecondAttribute = attribute2;
			SecondItem = view2;
			return this;
		}

		private void ThrowIfSecondItemAlreadySet()
		{
			if (SecondItem != null)
				throw new Exception("You cannot set the second item in a layout relation more than once");
		}
		*/
	}
}

