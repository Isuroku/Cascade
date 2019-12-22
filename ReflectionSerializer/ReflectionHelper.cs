using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ComponentModel;
using CascadeParser;
using System.Globalization;

namespace CascadeSerializer
{
    public static class ReflectionHelper
    {
        public static readonly BindingFlags PublicInstanceMembers = BindingFlags.Public | BindingFlags.Instance;

        public static Type GetMemberType(this MemberInfo member)
        {
            if (member is PropertyInfo)
                return (member as PropertyInfo).PropertyType;
            if (member is FieldInfo)
                return (member as FieldInfo).FieldType;
            throw new NotImplementedException();
        }

        public static bool IsHashSet(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(HashSet<>);
        }

        public static bool IsGenericList(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
        }

        public static bool IsGenericCollection(this Type type)
        {
            if (!type.IsGenericType)
                return false;

            Type[] interfaces = type.GetInterfaces();

            if(interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>)))
                return false;

            return interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>));
        }

        public static bool IsGenericDictionary(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>);
        }

        public static bool IsNullable(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static bool IsAtomic(this Type type)
        {
            // Atomic values are immutable and single-valued/can't be decomposed
            return type.IsPrimitive || type.IsEnum || type == typeof(string) || type.IsNullable();
        }

        public static bool IsStruct(this Type type)
        {
            return type.IsValueType && !type.IsEnum && !type.IsPrimitive && !type.IsNullable();
        }

        public static bool IsDefault(object value, IReflectionProvider provider, ILogPrinter inLogger)
        {
            Type type = value.GetType();
            if (type.IsValueType)
            {
                var t = provider.Instantiate(type, inLogger);
                return t.Equals(value);
            }
            else
            {
                if (value == null)
                    return true;

                if (value is string)
                    return string.IsNullOrEmpty((string)value);

                if (value is ICollection)
                    return (value as ICollection).Count == 0;

                if (type.IsHashSet())
                    return (int)provider.GetValue(type
                        .GetProperty("Count", BindingFlags.ExactBinding | ReflectionHelper.PublicInstanceMembers, null, typeof(int), Type.EmptyTypes, null),
                        value) == 0;

                return false;
            }
        }

        //public static bool IsDefault(object value, IReflectionProvider provider)
        //{
        //    if (value is string)
        //        return (string)value == string.Empty;
        //    if (value is char)
        //        return (char)value == '\0';
        //    if(IsINT(value))
        //        return Convert.ToInt64(value) == 0;
        //    if (IsUINT(value))
        //        return Convert.ToUInt64(value) == 0;
        //    if (IsFLOAT(value))
        //        return Convert.ToDecimal(value) == 0;
        //    if (value is bool)
        //        return (bool)value == false;
        //    if (value is ICollection)
        //        return (value as ICollection).Count == 0;
        //    if (value is Array)
        //        return (value as Array).Length == 0;

        //    Type type = value.GetType();
        //    if (type.IsEnum)
        //        return (int)value == 0;

        //    if (type.IsHashSet())
        //        return (int)provider.GetValue(type
        //            .GetProperty("Count", BindingFlags.ExactBinding | ReflectionHelper.PublicInstanceMembers, null, typeof(int), Type.EmptyTypes, null),
        //            value) == 0;


        //    return value.Equals(provider.Instantiate(type));
        //}

        public static object GetDefaultValue(Type inType, IReflectionProvider provider, ILogPrinter inLogger)
        {
            if (inType.IsValueType)
                return provider.Instantiate(inType, inLogger);
            return null;
        }

        public static bool StringToAtomicValue(string inText, Type inType, out object outValue, IReflectionProvider provider, ILogPrinter inLogger)
        {
            if (inType == typeof(string))
            {
                outValue = inText;
                return true;
            }

            if (inType.IsEnum)
            {
                if (string.IsNullOrEmpty(inText))
                {
                    outValue = GetDefaultValue(inType, provider, inLogger);
                    return false;
                }

                try
                {
                    outValue = Enum.Parse(inType, inText, true);
                }
                catch (ArgumentException)
                {
                    outValue = GetDefaultValue(inType, provider, inLogger);
                    return false;
                }
                return true;
            }

            outValue = 0;
            TypeConverter converter = TypeDescriptor.GetConverter(inType);
            try
            {
                outValue = converter.ConvertFromString(null, CultureInfo.InvariantCulture, inText);
            }
            catch (Exception ex)
            {
                //inLogger.LogError(ex.Message);
                return false;
            }
            return true;
        }
    }
}
