using System;
using System.Reflection;
using CascadeParser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CascadeSerializer;
using MathExpressionParser;

namespace CascadeUnitTest
{
    public class BaseUnitTest : IParserOwner, ILogPrinter, ILogger
    {
        protected CCascadeSerializer _serializer;

        protected int _error_count;

        public BaseUnitTest()
        {
            _serializer = new CCascadeSerializer(this);
        }

        public string GetTextFromFile(string inFileName, object inContextData)
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
