using System;
using System.Collections.Generic;

namespace FluentLayout.Cassowary
{
	public class View
	{
		public int Left { get; set; }
		public int Right { get; set; }
		public int Width { get { return Right - Left; } }

		public int Top { get; set; }
		public int Bottom { get; set; }
		public int Height { get{ return Bottom - Top;} }

		protected List<View> children; 
		public IEnumerable<View> Children { get { return children;}}

		public char Color { get; set; }

		public void AddChild(View view)
		{
			this.children.Add (view);
		}

		public void RemoveChild(View view)
		{
			this.children.Remove (view);
		}


		protected void Draw(char[,] canvas)
		{
			for (int x = Left; x < Right; x++)
				for (int y = Top; y < Bottom; y++) {

					canvas [x, y] = Color;
				}

			foreach (var child in children) {
				child.Draw (canvas);
			}
		}

		public char[,] Draw()
		{
			var canvas = new char[Width + Left, Height + Top];
			this.Draw (canvas);

			return canvas;
		}

		protected View ()
		{
			this.Color = 'x';
			this.children = new List<View> ();
		}

		public View(int left, int right, int top, int bottom, char color='x')
			:this()
		{
			this.Left = left;
			this.Right = right;
			this.Top = top;
			this.Bottom = bottom;

			this.Color = color;
		}
	}
}

