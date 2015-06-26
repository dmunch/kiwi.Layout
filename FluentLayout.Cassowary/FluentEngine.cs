using System;
using System.Linq;
using System.Collections.Generic;

using Cassowary;
using Cirrious.FluentLayouts;

namespace FluentLayout.Cassowary
{
	public class ViewAndLayoutEqualityComparer<T> : IEqualityComparer<ViewAndLayoutAttribute<T>>
	{
		#region IEqualityComparer implementation

		public bool Equals (ViewAndLayoutAttribute<T> x, ViewAndLayoutAttribute<T> y)
		{
			return x.View.GetHashCode() == y.View.GetHashCode() && x.Attribute == y.Attribute;
		}

		public int GetHashCode (ViewAndLayoutAttribute<T> obj)
		{
			unchecked {
				int hash = 17;

				hash = hash * 31 + obj.View.GetHashCode ();
				hash = hash * 31 + obj.Attribute.GetHashCode ();

				return hash;
			}
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
					new ViewAndLayoutEqualityComparer<T>()
				);

            _rootView = rootView;
            Init();
		}

        protected void Init()
        {
            solver = new ClSimplexSolver();
            solver.AutoSolve = false;

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

		public void AddConstraints (IEnumerable<IFluentLayout<T>> fluentLayouts)
		{
            foreach (var fluentLayout in fluentLayouts) {
				foreach(var constraint in GetConstraintsFromFluentLayout (fluentLayout))
                {
                    solver.AddConstraint(constraint);
                }
			}
            solver.Solve();            
            _constraints.AddRange(fluentLayouts);
		}

        public void AddConstraint(IFluentLayout<T> constraint)
        {
            foreach (var c in GetConstraintsFromFluentLayout(constraint))
            {
                solver.AddConstraint(c);
            }
        }

#if false
        public void AddStayConstraints(IEnumerable<IFluentLayout<T>> constraints)
        {
            //add constraints to solver
            foreach (var fluentLayout in constraints)
            {
                foreach (var constraint in GetConstraintsFromFluentLayout(fluentLayout))
                {
                    solver.AddConstraint(constraint);
                }
            }
            _constraints.AddRange(constraints);

            foreach (var constraint in constraints)
            {
                if (constraint.SecondItem != null)
                {
                    throw new Exception("Edit constraint can't have a right side");
                }

                var variable = GetVariableFromViewAndAttribute(constraint.View, constraint.Attribute);
                if (!isStay.ContainsKey(variable.Name))
                {
                    isStay.Add(variable.Name, 0);
                    variable.ChangeValue(0);

                    solver.AddStay(variable);                    
                } 
            }
        }
#endif

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
            
            BeginEdit();
            foreach(var cv in constraints.Zip(values, (c, v) => new {c, v}))
            {                
                var variable = GetVariableFromViewAndAttribute(cv.c.View, cv.c.Attribute);                
                solver.SuggestValue(variable, cv.v);
            }
            EndEdit();
        }

        public void SetEditedValues(IEnumerable<IFluentLayout<T>> constraints)
        {
            if (!constraints.Any()) return;

            SetEditConstraints(constraints);

            BeginEdit();
            foreach (var cv in constraints)
            {
                var variable = GetVariableFromViewAndAttribute(cv.View, cv.Attribute);
                solver.SuggestValue(variable, cv.Constant);
            }
            EndEdit();
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
            bool editSession = AddEditVar(solver, variable, width);

            var height = MeasureHeight(view);            

            if(editSession)
            {
                EndEdit();
            }
            solver.Resolve();

            return height;
        }

        bool AddEditVar(ClSimplexSolver solver, ClVariable variable, int value)
        {
            if (!isStay.ContainsKey(variable.Name))
            {
                isStay.Add(variable.Name, 0);
                variable.ChangeValue(value);

                solver.AddStay(variable);
                return false;
            } else
            {
                
                var status = isStay[variable.Name];
                if(status == 0)
                { 
                    solver.AddEditVar(variable);
                    isStay[variable.Name] = 1;
                }

                BeginEdit();
                solver.SuggestValue(variable, value);
                return true;
            }
        }
        
        void BeginEdit()
        {
            solver.BeginEdit();
        }
        
        void EndEdit()
        {
#if true
            solver.Resolve();
#else    
            solver.EndEdit();
            
            foreach(var stay in isStay.Keys.ToArray())
            {
                if(isStay[stay] == 1)
                {
                    isStay[stay] = 0;
                }
            }
#endif
        }

        void AddEditVar(ClSimplexSolver solver, ClVariable variable)
        {
            if (!isStay.ContainsKey(variable.Name))
            {
                isStay.Add(variable.Name, 1);
                solver.AddStay(variable);
                solver.AddEditVar(variable, ClStrength.Strong);
            } else
            {
                var status = isStay[variable.Name];

                if (status == 0)
                {
                    solver.AddEditVar(variable);
                    isStay[variable.Name] = 1;
                }
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

                var multiplier = !Cl.Approx(fluentLayout.Multiplier, 0) ? fluentLayout.Multiplier : 1;
                //make sure to construct the least complicated tableau possible by avoiding needless operations
                if(!Cl.Approx(multiplier, 1)) {                
				    secondExpression = Cl.Plus (
					                    Cl.Times (secondExpression, multiplier),
					                    new ClLinearExpression (fluentLayout.Constant)
                                    );
                } else if(!Cl.Approx(fluentLayout.Constant, 0))
                {
                    secondExpression = Cl.Plus(
                                        secondExpression,
                                        new ClLinearExpression(fluentLayout.Constant)
                                    );
                }
			} else {
				secondExpression = new ClLinearExpression (fluentLayout.Constant);
			}
            
			ClConstraint cn = null;
            var strength = ClStrength.Strong;
            var priority = fluentLayout.Priority / 1000;

			switch (fluentLayout.Relation) {
			case LayoutRelation.Equal:
				cn = new ClLinearEquation (
					firstExpression,
					secondExpression, strength, priority
				);
				break;
			case LayoutRelation.GreaterThanOrEqual:
				cn = new ClLinearInequality (
					firstExpression,
					Cl.Operator.GreaterThanOrEqualTo,
					secondExpression, strength, priority
				);
				break;
			case LayoutRelation.LessThanOrEqual:
				cn = new ClLinearInequality (
					firstExpression,
					Cl.Operator.LessThanOrEqualTo,
					secondExpression, strength, priority
				);
				break;
			}
            
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
                               heightVariable,
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

            
            if(attribute == LayoutAttribute.Left || attribute == LayoutAttribute.Right)
            {
                solver.AddConstraint(new ClLinearInequality(
                            GetVariableFromViewAndAttribute(view, LayoutAttribute.Right),
                            Cl.Operator.GreaterThanOrEqualTo,
                            GetVariableFromViewAndAttribute(view, LayoutAttribute.Left)
                        ));
            }
            if (attribute == LayoutAttribute.Top || attribute == LayoutAttribute.Bottom)
            {
                solver.AddConstraint(new ClLinearInequality(
                            GetVariableFromViewAndAttribute(view, LayoutAttribute.Bottom),
                            Cl.Operator.GreaterThanOrEqualTo,
                            GetVariableFromViewAndAttribute(view, LayoutAttribute.Top)
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

