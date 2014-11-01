namespace Cirrious.FluentLayouts
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
}

