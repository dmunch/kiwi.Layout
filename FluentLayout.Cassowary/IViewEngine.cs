using Cirrious.FluentLayouts;

namespace FluentLayout.Cassowary
{
	public interface IViewEngine<T>
	{
		string GetViewName (T view);
		float GetAttribute (T view, LayoutAttribute attribute);
		void SetAttribute (T view, LayoutAttribute attribute, float value);
	}
}