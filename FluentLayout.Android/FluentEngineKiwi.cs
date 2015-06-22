using System;
using System.Linq;
using System.Collections.Generic;

using kiwi;
using Cirrious.FluentLayouts;
using FluentLayout.Cassowary;

namespace FluentLayout.Android
{
	public class FluentEngineKiwi<T>
	{
        T _rootView;
        protected Solver solver;
		
        protected Dictionary<ViewAndLayoutAttribute<T>,Variable> variables;
        Dictionary<string, byte> isStay = new Dictionary<string, byte>();


        protected List<IFluentLayout<T>> _constraints;
		protected Dictionary<LayoutAttribute, Variable> _stayVariables = new Dictionary<LayoutAttribute,Variable>();

		protected IViewEngine<T> viewEngine;
		public FluentEngineKiwi (T rootView, IViewEngine<T> viewEngine)
		{
			this.viewEngine = viewEngine;

            _constraints = new List<IFluentLayout<T>>();
			variables = new Dictionary<ViewAndLayoutAttribute<T>, Variable>(
					new ViewAndLayoutEqualityComparer<T>(viewEngine)
				);

            _rootView = rootView;
            Init();

			this.AddRootView (rootView);
		}

        protected void Init()
        {
            solver = new Solver();
            
            var stayVariables = new LayoutAttribute[] {
				    LayoutAttribute.Left,				    
				    LayoutAttribute.Top
			    };

            foreach (var attribute in stayVariables)
            {
                var variable = GetVariableFromViewAndAttribute(_rootView, attribute);
				variable.setValue (0);

                _stayVariables.Add(attribute, variable);
                //solver.AddStay(variable);
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
				solver.addConstraint (GetConstraintFromFluentLayout (fluentLayout));
			}
            
            _constraints.AddRange(fluentLayouts);
		}

        public void AddConstraint(IFluentLayout<T> constraint)
        {
			solver.addConstraint (GetConstraintFromFluentLayout (constraint));
        }

		void SuggestValue(Variable variable, double value)
		{
			if (!solver.hasEditVariable (variable)) {
				solver.addEditVariable (variable, kiwi.kiwi.strong);
			}
			solver.suggestValue(variable, (float)value);
		}

        public void SetEditedValues(IEnumerable<IFluentLayout<T>> constraints, IEnumerable<double> values)
        {
            if (!constraints.Any()) return;

            foreach(var cv in constraints.Zip(values, (c, v) => new {c, v}))
            {                
				var variable = GetVariableFromViewAndAttribute(cv.c.View, cv.c.Attribute);                

                SuggestValue(variable, cv.v);
            }
        }

        public void SetEditedValues(IEnumerable<IFluentLayout<T>> constraints)
        {
            if (!constraints.Any()) return;

            foreach (var cv in constraints)
            {
				var variable = GetVariableFromViewAndAttribute(cv.View, cv.Attribute);
                SuggestValue(variable, cv.Constant);
            }
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
			var variable = GetVariableFromViewAndAttribute(view, LayoutAttribute.Right);

			SuggestValue (variable, width);
			solver.updateVariables ();
            var height = MeasureHeight(view);            

            return height;
        }

		public void UpdateVariables()
		{
			solver.updateVariables ();	
		}

        public double GetValue(T view, LayoutAttribute attribute)
        {
			var variable = GetVariableFromViewAndAttribute(view, attribute);
			return variable.value ();
        }

		static bool Approx(double a, double b)
		{
			return Math.Abs(a - b) < 0.0001;
		}

		protected Constraint GetConstraintFromFluentLayout(IFluentLayout<T> fluentLayout)
		{
			var firstExpression = GetVariableFromViewAndAttribute (fluentLayout.View, fluentLayout.Attribute);

			Expression secondExpression = null;
			Variable secondVariable = null;
			if (fluentLayout.SecondItem != null) {
				secondVariable = GetVariableFromViewAndAttribute (
					fluentLayout.SecondItem.View,
					fluentLayout.SecondItem.Attribute
				);

                var multiplier = !Approx(fluentLayout.Multiplier, 0) ? fluentLayout.Multiplier : 1;

                //make sure to construct the least complicated tableau possible by avoiding needless operations
				if (!Approx (multiplier, 1)) {                
					secondExpression = kiwi.kiwi.__plus__ (
						kiwi.kiwi.__mult__ (secondVariable, multiplier),
						fluentLayout.Constant
					);
				} else if (!Approx (fluentLayout.Constant, 0)) {
					secondExpression = kiwi.kiwi.__plus__ (
						secondVariable,
						fluentLayout.Constant
					);
				} else {
					secondExpression = new Expression (new Term (secondVariable));
				}
			} else {
				secondExpression = new Expression (fluentLayout.Constant);
			}
            
			Constraint constraint = null;
			var strength = fluentLayout.Priority;

			switch (fluentLayout.Relation) {
			case LayoutRelation.Equal:
				constraint = new Constraint (kiwi.kiwi.__minus__ (firstExpression, secondExpression),
					RelationalOperator.OP_EQ, strength);
				break;
			case LayoutRelation.GreaterThanOrEqual:
				constraint = new Constraint (kiwi.kiwi.__minus__ (firstExpression, secondExpression),
					RelationalOperator.OP_GE, strength);
				break;
			case LayoutRelation.LessThanOrEqual:
				constraint = new Constraint (kiwi.kiwi.__minus__ (firstExpression, secondExpression),
					RelationalOperator.OP_LE, strength);
				break;
			}
            
			return constraint;
		}

