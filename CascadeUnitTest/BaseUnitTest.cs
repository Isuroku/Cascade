using System;
using System.Reflection;
using CascadeParser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReflectionSerializer;

namespace CascadeUnitTest
{
    public class BaseUnitTest : IParserOwner, ILogPrinter
    {
        protected CParserManager _parser;
        protected CCascadeSerializer _serializer;

        protected int _error_count;

        public BaseUnitTest()
        {
            _parser = new CParserManager(this, this);
            _serializer = new CCascadeSerializer(_parser);
        }

        public string GetTextFromFile(string inFileName)
        {
            throw new NotImplementedException();
        }

        public void LogError(string inText)
        {
            _error_count++;
            Console.WriteLine("Error: {0}", inText);
        }

        public void LogWarning(string inText)
        {
            _error_count++;
            Console.WriteLine("Warning: {0}", inText);
        }

        public void ResetTestState()
        {
            _error_count = 0;
        }

        public void CheckInternalErrors()
        {
            Assert.IsTrue(_error_count == 0, "Internal errors were detected!");
        }

        public void Trace(string inText)
        {
            Console.WriteLine(inText);
        }
    }
}
