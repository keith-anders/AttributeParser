using System;

namespace AttributeCloner.Specs
{
    public static class Extensions
    {
        public static bool AreEqual<T>(this T[] lhs, T[] rhs)
        {
            if (lhs == null)
                return rhs == null;
            if (rhs == null)
                return false;
            if (lhs.Length != rhs.Length)
                return false;
            for (int i = 0; i < lhs.Length; ++i)
                if (!Object.Equals(lhs[i], rhs[i]))
                    return false;
            return true;
        }
    }
}
