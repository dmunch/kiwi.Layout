using System;
using System.Linq;
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

    /*
    public class SolverWithView
    {
        public ClSimplexSolver Solver { get; protected set; }

        public bool AlreadySolved { get; set; }

        public SolverWithView()
        {
            Solver = new ClSimplexSolver();
            Solver.AutoSolve = true;
            AlreadySolved = false;
        }
    }*/

	public class FluentEngine<T>
	{
        T _rootView;
        protected ClSimplexSolver solver;
		
        protected Dictionary<ViewAndLayoutAttribute<T>,ClVariable> variables;
        Dictionary<string, byte> isStay = new Dictionary<string, byte>();


        protected List<IFluentLayout<T>> _constraints;
        protected Dictionary<LayoutAttribute, ClVariable> _stayVariables = new Dictionary<LayoutAttribute,ClVariable>();

		protected IViewEngine<T> viewEngine;
		public FluentEngine (T rootView, IViewEngine<T> viewEngine)
		{
			this.viewEngine = viewEngine;

            _constraints = new List<IFluentLayout<T>>();
			variables = new Dictionary<ViewAndLayoutAttribute<T>, ClVariable>(
					new ViewAndLayoutEqualityComparer<T>(viewEngine)
				);

            _rootView = rootView;
            Init();
		}

        protected void Init()
        {
            solver = new ClSimplexSolver();
            solver.AutoSolve = true;

            var stayVariables = new LayoutAttribute[] {
				    LayoutAttribute.Left,				    
				    LayoutAttribute.Top
			    };

            foreach (var attribute in stayVariables)
            {
                var variable = GetVariableFromViewAndAttribute(_rootView, attribute);
                variable.ChangeValue(0);

                _stayVariables.Add(attribute, variable);
                solver.AddStay(variable);
            }
        }

        public void RemoveAllConstraints()
        {
            _constraints.Clear();
            isStay.Clear();
            variables.Clear();
            _stayVariables.Clear();
            Init();
        }

		public string AddConstraints (IEnumerable<IFluentLayout<T>> fluentLayouts)
		{
			foreach (var fluentLayout in fluentLayouts) {
				foreach(var constraint in GetConstraintsFromFluentLayout (fluentLayout))
                {
                    solver.AddConstraint(constraint);
                }
			}

            _constraints.AddRange(fluentLayouts);
			return solver.ToString ();
		}

        public void SetEditConstraints(IFluentLayout<T> constraint)
        {            
            if (constraint.SecondItem != null)
            {
                throw new Exception("Edit constraint can't have a right side");
            }

            var variable = GetVariableFromViewAndAttribute(constraint.View, constraint.Attribute);
            solver.AddEditVar(variable);
        }

        
        protected void SetEditConstraints(IEnumerable<IFluentLayout<T>> constraints)
        {
            foreach(var constraint in constraints)
            { 
                if (constraint.SecondItem != null)
                {
                    throw new Exception("Edit constraint can't have a right side");
                }

                var variable = GetVariableFromViewAndAttribute(constraint.View, constraint.Attribute);
                AddEditVar(solver, variable);
            }
        }

        public void SetEditedValues(IEnumerable<IFluentLayout<T>> constraints, IEnumerable<double> values)
        {
            if (!constraints.Any()) return;

            SetEditConstraints(constraints);
            //solver.AutoSolve = false;
            solver.BeginEdit();
            foreach(var cv in constraints.Zip(values, (c, v) => new {c, v}))
            {                
                var variable = GetVariableFromViewAndAttribute(cv.c.View, cv.c.Attribute);
                //variable.ChangeValue(cv.v);
                solver.SuggestValue(variable, cv.v);
            }
            //solver.Resolve();
            //solver.AutoSolve = true;
            //solver.EndEdit();
            /*
            foreach (var cv in constraints.Zip(values, (c, v) => new { c, v }))
            {
                var variable = GetVariableFromViewAndAttribute(cv.c.View, cv.c.Attribute);
                solver.AddStay(variable);
                solver.SetEditedValue(variable, cv.v);
            } */           
        }

        public double MeasureHeight(T v)
        {
            var top = GetValue(v, LayoutAttribute.Top);
            var bottm = GetValue(v, LayoutAttribute.Bottom);

            return bottm - top;
        }

        public double MeasuredWidth(T v)
        {
            var left = GetValue(v, LayoutAttribute.Left);
            var right = GetValue(v, LayoutAttribute.Right);

            return right - left;
        }

        public double MeasureHeight(T view, int width)        
        {
            //var variable = GetCompositeVariableFromViewAndAttribute(view, view, LayoutAttribute.Width);
            var variable = GetVariableFromViewAndAttribute(view, LayoutAttribute.Right);
            AddEditVar(solver, variable, width);

            var height = MeasureHeight(view);            
            //solver.Solver.EndEdit();

            return height;
        }

        void AddEditVar(ClSimplexSolver solver, ClVariable variable, int value)
        {
            if (!isStay.ContainsKey(variable.Name))
            {
                isStay.Add(variable.Name, 0);
                variable.ChangeValue(value);

                solver.AddStay(variable);
            } else
            {
                var status = isStay[variable.Name];

                if(status == 0)
                { 
                    solver.AddEditVar(variable);
                    isStay[variable.Name] = 1;
                }

                solver.BeginEdit();
                solver.SuggestValue(variable, value);
            }
        }

        void AddEditVar(ClSimplexSolver solver, ClVariable variable)
        {
            if (!isStay.ContainsKey(variable.Name))
            {
                isStay.Add(variable.Name, 1);

                solver.AddStay(variable);
                solver.AddEditVar(variable);
            }
        }

        public void SetValues()
        {
            //update attributes from solved values
            foreach (var kvp in variables)
            {
                viewEngine.SetAttribute(kvp.Key.View, kvp.Key.Attribute, (int)kvp.Value.Value);
            }
        }

        public double GetValue(T view, LayoutAttribute attribute)
        {
            var variable = GetVariableFromViewAndAttribute(view, attribute);
            return variable.Value;
        }

		protected IEnumerable<ClConstraint> GetConstraintsFromFluentLayout(IFluentLayout<T> fluentLayout)
		{
            var constraints = new List<ClConstraint>();
			ClLinearExpression firstExpression = null;
			firstExpression = GetExpressionFromViewAndAttribute (fluentLayout.View, fluentLayout.Attribute);

			ClLinearExpression secondExpression = null;
			if (fluentLayout.SecondItem != null) {
				secondExpression = GetExpressionFromViewAndAttribute (
					fluentLayout.SecondItem.View,
					fluentLayout.SecondItem.Attribute
				);
                
                var multiplier = fluentLayout.Multiplier != 0 ? fluentLayout.Multiplier : 1;
				secondExpression = Cl.Plus (
					Cl.Times (secondExpression, multiplier),
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

            constraints.Add(cn);
			
			return constraints;
		}

        protected ClVariable GetCompositeVariableFromViewAndAttribute(T view, LayoutAttribute attribute)
        {            
            if (HasVariableForViewAndAttribute(view, attribute))
            {
                return GetVariableFromViewAndAttribute(view, attribute);
            }

            switch (attribute)
            {
                case LayoutAttribute.Width:
                    {
                        var leftVar = GetVariableFromViewAndAttribute(view, LayoutAttribute.Left);
                        var rightVar = GetVariableFromViewAndAttribute(view, LayoutAttribute.Right);

                        var widthExpression = Cl.Minus(
                            new ClLinearExpression(rightVar),
                            new ClLinearExpression(leftVar)
                        );

                        var widthVariable = new ClVariable(viewEngine.GetViewName(view) + ".Width");
                                                
                        solver.AddConstraint(new ClLinearEquation(
                               widthVariable,
                               widthExpression,
                               ClStrength.Required
                           ));

                        AddVariableForViewAndAttribute(view, attribute, widthVariable);
                        return widthVariable;
                    }
                case LayoutAttribute.Height:
                    {
                        var topVar = GetVariableFromViewAndAttribute(view, LayoutAttribute.Top);
                        var bottomVar = GetVariableFromViewAndAttribute(view, LayoutAttribute.Bottom);

                        var heightExpression = Cl.Minus(
                            new ClLinearExpression(bottomVar),
                            new ClLinearExpression(topVar)
                        );

                        var heightVariable = new ClVariable(viewEngine.GetViewName(view) + ".Height");
                        
                        solver.AddConstraint(new ClLinearEquation(
                               heightVariable,
                               heightExpression,
                               ClStrength.Required                            
                           ));

                        solver.AddConstraint(new ClLinearInequality(
                               heightExpression,
                               Cl.Operator.GreaterThanOrEqualTo,
                               new ClLinearExpression(0)
                           ));

                        AddVariableForViewAndAttribute(view, attribute, heightVariable);
                        return heightVariable;
                    }
                case LayoutAttribute.CenterX:
                    {
                        var leftVar = GetVariableFromViewAndAttribute(view, LayoutAttribute.Left);
                        var rightVar = GetVariableFromViewAndAttribute(view, LayoutAttribute.Right);

                        var centerXExpression = Cl.Plus(
                            Cl.Divide(new ClLinearExpression(leftVar), new ClLinearExpression(2)),
                            Cl.Divide(new ClLinearExpression(rightVar), new ClLinearExpression(2))
                        );

                        var centerXVariable = new ClVariable(viewEngine.GetViewName(view) + ".CenterX");
                        solver.AddConstraint(new ClLinearEquation(
                               centerXVariable,
                               centerXExpression,
                               ClStrength.Required                            
                           ));

                        AddVariableForViewAndAttribute(view, attribute, centerXVariable);
                        return centerXVariable;
                    }
                case LayoutAttribute.CenterY:
                    {
                        var topVar = GetVariableFromViewAndAttribute(view, LayoutAttribute.Top);
                        var bottomVar = GetVariableFromViewAndAttribute(view, LayoutAttribute.Bottom);

                        var centerYExpression = Cl.Plus(
                            Cl.Divide(new ClLinearExpression(topVar), new ClLinearExpression(2)),
                            Cl.Divide(new ClLinearExpression(bottomVar), new ClLinearExpression(2))
                        );

                        var centerYVariable = new ClVariable(viewEngine.GetViewName(view) + ".CenterY");
                        solver.AddConstraint(new ClLinearEquation(
                               centerYVariable,
                               centerYExpression,
                               ClStrength.Required                            
                           ));

                        AddVariableForViewAndAttribute(view, attribute, centerYVariable);
                        return centerYVariable;
                    }
            }

            return null;
        }

		protected ClLinearExpression GetExpressionFromViewAndAttribute(T view, LayoutAttribute attribute/*, out ClLinearEquation egality*/)
		{            
            if (HasVariableForViewAndAttribute(view, attribute))
            {
                return new ClLinearExpression(GetVariableFromViewAndAttribute(view, attribute));
            }

            var compositeVariable = GetCompositeVariableFromViewAndAttribute(view, attribute);
            if(compositeVariable != null)
            {
                return new ClLinearExpression(compositeVariable);
            }

            var variable = GetVariableFromViewAndAttribute(view, attribute);
            var expression = new ClLinearExpression(variable);

            var greaterZero = new LayoutAttribute[] {
				    LayoutAttribute.Width,				    
				    LayoutAttribute.Height,
                    LayoutAttribute.Top,
                    LayoutAttribute.Bottom,
                    LayoutAttribute.Left,
                    LayoutAttribute.Right
			    };

            if(greaterZero.Contains(attribute))
            {
                solver.AddConstraint(new ClLinearInequality(
                              expression,
                              Cl.Operator.GreaterThanOrEqualTo,
                              new ClLinearExpression(0)
                          ));
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

        protected bool HasVariableForViewAndAttribute(T view, LayoutAttribute attribute)
        {
            var viewAndAttribute = new ViewAndLayoutAttribute<T>(view, attribute);
            return variables.ContainsKey(viewAndAttribute);            
        }

        protected void AddVariableForViewAndAttribute(T view, LayoutAttribute attribute, ClVariable variable)
        {
            if (HasVariableForViewAndAttribute(view, attribute)) return;

            var viewAndAttribute = new ViewAndLayoutAttribute<T> (view, attribute);
            variables.Add(viewAndAttribute, variable);
        }
	}
}

