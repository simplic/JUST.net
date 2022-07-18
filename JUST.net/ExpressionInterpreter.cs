using DynamicExpresso;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace JUST.net
{
    /// <summary>
    /// JUST expresso interprter implementation
    /// </summary>
    internal class ExpressionInterpreter : Interpreter
    {
        private bool strictPathHandling;

        /// <summary>
        /// Initialize interpreter
        /// </summary>
        public ExpressionInterpreter()
        {
            // Add references
            Reference(typeof(DateTime));
            Reference(typeof(String));
            Reference(typeof(Convert));
            Reference(typeof(Regex));
        }

        /// <summary>
        /// Initialize core methods
        /// </summary>
        public void Setup(JToken inputObject, bool strictPathHandling)
        {
            this.strictPathHandling = strictPathHandling;

            Func<string, string> valueOfStr = (path) => Transformer.ValueOf(path, inputObject, strictPathHandling, "").ToString();
            Func<string, object> valueOf = (path) => Transformer.ValueOf(path, inputObject, strictPathHandling, new object());
            Func<string, int> valueOfInt = (path) =>
            {
                int.TryParse(Transformer.ValueOf(path, inputObject, strictPathHandling, 0)?.ToString(), out int r);
                return r;
            };
            Func<string, double> valueOfDouble = (path) =>
            {
                double.TryParse(Transformer.ValueOf(path, inputObject, strictPathHandling, 0)?.ToString(), out double r);
                return r;
            };
            Func<string, string> nullToString = (value) => value ?? "";

            Func<string, string, string, string> regex= (value, pattern, defaultValue) =>
            {
                var result = Regex.Match(value, pattern);
                if (result.Success)
                    return result.Value;

                return defaultValue;
            };

            SetFunction("valueOf", valueOf);
            SetFunction("valueOfStr", valueOfStr);
            SetFunction("valueOfInt", valueOfInt);
            SetFunction("valueOfDouble", valueOfDouble);
            SetFunction("nullToString", nullToString);
            SetFunction("regex", regex);
        }

        /// <summary>
        /// Initialize core methods
        /// </summary>
        public void SetContext(JToken arrayElement, JArray array, string expression)
        {
            Func<string, string> valueOfStr = (path) => Transformer.ValueOf(path, arrayElement, strictPathHandling, "").ToString();
            Func<string, object> valueOf = (path) => Transformer.ValueOf(path, arrayElement, strictPathHandling, new object());
            Func<string, int> valueOfInt = (path) =>
            {
                int.TryParse(Transformer.ValueOf(path, arrayElement, strictPathHandling, 0)?.ToString(), out int r);
                return r;
            };
            Func<string, double> valueOfDouble = (path) =>
            {
                double.TryParse(Transformer.ValueOf(path, arrayElement, strictPathHandling, 0)?.ToString(), out double r);
                return r;
            };

            SetFunction("valueOfIter", valueOf);
            SetFunction("valueOfIterStr", valueOfStr);
            SetFunction("valueOfIterInt", valueOfInt);
            SetFunction("valueOfIterDouble", valueOfDouble);

            var identifiers = DetectIdentifiers(expression);
            if (identifiers != null)
            {

                foreach (var identifier in identifiers.UnknownIdentifiers.Concat(identifiers.Identifiers.Select(x => x.Name)))
                {
                    switch (identifier)
                    {
                        case "currentIndex":
                            SetVariable("currentIndex", array.IndexOf(arrayElement));
                            break;

                        case "lastIndex":
                            SetVariable("lastIndex", array.IndexOf(array.Last));
                            break;

                        case "arrayElementStr":
                            {
                                if (arrayElement == null)
                                    SetVariable("currentElementStr", arrayElement.ToString());
                                SetVariable("currentElementStr", arrayElement.ToString());
                            }
                            break;
                    }
                }
            }
        }
    }
}