		public void AddView(T view)
		{
			//setup common constraints
			var top = GetVariableFromViewAndAttribute (view, LayoutAttribute.Top);
			var bottom = GetVariableFromViewAndAttribute (view, LayoutAttribute.Bottom);

			var left = GetVariableFromViewAndAttribute (view, LayoutAttribute.Left);
			var right = GetVariableFromViewAndAttribute (view, LayoutAttribute.Right);

			//top > 0
			solver.addConstraint(new Constraint(new Expression(new Term(top)), RelationalOperator.OP_GE, kiwi.kiwi.required));
			//left > 0
			solver.addConstraint(new Constraint(new Expression(new Term(left)), RelationalOperator.OP_GE, kiwi.kiwi.required));
			//bottom > zero
			solver.addConstraint(new Constraint(kiwi.kiwi.__minus__(bottom, top), RelationalOperator.OP_GE, kiwi.kiwi.required));
			//right > left
			solver.addConstraint(new Constraint(kiwi.kiwi.__minus__(right, left), RelationalOperator.OP_GE, kiwi.kiwi.required));

			//setup composit constraints
			var width = GetCompositeVariableFromViewAndAttribute (view, LayoutAttribute.Width, left, right, top, bottom);
			var height = GetCompositeVariableFromViewAndAttribute (view, LayoutAttribute.Height, left, right, top, bottom);
			solver.addConstraint(new Constraint(new Expression(new Term(width)), RelationalOperator.OP_GE, kiwi.kiwi.required));
			solver.addConstraint(new Constraint(new Expression(new Term(height)), RelationalOperator.OP_GE, kiwi.kiwi.required));

			var centerX = GetCompositeVariableFromViewAndAttribute (view, LayoutAttribute.CenterX, left, right, top, bottom);
			var centerY = GetCompositeVariableFromViewAndAttribute (view, LayoutAttribute.CenterY, left, right, top, bottom);
			solver.addConstraint(new Constraint(new Expression(new Term(centerX)), RelationalOperator.OP_GE, kiwi.kiwi.required));
			solver.addConstraint(new Constraint(new Expression(new Term(centerY)), RelationalOperator.OP_GE, kiwi.kiwi.required));
		}
		public void AddRootView(T view)
		{
			//setup common constraints
			var top = GetVariableFromViewAndAttribute (view, LayoutAttribute.Top);
			var bottom = GetVariableFromViewAndAttribute (view, LayoutAttribute.Bottom);

			var left = GetVariableFromViewAndAttribute (view, LayoutAttribute.Left);
			var right = GetVariableFromViewAndAttribute (view, LayoutAttribute.Right);

			//top == 0
			solver.addConstraint(new Constraint(new Expression(new Term(top)), RelationalOperator.OP_EQ, kiwi.kiwi.required));
			//left == 0
			solver.addConstraint(new Constraint(new Expression(new Term(left)), RelationalOperator.OP_EQ, kiwi.kiwi.required));
			//bottom > zero
			solver.addConstraint(new Constraint(kiwi.kiwi.__minus__(bottom, top), RelationalOperator.OP_GE, kiwi.kiwi.required));
			//right > left
			solver.addConstraint(new Constraint(kiwi.kiwi.__minus__(right, left), RelationalOperator.OP_GE, kiwi.kiwi.required));

			//setup composit constraints
			var width = GetCompositeVariableFromViewAndAttribute (view, LayoutAttribute.Width, left, right, top, bottom);
			var height = GetCompositeVariableFromViewAndAttribute (view, LayoutAttribute.Height, left, right, top, bottom);
			solver.addConstraint(new Constraint(new Expression(new Term(width)), RelationalOperator.OP_GE, kiwi.kiwi.required));
			solver.addConstraint(new Constraint(new Expression(new Term(height)), RelationalOperator.OP_GE, kiwi.kiwi.required));

			var centerX = GetCompositeVariableFromViewAndAttribute (view, LayoutAttribute.CenterX, left, right, top, bottom);
			var centerY = GetCompositeVariableFromViewAndAttribute (view, LayoutAttribute.CenterY, left, right, top, bottom);
			solver.addConstraint(new Constraint(new Expression(new Term(centerX)), RelationalOperator.OP_GE, kiwi.kiwi.required));
			solver.addConstraint(new Constraint(new Expression(new Term(centerY)), RelationalOperator.OP_GE, kiwi.kiwi.required));
		}

