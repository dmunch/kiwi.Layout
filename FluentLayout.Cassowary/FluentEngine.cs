using System;
using System.Collections.Generic;

using Cassowary;
using Cirrious.FluentLayouts;

namespace FluentLayout.Cassowary
{
	class ViewAndLayoutEqualityComparer<T> : IEqualityComparer<ViewAndLayoutAttribute<T>>
	{
		protected IViewEngine<T> viewEngine;
		public ViewAndLayoutEqualityComparer(IViewEngine<T> viewEngine)
		{
			this.viewEngine = viewEngine;
		}

		#region IEqualityComparer implementation

		public bool Equals (ViewAndLayoutAttribute<T> x, ViewAndLayoutAttribute<T> y)
		{
			return viewEngine.GetViewName(x.View) == viewEngine.GetViewName(y.View) && x.Attribute == y.Attribute;
		}

		public int GetHashCode (ViewAndLayoutAttribute<T> obj)
		{
			var hc = string.Format("{0}-{1}", viewEngine.GetViewName(obj.View), obj.Attribute);
			return hc.GetHashCode ();
		}
		#endregion
	}

	public class FluentEngine<T>
	{
		protected Dictionary<T,ClSimplexSolver> solvers;
		protected Dictionary<ViewAndLayoutAttribute<T>,ClVariable> variables;

		protected IViewEngine<T> viewEngine;
		public FluentEngine (IViewEngine<T> viewEngine)
		{
			this.viewEngine = viewEngine;

			solvers = new Dictionary<T, ClSimplexSolver>();
			variables = 
				new Dictionary<ViewAndLayoutAttribute<T>, ClVariable>(
					new ViewAndLayoutEqualityComparer<T>(viewEngine)
				);
		}

		public string AddConstraints (T view, IEnumerable<IFluentLayout<T>> fluentLayouts)
		{
			ClSimplexSolver solver = GetSolver (view);

			//parent view always stays
			var stayAttributes = new LayoutAttribute[] {
				LayoutAttribute.Left,
				LayoutAttribute.Right,
				LayoutAttribute.Top,
				LayoutAttribute.Bottom
			};

			foreach (var attribute in stayAttributes) {
				var variable = GetVariableFromViewAndAttribute (view, attribute);

				solver.AddStay (variable, ClStrength.Strong);
			}

			foreach (var fluentLayout in fluentLayouts) {
				var cn = GetConstraintFromFluentLayout (fluentLayout);
				solver.AddConstraint (cn);
			}

			return solver.ToString ();
		}

		public string Solve(T view)
		{
			ClSimplexSolver solver = GetSolver (view);
			solver.Solve ();

			//update attributes from solved values
			foreach (var kvp in variables) {
				viewEngine.SetAttribute (kvp.Key.View, kvp.Key.Attribute, (int)kvp.Value.Value);
			}

			return solver.ToString ();
		}

		protected ClSimplexSolver GetSolver(T view)
		{
			ClSimplexSolver solver = null;

			if (!solvers.TryGetValue (view, out solver)) {
				solver = new ClSimplexSolver();
				solvers.Add (view, solver);
			}

			return solver;
		}

		protected ClConstraint GetConstraintFromFluentLayout(IFluentLayout<T> fluentLayout)
		{
			ClLinearExpression firstExpression = null;
			firstExpression = GetExpressionFromViewAndAttribute (fluentLayout.View, fluentLayout.Attribute);

			ClLinearExpression secondExpression = null;
			if (fluentLayout.SecondItem != null) {
				secondExpression = GetExpressionFromViewAndAttribute (
					fluentLayout.SecondItem.View,
					fluentLayout.SecondItem.Attribute
				);
				secondExpression = Cl.Plus (
					Cl.Times (secondExpression, fluentLayout.Multiplier),
					new ClLinearExpression (fluentLayout.Constant)
				);
			} else {
				secondExpression = new ClLinearExpression (fluentLayout.Constant);
			}

			ClConstraint cn = null;
			switch (fluentLayout.Relation) {
			case LayoutRelation.Equal:
				cn = new ClLinearEquation (
					firstExpression,
					secondExpression
				);
				break;
			case LayoutRelation.GreaterThanOrEqual:
				cn = new ClLinearInequality (
					firstExpression,
					Cl.Operator.GreaterThanOrEqualTo,
					secondExpression
				);
				break;
			case LayoutRelation.LessThanOrEqual:
				cn = new ClLinearInequality (
					firstExpression,
					Cl.Operator.LessThanOrEqualTo,
					secondExpression
				);
				break;
			}

			cn.Strength = ClStrength.Strong;
			return cn;
		}

		protected ClLinearExpression GetExpressionFromViewAndAttribute(T view, LayoutAttribute attribute)
		{
			ClLinearExpression expression = null;
			switch(attribute)
			{
			case LayoutAttribute.Width:
				var leftVar = GetVariableFromViewAndAttribute (view, LayoutAttribute.Left);
				var rightVar = GetVariableFromViewAndAttribute (view, LayoutAttribute.Right);
				expression = Cl.Minus (
					new ClLinearExpression(rightVar), 
					new ClLinearExpression(leftVar)
				);
				break;
			case LayoutAttribute.Height:
				var topVar = GetVariableFromViewAndAttribute (view, LayoutAttribute.Top);
				var bottomVar = GetVariableFromViewAndAttribute (view, LayoutAttribute.Bottom);
				expression = Cl.Minus (
					new ClLinearExpression(bottomVar), 
					new ClLinearExpression(topVar)
				);
				break;
			default:
				var variable = GetVariableFromViewAndAttribute (view, attribute);
				expression = new ClLinearExpression (variable);
				break;
			}

			return expression;
		}

		protected ClVariable GetVariableFromViewAndAttribute(T view, LayoutAttribute attribute)
		{
			ClVariable variable = null;
			var viewAndAttribute = new ViewAndLayoutAttribute<T> (view, attribute);

			if (!variables.TryGetValue (viewAndAttribute, out variable)) {
				var value = viewEngine.GetAttribute (view, attribute);
				var name = string.Format ("{0}.{1}", viewEngine.GetViewName (view), attribute.ToString ());
				variable = new ClVariable (name, value);
				variables.Add (viewAndAttribute, variable);
			} 

			return variable; 
		}
	}
}

