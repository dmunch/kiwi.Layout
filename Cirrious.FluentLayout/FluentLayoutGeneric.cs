using System;

namespace Cirrious.FluentLayouts.Touch
{
	public interface IFluentLayout<T>
	{
		float Multiplier { get; }
		float Constant { get; }
		float Priority { get; }

		IFluentLayout<T> Plus (float constant);
		IFluentLayout<T> Minus (float constant);
		IFluentLayout<T> WithMultiplier (float multiplier);
		IFluentLayout<T> SetPriority (float priority);
	
		IFluentLayout<T> LeftOf (T view2);
		IFluentLayout<T> RightOf(T view2);
		IFluentLayout<T> TopOf(T view2);
		IFluentLayout<T> BottomOf(T view2);
		IFluentLayout<T> BaselineOf(T view2);
		IFluentLayout<T> TrailingOf(T view2);
		IFluentLayout<T> LeadingOf(T view2);
		IFluentLayout<T> CenterXOf(T view2);
		IFluentLayout<T> CenterYOf(T view2);
		IFluentLayout<T> HeightOf(T view2);
		IFluentLayout<T> WidthOf(T view2);
	}

	public abstract class FluentLayoutGeneric<T>  : IFluentLayout<T>
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