		protected Variable GetCompositeVariableFromViewAndAttribute(T view, LayoutAttribute attribute, Variable leftVar, Variable rightVar, Variable topVar, Variable bottomVar)
        {            
            switch (attribute)
            {
                case LayoutAttribute.Width:
                    {
						var widthExpression = kiwi.kiwi.__minus__(
                            rightVar,
                            leftVar
                        );
					
                        var widthVariable = new Variable(viewEngine.GetViewName(view) + ".Width");

                        solver.addConstraint(new Constraint(
							   kiwi.kiwi.__minus__(widthVariable, widthExpression), 
							   RelationalOperator.OP_EQ,
							   kiwi.kiwi.required));

                        AddVariableForViewAndAttribute(view, attribute, widthVariable);
                        return widthVariable;
                    }
                case LayoutAttribute.Height:
                    {
						var heightExpression = kiwi.kiwi.__minus__(
                            bottomVar,
                            topVar
                        );

                        var heightVariable = new Variable(viewEngine.GetViewName(view) + ".Height");
                        
                        solver.addConstraint(new Constraint(
									kiwi.kiwi.__minus__(heightVariable, heightExpression),
									RelationalOperator.OP_EQ, 
									kiwi.kiwi.required));

                        AddVariableForViewAndAttribute(view, attribute, heightVariable);
                        return heightVariable;
                    }
                case LayoutAttribute.CenterX:
                    {
						var centerXExpression = kiwi.kiwi.__plus__(
							kiwi.kiwi.__div__(leftVar, 2),
							kiwi.kiwi.__div__(rightVar, 2)
                        );

                        var centerXVariable = new Variable(viewEngine.GetViewName(view) + ".CenterX");
                        solver.addConstraint(new Constraint(
							   kiwi.kiwi.__minus__(centerXVariable, centerXExpression),
							   RelationalOperator.OP_EQ,
								kiwi.kiwi.required
                           ));

                        AddVariableForViewAndAttribute(view, attribute, centerXVariable);
                        return centerXVariable;
                    }
                case LayoutAttribute.CenterY:
                    {
						var centerYExpression = kiwi.kiwi.__plus__(
							kiwi.kiwi.__div__(topVar, 2),
							kiwi.kiwi.__div__(bottomVar, 2)
                        );

                        var centerYVariable = new Variable(viewEngine.GetViewName(view) + ".CenterY");
                        solver.addConstraint(new Constraint(
							   kiwi.kiwi.__minus__(centerYVariable, centerYExpression),
							   RelationalOperator.OP_EQ,
							 	kiwi.kiwi.required                      
                           ));

                        AddVariableForViewAndAttribute(view, attribute, centerYVariable);
                        return centerYVariable;
                    }
            }

            return null;
        }

		protected Variable GetVariableFromViewAndAttribute(T view, LayoutAttribute attribute)
		{
			Variable variable = null;
			var viewAndAttribute = new ViewAndLayoutAttribute<T> (view, attribute);

			if (!variables.TryGetValue (viewAndAttribute, out variable)) {
				var value = viewEngine.GetAttribute (view, attribute);
				var name = string.Format ("{0}.{1}", viewEngine.GetViewName (view), attribute.ToString ());

				variable = new Variable (name);
				variable.setValue (value);
				variables.Add (viewAndAttribute, variable);
			}
            
			return variable; 
		}

        protected bool HasVariableForViewAndAttribute(T view, LayoutAttribute attribute)
        {
            var viewAndAttribute = new ViewAndLayoutAttribute<T>(view, attribute);
            return variables.ContainsKey(viewAndAttribute);            
        }

        protected void AddVariableForViewAndAttribute(T view, LayoutAttribute attribute, Variable variable)
        {
            if (HasVariableForViewAndAttribute(view, attribute)) return;

            var viewAndAttribute = new ViewAndLayoutAttribute<T> (view, attribute);
            variables.Add(viewAndAttribute, variable);
        }
	}
}

