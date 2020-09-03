using JUST.net;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace JUST.Net.Test
{
    public class ExpressionParserTest
    {
        [Fact]
        public void ExpressionTest_False()
        {
            var isExpression = ExpressionParserUtilities.IsExpression("~Sample Value", out string expression);

            Assert.False(isExpression);
            Assert.Equal("", expression);
        }

        [Fact]
        public void ExpressionTest_NoNested_False()
        {
            var isExpression = ExpressionParserUtilities.IsExpression("~Sample ~(some-not-expression) Value", out string expression);

            Assert.False(isExpression);
            Assert.Equal("", expression);
        }

        [Fact]
        public void ExpressionTest_True()
        {
            var isExpression = ExpressionParserUtilities.IsExpression("~(func(\"Sample Value\"))", out string expression);

            Assert.True(isExpression);
            Assert.Equal("func(\"Sample Value\")", expression);
        }

        [Fact]
        public void ExpressionTest_EmptyExpression_True()
        {
            var isExpression = ExpressionParserUtilities.IsExpression("~()", out string expression);

            Assert.True(isExpression);
            Assert.Equal("", expression);
        }
    }
}
