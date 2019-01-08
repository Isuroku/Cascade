using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace MathExpressionParser
{
    public static class Utils
    {
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

        public static T ToEnum<T>(string value, T default_value)
        {
            if (string.IsNullOrEmpty(value) || !Enum.IsDefined(typeof(T), value))
                return default_value;

            return (T)Enum.Parse(typeof(T), value);
        }
    }
}
