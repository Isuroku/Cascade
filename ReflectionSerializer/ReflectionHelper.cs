using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ComponentModel;
using CascadeParser;

namespace ReflectionSerializer
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

        public static bool IsINT(object value)
        {
            return value is sbyte || value is short || value is int || value is long;
        }

        public static bool IsUINT(object value)
        {
            return value is byte || value is ushort || value is uint || value is ulong;
        }

        public static bool IsFLOAT(object value)
        {
            return value is float || value is double || value is decimal;
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

        public static bool StringToAtomicValue(string inText, Type inType, out object outValue)
        {
            if (inType == typeof(string))
            {
                outValue = inText;
                return true;
            }
            if (inType.IsEnum)
            {
                outValue = Enum.Parse(inType, inText);
                return true;
            }

            outValue = 0;
            TypeConverter converter = TypeDescriptor.GetConverter(inType);
            try
            {
                outValue = converter.ConvertFromString(inText);
            }
            catch(NotSupportedException)
            {
                return false;
            }
            return true;
        }
    }
}
