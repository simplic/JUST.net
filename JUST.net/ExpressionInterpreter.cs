using DynamicExpresso;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace JUST.net
{
    /// <summary>
    /// JUST expresso interprter implementation
    /// </summary>
    internal class ExpressionInterpreter : Interpreter
    {
        /// <summary>
        /// Initialize interpreter
        /// </summary>
        public ExpressionInterpreter()
        {
            // Add references
            Reference(typeof(DateTime));
            Reference(typeof(String));
        }

        /// <summary>
        /// Initialize core methods
        /// </summary>
        public void Setup(JToken inputObject)
        {
            Func<string, string> valueOfStr = (path) => Transformer.valueof(path, inputObject).ToString();
            Func<string, object> valueOf = (path) => Transformer.valueof(path, inputObject);
            Func<string, int> valueOfInt = (path) =>
            {
                int.TryParse(Transformer.valueof(path, inputObject)?.ToString(), out int r);
                return r;
            };
            Func<string, double> valueOfDouble = (path) =>
            {
                double.TryParse(Transformer.valueof(path, inputObject)?.ToString(), out double r);
                return r;
            };

            SetFunction("valueOf", valueOf);
            SetFunction("valueOfStr", valueOfStr);
            SetFunction("valueOfInt", valueOfInt);
            SetFunction("valueOfDouble", valueOfDouble);
        }

        /// <summary>
        /// Initialize core methods
        /// </summary>
        public void SetContext(JToken arrayElement)
        {
            Func<string, string> valueOfStr = (path) => Transformer.valueof(path, arrayElement).ToString();
            Func<string, object> valueOf = (path) => Transformer.valueof(path, arrayElement);
            Func<string, int> valueOfInt = (path) =>
            {
                int.TryParse(Transformer.valueof(path, arrayElement)?.ToString(), out int r);
                return r;
            };
            Func<string, double> valueOfDouble = (path) =>
            {
                double.TryParse(Transformer.valueof(path, arrayElement)?.ToString(), out double r);
                return r;
            };

            SetFunction("valueOfIter", valueOf);
            SetFunction("valueOfIterStr", valueOfStr);
            SetFunction("valueOfIterInt", valueOfInt);
            SetFunction("valueOfIterDouble", valueOfDouble);
        }
    }
}
