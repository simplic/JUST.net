using System;
using System.Collections.Generic;
using System.Text;

namespace JUST.net
{
    /// <summary>
    /// Expression utilities
    /// </summary>
    public static class ExpressionParserUtilities
    {
        /// <summary>
        /// Checks whether a string represents an expression
        /// </summary>
        /// <param name="value">Json string value</param>
        /// <param name="expression">If value is an expression, the expression string will be returned as this output parameter</param>
        /// <returns>True if the input string is an expression</returns>
        public static bool IsExpression(string value, out string expression)
        {
            expression = "";

            if (string.IsNullOrWhiteSpace(value))
                return false;

            var trimmedValue = value.Trim();
            if (trimmedValue.StartsWith("~(") && trimmedValue.EndsWith(")"))
            {
                expression = trimmedValue.Substring(2, trimmedValue.Length - 3);
                return true;
            }

            return false;
        }
    }
}
