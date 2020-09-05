using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace JUST.Net.Test
{
    public class DynamicObjectExpressionTest
    {
        [Fact]
        public void String_DynamicVAlue_Concat_Test()
        {
            var input = @"{ ""firstName"": ""max"", ""lastName"": ""foo"" }";
            var transformer = @"{ ""friendlyName"": ""~(input.firstName + input.lastName)"" }";

            var jsonTransformer = new JsonTransformer();
            var result = jsonTransformer.Transform(transformer, input);

            var obj = JObject.Parse(result);
            Assert.Equal("maxfoo", obj.SelectToken("$.friendlyName").Value<string>());
            Assert.True(true);
        }
    }
}
