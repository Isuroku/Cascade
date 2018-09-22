using System;
using System.Reflection;
using System.Collections.Generic;

namespace ReflectionSerializer
{
    public delegate object MethodHandler(object target, params object[] args);

    public interface IReflectionProvider
    {
        T GetSingleAttributeOrDefault<T>(MemberInfo memberInfo) where T : Attribute, new();
        MemberInfo[] GetSerializableMembers(Type type);
        object Instantiate(Type type);
        object GetValue(MemberInfo member, object instance);
        void SetValue(MemberInfo member, object instance, object value);
        MethodHandler GetDelegate(MethodBase method);
    }
}
