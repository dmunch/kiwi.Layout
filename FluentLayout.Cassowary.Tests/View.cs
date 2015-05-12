using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentLayout.Cassowary.Tests
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
            var widthMax = this.children.Max(c => c.Width + c.Left);
            var heightMax = this.children.Max(c => c.Height + c.Top);

			var canvas = new char[widthMax, heightMax];
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

        public override string ToString()
        {
            return string.Format("{0} l: {1}, t: {2}, w: {3}, h: {4}", Color, Left, Top, Width, Height);
        }
	}
}

