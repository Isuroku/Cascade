using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace CascadeUnitTest
{
    public static class Utils
    {
        public static bool IsHashSetEquals<T>(HashSet<T> col1, HashSet<T> col2)
        {
            if (col1 == null && col2 == null)
                return true;

            if (col1 == null || col2 == null)
                return false;

            if (col1.Count != col2.Count)
                return false;

            var e1 = col1.GetEnumerator();
            var e2 = col2.GetEnumerator();

            while (e1.MoveNext() && e2.MoveNext())
            {
                if (!e1.Current.Equals(e2.Current))
                    return false;
            }
            return true;
        }

        public static bool IsArrayEquals(Array col1, Array col2)
        {
            if (col1 == null && col2 == null)
                return true;

            if (col1 == null || col2 == null)
                return false;

            if (col1.Length != col2.Length)
                return false;

            if (col1.Rank != col2.Rank)
                return false;

            var e1 = col1.GetEnumerator();
            var e2 = col2.GetEnumerator();

            while (e1.MoveNext() && e2.MoveNext())
            {
                if (!e1.Current.Equals(e2.Current))
                    return false;
            }
            return true;
        }

        public static bool IsCollectionEquals(ICollection col1, ICollection col2)
        {
            if (col1 == null && col2 == null)
                return true;

            if (col1 == null || col2 == null)
                return false;

            if (col1.Count != col2.Count)
                return false;

            var e1 = col1.GetEnumerator();
            var e2 = col2.GetEnumerator();

            while (e1.MoveNext() && e2.MoveNext())
            {
                if (!e1.Current.Equals(e2.Current))
                    return false;
            }
            return true;
        }

        static CultureInfo _custom_culture;
        public static CultureInfo GetCultureInfoFloatPoint()
        {
            if (_custom_culture == null)
            {
                _custom_culture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
                _custom_culture.NumberFormat.NumberDecimalSeparator = ".";
            }
            return _custom_culture;
        }
    }
}
