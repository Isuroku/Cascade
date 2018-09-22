using System;
using System.Reflection;

namespace ReflectionSerializer
{
    class EmitReflector : BaseReflector
    {
        public override object Instantiate(Type type)
        {
            return EmitHelper.CreateParameterlessConstructorHandler(type)();
        }

        public override object GetValue(MemberInfo member, object instance)
        {
            if (member is PropertyInfo)
                return EmitHelper.CreatePropertyGetterHandler(member as PropertyInfo)(instance);
            if (member is FieldInfo)
                return EmitHelper.CreateFieldGetterHandler(member as FieldInfo)(instance);
            throw new NotImplementedException();
        }

        public override void SetValue(MemberInfo member, object instance, object value)
        {
            if (member is PropertyInfo)
                EmitHelper.CreatePropertySetterHandler(member as PropertyInfo)(instance, value);
            else if (member is FieldInfo)
                EmitHelper.CreateFieldSetterHandler(member as FieldInfo)(instance, value);
            else throw new NotImplementedException();
        }

        public override MethodHandler GetDelegate(MethodBase method)
        {
            return EmitHelper.CreateMethodHandler(method);
        }
    }
}
