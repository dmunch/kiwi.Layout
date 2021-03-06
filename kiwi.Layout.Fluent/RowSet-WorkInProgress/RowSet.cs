﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace kiwi.Layout.Fluent
{
    public class RowSetTemplate
    {
        public float TopMargin { get; set; }
        public float BottomMargin { get; set; }
        public float VInterspacing { get; set; }

		public IEnumerable<IFluentLayout<T>> Generate<T>(T container, params IRow<T>[] rows)
        {
            for (var i = 0; i < rows.Length; i++)
            {
				var verticalGenerators = new List<Func<T, IFluentLayout<T>>>();

                var isFirst = i == 0;
                if (isFirst)
                    verticalGenerators.Add(view => view.AtTopOf(container, TopMargin));
                else
                {
                    T previousRowView = rows[i - 1].Views.First();
                    verticalGenerators.Add(view => view.Below(previousRowView, VInterspacing));
                }

                var isLast = i == rows.Length - 1;
                if (isLast)
                    verticalGenerators.Add(view => view.AtBottomOf(container, BottomMargin));

                foreach (var view in rows[i].Views)
                {
                    foreach (var verticalGenerator in verticalGenerators)
                        yield return verticalGenerator(view);
                }

                foreach (var horizontalConstraint in rows[i].Template.Generate(container, rows[i].Views.ToArray()))
                    yield return horizontalConstraint;
            }
        }
    }

	public class Row
	{
		public static Row<T> Create<T>(IRowTemplate rowTemplate, params T[] views)
		{
			return new Row<T> (rowTemplate, views);
		}
	}
	public interface IRow<out T>
	{
		IRowTemplate Template { get; }
		IEnumerable<T> Views { get; }
	}

	public class Row<T> : IRow<T>
    {
        public Row()
        {
        }

		public Row(IRowTemplate rowTemplate, params T[] views)
        {
            Template = rowTemplate;
            Views = views;
        }
			
		public IRowTemplate Template { get; set; }
        public IEnumerable<T> Views { get; set; }
    }

    public interface IRowTemplate
    {
		IEnumerable<IFluentLayout<T>> Generate<T>(T container, params T[] views);
    }

    public class RowTemplate : IRowTemplate
    {
        public float LeftMargin { get; set; }
        public float RightMargin { get; set; }
        public float HInterspacing { get; set; }

        public class Column
        {
            public static readonly Column Default = new WeightedWidthColumn();
        }

        public class FixedWidthColumn
            : Column
        {
            public float Width { get; set; }

            public FixedWidthColumn(float width)
            {
                Width = width;
            }
        }

        public class WeightedWidthColumn
            : Column
        {
            public float Weight { get; set; }

            public WeightedWidthColumn(float weight = 1.0f)
            {
                Weight = weight;
            }
        }

        private readonly Dictionary<int, Column> _columnDefinitions = new Dictionary<int, Column>();

        public void ColumnWidth(int position, float width)
        {
            _columnDefinitions[position] = new FixedWidthColumn(width);
        }

        public void ColumnWeight(int position, float weight)
        {
            _columnDefinitions[position] = new WeightedWidthColumn(weight);
        }

		public IEnumerable<IFluentLayout<T>> Generate<T>(T container, params T[] views)
        {
            WeightedWidthColumn firstWeightedColumn = null;
			T firstWeightedView = default(T);

            for (var i = 0; i < views.Length; i++)
            {
                var view = views[i];
                var column = GetColumn(i);

                if (i == 0)
                {
                    yield return view.AtLeftOf(container, LeftMargin);
                }
                else
                {
                    yield return view.ToRightOf(views[i - 1], HInterspacing);
                }

                if (i == views.Length - 1)
                {
                    yield return view.AtRightOf(container, RightMargin);
                }

                var weightedColumn = column as WeightedWidthColumn;
                if (weightedColumn != null)
                {
                    if (firstWeightedColumn == null)
                    {
                        firstWeightedColumn = weightedColumn;
                        firstWeightedView = view;
                    }
                    else
                    {
                        var multiplier = weightedColumn.Weight / firstWeightedColumn.Weight;
                        yield return view.WithRelativeWidth(firstWeightedView, multiplier);
                    }
                }

                var fixedColumn = column as FixedWidthColumn;
                if (fixedColumn != null)
                {
                    yield return view.Width().EqualTo(fixedColumn.Width);
                }
            }
        }

        private Column GetColumn(int index)
        {
            Column column;
            if (_columnDefinitions.TryGetValue(index, out column))
                return column;

            return Column.Default;
        }
    }
}
