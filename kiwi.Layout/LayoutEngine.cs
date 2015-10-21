using System;
using System.Collections.Generic;
using System.Linq;

namespace kiwi.Layout
{
	using Fluent;

	public sealed class LayoutEngine<T> : IDisposable
	{
		protected Solver solver = new Solver();
		
		protected Dictionary<ViewAndLayoutAttribute<T>,Variable> variables = new Dictionary<ViewAndLayoutAttribute<T>, Variable>(
				new ViewAndLayoutEqualityComparer<T>()
			);
        
		protected List<IFluentLayout<T>> _constraints = new List<IFluentLayout<T>>();
		protected IViewEngine<T> viewEngine;

		public LayoutEngine(T rootView, IViewEngine<T> viewEngine)
		{
			this.viewEngine = viewEngine;
            
			this.AddRootView (rootView);
		}
			
        public void RemoveAllConstraints()
        {
            _constraints.Clear();

			foreach (var variable in variables.Values) {
				variable.Dispose ();
			}

            variables.Clear();

			solver.reset ();
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

		T rootView;
		Constraint rootLeftConstraint;
		Constraint rootTopConstraint;

		public int PaddingLeft { get; protected set; }
		public int PaddingRight { get; protected  set; }
		public int PaddingTop { get; protected set; }
		public int PaddingBottom { get; protected set; }

		public void SetPadding(int left, int top, int right, int bottom)
		{
			if (left != PaddingLeft || top != PaddingTop)
			{
				SetRootLeftTopPadding(left, top);
			}
			PaddingLeft = left;
			PaddingTop = top;
			PaddingRight = right;
			PaddingBottom = bottom;
		}

		void SuggestValue(Variable variable, double value)
		{
			if (!solver.hasEditVariable (variable)) {
				solver.addEditVariable (variable, kiwi.strong);
			}
			solver.suggestValue(variable, (float)value);
		}

        List<Constraint> goneConstraints = new List<Constraint>();
        public void SetGoneViews(IEnumerable<T> views)
        {
            /*
            foreach (var ev in editVariables)
            {
                solver.removeEditVariable(ev);
            }
            editVariables.Clear();
            */

            foreach (var goneConstraint in goneConstraints)
            {
                solver.removeConstraint(goneConstraint);
            }
            goneConstraints.Clear();

            foreach (var v in views)
            {
                //var width = GetVariableFromViewAndAttribute(v, LayoutAttribute.Width);
                //var height = GetVariableFromViewAndAttribute(v, LayoutAttribute.Height);

                //SuggestValue(width, 1,  kiwi.create(3.0, 0, 0));
                //SuggestValue(height, 1, kiwi.create(3.0, 0, 0));

                //editVariables.Add(width);
                //editVariables.Add(height);

                var left = GetVariableFromViewAndAttribute(v, LayoutAttribute.Left);
                var right = GetVariableFromViewAndAttribute(v, LayoutAttribute.Right);
                var top = GetVariableFromViewAndAttribute(v, LayoutAttribute.Top);
                var bottom = GetVariableFromViewAndAttribute(v, LayoutAttribute.Bottom);

                var c1 = new Constraint(kiwi.__minus__(left, right), RelationalOperator.OP_EQ, kiwi.required);
                var c2 = new Constraint(kiwi.__minus__(top, bottom), RelationalOperator.OP_EQ, kiwi.required);

                solver.addConstraint(c1);
                solver.addConstraint(c2);

                goneConstraints.Add(c1);
                goneConstraints.Add(c2);
            }
            solver.updateVariables ();
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

			SuggestValue (variable, width + PaddingLeft - PaddingRight);
			solver.updateVariables ();
            var height = MeasureHeight(view);            

			return height + PaddingTop + PaddingBottom;
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
					secondExpression = kiwi.__plus__ (
						kiwi.__mult__ (secondVariable, multiplier),
						fluentLayout.Constant
					);
				} else if (!Approx (fluentLayout.Constant, 0)) {
					secondExpression = kiwi.__plus__ (
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
				constraint = new Constraint (kiwi.__minus__ (firstExpression, secondExpression),
					RelationalOperator.OP_EQ, strength);
				break;
			case LayoutRelation.GreaterThanOrEqual:
				constraint = new Constraint (kiwi.__minus__ (firstExpression, secondExpression),
					RelationalOperator.OP_GE, strength);
				break;
			case LayoutRelation.LessThanOrEqual:
				constraint = new Constraint (kiwi.__minus__ (firstExpression, secondExpression),
					RelationalOperator.OP_LE, strength);
				break;
			}
            
			return constraint;
		}

		public void AddView(T view)
		{
			//for "normal" child views left and right position are greater then zero

			var top = GetVariableFromViewAndAttribute (view, LayoutAttribute.Top);
			var left = GetVariableFromViewAndAttribute (view, LayoutAttribute.Left);

			//for the root view, left and right position are equal zero
			//top == 0
			solver.addConstraint(new Constraint(new Expression(new Term(top)), RelationalOperator.OP_GE, kiwi.required));
			//left == 0
			solver.addConstraint(new Constraint(new Expression(new Term(left)), RelationalOperator.OP_GE, kiwi.required));

			AddViewConstraints (view);
		}

		protected void AddRootView(T view)
		{
			rootView = view;
			SetRootLeftTopPadding (PaddingLeft, PaddingTop);
			AddViewConstraints (view);
		}

		protected void SetRootLeftTopPadding(int leftPadding, int topPadding)
		{
			var top = GetVariableFromViewAndAttribute (rootView, LayoutAttribute.Top);
			var left = GetVariableFromViewAndAttribute (rootView, LayoutAttribute.Left);

			if (rootTopConstraint != null) 
			{
				solver.removeConstraint (rootTopConstraint);
				rootTopConstraint.Dispose();
			}
			if (rootLeftConstraint != null) 
			{
				solver.removeConstraint (rootLeftConstraint);
				rootLeftConstraint.Dispose();
			}

			//for the root view, left and right position are equal zero
			//top == 0
			rootTopConstraint = new Constraint(kiwi.__minus__(top, new Expression(topPadding)), RelationalOperator.OP_EQ, kiwi.required);
			solver.addConstraint(rootTopConstraint);
			//left == 0
			rootLeftConstraint = new Constraint(kiwi.__minus__(left, new Expression(leftPadding)), RelationalOperator.OP_EQ, kiwi.required);
			solver.addConstraint(rootLeftConstraint);
		}

		protected void AddViewConstraints(T view)
		{
			//setup common constraints
			var top = GetVariableFromViewAndAttribute (view, LayoutAttribute.Top);
			var bottom = GetVariableFromViewAndAttribute (view, LayoutAttribute.Bottom);

			var left = GetVariableFromViewAndAttribute (view, LayoutAttribute.Left);
			var right = GetVariableFromViewAndAttribute (view, LayoutAttribute.Right);

			//bottom > zero
			solver.addConstraint(new Constraint(kiwi.__minus__(bottom, top), RelationalOperator.OP_GE, kiwi.required));
			//right > left
			solver.addConstraint(new Constraint(kiwi.__minus__(right, left), RelationalOperator.OP_GE, kiwi.required));

			//setup composit constraints
			var width = GetCompositeVariableFromViewAndAttribute (view, LayoutAttribute.Width, left, right, top, bottom);
			var height = GetCompositeVariableFromViewAndAttribute (view, LayoutAttribute.Height, left, right, top, bottom);
			solver.addConstraint(new Constraint(new Expression(new Term(width)), RelationalOperator.OP_GE, kiwi.required));
			solver.addConstraint(new Constraint(new Expression(new Term(height)), RelationalOperator.OP_GE, kiwi.required));

			var centerX = GetCompositeVariableFromViewAndAttribute (view, LayoutAttribute.CenterX, left, right, top, bottom);
			var centerY = GetCompositeVariableFromViewAndAttribute (view, LayoutAttribute.CenterY, left, right, top, bottom);
			solver.addConstraint(new Constraint(new Expression(new Term(centerX)), RelationalOperator.OP_GE, kiwi.required));
			solver.addConstraint(new Constraint(new Expression(new Term(centerY)), RelationalOperator.OP_GE, kiwi.required));
		}

		protected Variable GetCompositeVariableFromViewAndAttribute(T view, LayoutAttribute attribute, Variable leftVar, Variable rightVar, Variable topVar, Variable bottomVar)
        {            
            switch (attribute)
            {
                case LayoutAttribute.Width:
                    {
						var widthExpression = kiwi.__minus__(
                            rightVar,
                            leftVar
                        );
					
                        var widthVariable = new Variable(viewEngine.GetViewName(view) + ".Width");

                        solver.addConstraint(new Constraint(
							   kiwi.__minus__(widthVariable, widthExpression), 
							   RelationalOperator.OP_EQ,
							   kiwi.required));

                        AddVariableForViewAndAttribute(view, attribute, widthVariable);
                        return widthVariable;
                    }
                case LayoutAttribute.Height:
                    {
						var heightExpression = kiwi.__minus__(
                            bottomVar,
                            topVar
                        );

                        var heightVariable = new Variable(viewEngine.GetViewName(view) + ".Height");
                        
                        solver.addConstraint(new Constraint(
									kiwi.__minus__(heightVariable, heightExpression),
									RelationalOperator.OP_EQ, 
									kiwi.required));

                        AddVariableForViewAndAttribute(view, attribute, heightVariable);
                        return heightVariable;
                    }
                case LayoutAttribute.CenterX:
                    {
						var centerXExpression = kiwi.__plus__(
							kiwi.__div__(leftVar, 2),
							kiwi.__div__(rightVar, 2)
                        );

                        var centerXVariable = new Variable(viewEngine.GetViewName(view) + ".CenterX");
                        solver.addConstraint(new Constraint(
							   kiwi.__minus__(centerXVariable, centerXExpression),
							   RelationalOperator.OP_EQ,
								kiwi.required
                           ));

                        AddVariableForViewAndAttribute(view, attribute, centerXVariable);
                        return centerXVariable;
                    }
                case LayoutAttribute.CenterY:
                    {
						var centerYExpression = kiwi.__plus__(
							kiwi.__div__(topVar, 2),
							kiwi.__div__(bottomVar, 2)
                        );

                        var centerYVariable = new Variable(viewEngine.GetViewName(view) + ".CenterY");
                        solver.addConstraint(new Constraint(
							   kiwi.__minus__(centerYVariable, centerYExpression),
							   RelationalOperator.OP_EQ,
							 	kiwi.required                      
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

		public void Dispose()
		{
			//TODO dispose all the intermediate objects created by kiwi.__minus__() for example
			_constraints.Clear();

			foreach (var variable in variables.Values) {
				variable.Dispose ();
			}

			variables.Clear();
			solver.Dispose ();
		}
	}
}