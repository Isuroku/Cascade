using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    class CTest2Base
    {
        private float _base_float;

        public CTest2Base()
        {
            _base_float = 3.1415f;
        }

        public virtual void Change()
        {
            _base_float = 101.909f;
        }
    }

    class CTest2 : CTest2Base
    {
        private int _int;

        public int PubProp { get; set; }

        public CTest2()
        {
            _int = 9;
            PubProp = 11;
        }

        public override void Change()
        {
            base.Change();
            _int = 777;
            PubProp = 888;
        }

        public static CTest2 CreateTestObject()
        {
            CTest2 obj = new CTest2();
            obj.Change();
            return obj;
        }
    }
}
