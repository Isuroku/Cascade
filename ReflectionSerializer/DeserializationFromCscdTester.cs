using CascadeParser;
using System;
using System.Collections.Generic;

namespace CascadeSerializer
{
    public class CDeserializationFromCscdTester
    {
        private string _field;

        public CDeserializationFromCscdTester()
        {
            _field = string.Empty;
        }

        public CDeserializationFromCscdTester(string value)
        {
            _field = value;
        }

        public bool Equals(CDeserializationFromCscdTester other)
        {
            return string.Equals(_field, other._field, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CDeserializationFromCscdTester)obj);
        }

        public override int GetHashCode()
        {
            return _field.GetHashCode();
        }

        public CDeserializationFromCscdTester(IKey key, ILogPrinter inLogger)
        {
            if (key.GetValuesCount() == 0)
            {
                inLogger.LogError(string.Format("NamedId.CscdConverter: Key {0} hasnt value!", key.GetPath()));
                return;
            }
            _field = key.GetValue(0).ToString();
        }

        //public void DeserializationFromCscd(IKey key, ILogPrinter inLogger)
        //{
        //    if (key.GetValuesCount() == 0)
        //    {
        //        inLogger.LogError(string.Format("NamedId.CscdConverter: Key {0} hasnt value!", key.GetPath()));
        //        return;
        //    }
        //    _field = key.GetValue(0).ToString();
        //}

        public void SerializationToCscd(IKey key, ILogPrinter inLogger)
        {
            key.AddValue(_field);
        }
    }
}
