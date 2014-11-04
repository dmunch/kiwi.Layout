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
			var v = new View (0, 60, 0, 20, 'x');
			var v1 = new View (0, 10, 0, 5, 'o');
			var v2 = new View (10, 20, 0, 10, 'i');

			v.AddChild(v1);
			v.AddChild(v2);

			var tableau = v.AddConstraints(
				v1.WithSameRight(v),
				v1.WithSameTop(v),
				v1.Width().EqualTo(5),
				v1.Height().EqualTo(5),
				v2.Below (v1),
				v2.Width().EqualTo(6),
				v2.Height().EqualTo(7),
				v2.WithSameRight(v)
			);

			Console.WriteLine (tableau);

			Console.WriteLine ("Start.");
			ViewPrinter.Print (v);

			Console.WriteLine ("End.");
		}
	}
}
