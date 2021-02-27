using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Size.Sharp
{
    internal class ReferenceComparer : IEqualityComparer<object>
    {

        public static readonly ReferenceComparer Instance = new ReferenceComparer();

        bool IEqualityComparer<object>.Equals(object x, object y)
        {
            return ReferenceEquals(x, y);
        }

        int IEqualityComparer<object>.GetHashCode(object obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }

    }
}
