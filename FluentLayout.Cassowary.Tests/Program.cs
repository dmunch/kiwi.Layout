using System;
using Cirrious.FluentLayouts;

namespace FluentLayout.Cassowary.Tests
{
	public class ViewPrinter
	{
		public static void Print(View v)
		{
			var canvas = v.Draw ();

			for (int y = 0; y < canvas.GetLength (1); y++)
			{
				for (int x = 0; x < canvas.GetLength (0); x++) {
					Console.Write (canvas [x, y]);
				}
				Console.WriteLine ();
			}
		}
	}

	class MainClass
	{
		public static void Main (string[] args)
		{
			var v = new View (0, 60, 0, 20, ' ');
			var vo = new View (0, 10, 0, 5, 'o');
			var vi = new View (10, 20, 0, 10, 'i');
			var vk = new View (0, 0, 0, 0, 'k');

			v.AddChild(vo);
			v.AddChild(vi);
			v.AddChild (vk);

			var tableau = v.AddConstraints(
				vo.WithSameRight(v),
				vo.WithSameTop(v),
				vo.Width().EqualTo(5),
				vo.Height().EqualTo(5),
				vi.Below (vo),
				vi.Width().EqualTo(6),
				vi.Height().EqualTo(7),
				vi.WithSameRight(v),
				vk.Right().LessThanOrEqualTo().LeftOf(vo),
				vk.Top().GreaterThanOrEqualTo(4),
				vk.WithSameWidth(vo),
				vk.WithSameHeight(vo)
			);

			Console.WriteLine (tableau);

			Console.WriteLine ("Start.");
			ViewPrinter.Print (v);

			Console.WriteLine ("End.");
		}
	}
}
