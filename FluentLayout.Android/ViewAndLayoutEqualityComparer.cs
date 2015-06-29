using System;
using System.Collections.Generic;
using Cirrious.FluentLayouts;

namespace FluentLayout.Android
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
}