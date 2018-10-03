
using System;
using System.Collections.Generic;
using CascadeParser;
using ReflectionSerializer;

namespace Parser
{

    public class CAIActionDescrs : Dictionary<string, string>
    {
        public int _some_int;

        public static CAIActionDescrs CreateTestObject()
        {
            CAIActionDescrs obj = new CAIActionDescrs();
            obj.Add("aikey", "aivalue");
            obj._some_int = 99;
            return obj;
        }
    }
}
