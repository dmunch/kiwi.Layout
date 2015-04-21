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
    }

	public class FluentEngine<T>
	{
        protected Dictionary<T, SolverWithView> solvers;
		protected Dictionary<ViewAndLayoutAttribute<T>,ClVariable> variables;

        protected Dictionary<T, List<IFluentLayout<T>>> _constraints;
        protected Dictionary<LayoutAttribute, ClVariable> _stayVariables = new Dictionary<LayoutAttribute,ClVariable>();

		protected IViewEngine<T> viewEngine;
		public FluentEngine (IViewEngine<T> viewEngine)
		{
			this.viewEngine = viewEngine;

            _constraints = new Dictionary<T, List<IFluentLayout<T>>>();
            solvers = new Dictionary<T, SolverWithView>();
			variables = 
				new Dictionary<ViewAndLayoutAttribute<T>, ClVariable>(
					new ViewAndLayoutEqualityComparer<T>(viewEngine)
				);
		}

		public string AddConstraints (T view, IEnumerable<IFluentLayout<T>> fluentLayouts)
		{
			SolverWithView solver = GetSolver (view);
            
			foreach (var fluentLayout in fluentLayouts) {
				foreach(var constraint in GetConstraintsFromFluentLayout (fluentLayout))
                {
                    solver.Solver.AddConstraint(constraint);
                }
			}

            if (!_constraints.ContainsKey(view))
            {
                _constraints.Add(view, new List<IFluentLayout<T>>());
            }
            List<IFluentLayout<T>> constraints = _constraints[view];
            constraints.AddRange(fluentLayouts);

			return solver.ToString ();
		}

        public void SetEditConstraints(T view, IFluentLayout<T> constraint)
        {
            var solver = GetSolver(view).Solver;
            if (constraint.SecondItem != null)
            {
                throw new Exception("Edit constraint can't have a right side");
            }

            var variable = GetVariableFromViewAndAttribute(view, constraint.Attribute);
            solver.AddEditVar(variable);            
        }

        
        protected void SetEditConstraints(T view, IEnumerable<IFluentLayout<T>> constraints)
        {
            var solver = GetSolver(view).Solver;

            foreach(var constraint in constraints)
            { 
                if (constraint.SecondItem != null)
                {
                    throw new Exception("Edit constraint can't have a right side");
                }

                var variable = GetVariableFromViewAndAttribute(constraint.View, constraint.Attribute);
                solver.AddStay(variable);
                solver.AddEditVar(variable);
            }
        }

        public void SetEditedValues(T view, IEnumerable<IFluentLayout<T>> constraints, IEnumerable<double> values)
        {
            if (!constraints.Any()) return;

            var solver = GetSolver(view).Solver;

            
            SetEditConstraints(view, constraints);
            solver.AutoSolve = false;
            solver.BeginEdit();
            foreach(var cv in constraints.Zip(values, (c, v) => new {c, v}))
            {                
                var variable = GetVariableFromViewAndAttribute(cv.c.View, cv.c.Attribute);
                variable.ChangeValue(cv.v);
                solver.SuggestValue(variable, cv.v);
            }
            //solver.Resolve();
            solver.AutoSolve = true;
            solver.EndEdit();
            /*
            foreach (var cv in constraints.Zip(values, (c, v) => new { c, v }))
            {
                var variable = GetVariableFromViewAndAttribute(cv.c.View, cv.c.Attribute);
                solver.AddStay(variable);
                solver.SetEditedValue(variable, cv.v);
            } */           
        }

		public string Solve(T view)
		{
			SolverWithView solver = GetSolver (view);

            if(solver.AlreadySolved)
            {
                var changedStayVariables = _stayVariables.Where(v =>  !Cl.Approx((double)viewEngine.GetAttribute(view, v.Key), v.Value.Value)).ToArray();
                if(changedStayVariables.Any())
                {
                    
                    foreach (var stayVariable in changedStayVariables)
                    {
                        solver.Solver.AddEditVar(stayVariable.Value);
                    }
                    solver.Solver.BeginEdit();
                    foreach (var stayVariable in changedStayVariables)
                    {
                        solver.Solver.SuggestValue(stayVariable.Value, viewEngine.GetAttribute(view, stayVariable.Key));
                    }
                    solver.Solver.EndEdit();
                    /*
                    foreach (var stayVariable in changedStayVariables)
                    {
                        solver.Solver.SetEditedValue(stayVariable.Value, viewEngine.GetAttribute(view, stayVariable.Key));
                    }*/
                }
            } else
            {
                //initialize stay variables
                //parent view always stays
                
                var stayVariables = new LayoutAttribute[] {
				    LayoutAttribute.Left,
				    LayoutAttribute.Right,
				    LayoutAttribute.Top/*,
				    LayoutAttribute.Bottom*/
			    };
                foreach (var attribute in stayVariables)
                {
                    var variable = GetVariableFromViewAndAttribute(view, attribute);
                    variable.ChangeValue(viewEngine.GetAttribute(view, attribute));

                    _stayVariables.Add(attribute, variable);
                   
                    solver.Solver.AddStay(variable, ClStrength.Strong);
                    //solver.Solver.AddEditVar(variable, ClStrength.Strong);
                }
                /*
                solver.Solver.BeginEdit();
                foreach (var attribute in stayVariables)
                {
                    var variable = GetVariableFromViewAndAttribute(view, attribute);
                    solver.Solver.SuggestValue(variable, viewEngine.GetAttribute(view, attribute));
                }*/

                
                solver.AlreadySolved = true;
                solver.Solver.Solve();
                //solver.Solver.Resolve();
            }			
			
			return solver.ToString ();
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

        protected SolverWithView GetSolver(T view)
		{
            SolverWithView solver = null;

			if (!solvers.TryGetValue (view, out solver)) {
                solver = new SolverWithView();
				solvers.Add (view, solver);
			}

			return solver;
		}

		protected IEnumerable<ClConstraint> GetConstraintsFromFluentLayout(IFluentLayout<T> fluentLayout)
		{
            var constraints = new List<ClConstraint>();
			ClLinearExpression firstExpression = null;
            ClLinearEquation egality = null;
			firstExpression = GetExpressionFromViewAndAttribute (fluentLayout.View, fluentLayout.Attribute, out egality);

            if(egality != null)
            {
                constraints.Add(egality);
            }

			ClLinearExpression secondExpression = null;
			if (fluentLayout.SecondItem != null) {
				secondExpression = GetExpressionFromViewAndAttribute (
					fluentLayout.SecondItem.View,
					fluentLayout.SecondItem.Attribute,
                    out egality
				);
                
                var multiplier = fluentLayout.Multiplier != 0 ? fluentLayout.Multiplier : 1;
				secondExpression = Cl.Plus (
					Cl.Times (secondExpression, multiplier),
					new ClLinearExpression (fluentLayout.Constant)
				);
                
                if (egality != null)
                {
                    constraints.Add(egality);
                }
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
            
            if (fluentLayout.Attribute == LayoutAttribute.Width || fluentLayout.Attribute == LayoutAttribute.Height)
            {
                constraints.Add(new ClLinearInequality(
                    firstExpression,
                    Cl.Operator.GreaterThanOrEqualTo,
                    new ClLinearExpression(0)
                ));
            }
			
			return constraints;
		}

		protected ClLinearExpression GetExpressionFromViewAndAttribute(T view, LayoutAttribute attribute, out ClLinearEquation egality)
		{
            egality = null;
            if (HasVariableForViewAndAttribute(view, attribute))
            {
                return new ClLinearExpression(GetVariableFromViewAndAttribute(view, attribute));
            }
                
			switch(attribute)
			{
			    case LayoutAttribute.Width:
                    { 
				        var leftVar = GetVariableFromViewAndAttribute (view, LayoutAttribute.Left);
				        var rightVar = GetVariableFromViewAndAttribute (view, LayoutAttribute.Right);
				
                        var widthExpression = Cl.Minus (
					        new ClLinearExpression(rightVar), 
					        new ClLinearExpression(leftVar)
				        );

                        var widthVariable = new ClVariable(viewEngine.GetViewName(view) + ".Width");
                        egality = new ClLinearEquation(widthVariable, widthExpression);
                        egality.Strength = ClStrength.Required;

                        AddVariableForViewAndAttribute(view, attribute, widthVariable);
                        return new ClLinearExpression(widthVariable);
                    }
			    case LayoutAttribute.Height:
                    { 
				        var topVar = GetVariableFromViewAndAttribute (view, LayoutAttribute.Top);
				        var bottomVar = GetVariableFromViewAndAttribute (view, LayoutAttribute.Bottom);

				        var heightExpression = Cl.Minus (
                            new ClLinearExpression(bottomVar), 
					        new ClLinearExpression(topVar)
				        );

                        var heightVariable = new ClVariable(viewEngine.GetViewName(view) + ".Height");
                        egality = new ClLinearEquation(heightVariable, heightExpression);
                        egality.Strength = ClStrength.Required;

                        AddVariableForViewAndAttribute(view, attribute, heightVariable);
                        return new ClLinearExpression(heightVariable);
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
                        egality = new ClLinearEquation(centerXVariable, centerXExpression);
                        egality.Strength = ClStrength.Required;

                        AddVariableForViewAndAttribute(view, attribute, centerXVariable);
                        return new ClLinearExpression(centerXVariable);
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
                        egality = new ClLinearEquation(centerYVariable, centerYExpression);
                        egality.Strength = ClStrength.Required;

                        AddVariableForViewAndAttribute(view, attribute, centerYVariable);
                        return new ClLinearExpression(centerYVariable);
                    }
			    default:
				    var variable = GetVariableFromViewAndAttribute (view, attribute);
				    return new ClLinearExpression (variable);
			}
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

