using System;

namespace Cirrious.FluentLayouts
{
	public abstract class FluentLayout<T>  : IFluentLayout<T>
	{
		public float Multiplier { get; protected set; }
		public float Constant { get; protected set; }
		public float Priority { get; protected set; }

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
	}
}

