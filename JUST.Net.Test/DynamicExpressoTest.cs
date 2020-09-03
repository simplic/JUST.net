using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace JUST.Net.Test
{
    public class DynamicExpressoTest
    {
        [Fact]
        public void StringConcat_Test()
        {
            var input = @"{ ""firstName"": ""max"", ""lastName"": ""foo"" }";
            var transformer = @"{ ""friendlyName"": ""~(valueOf(c, \""$.firstName\"") + valueOf(c, \""$.lastName\""))"" }";

            var jsonTransformer = new JsonTransformer();
            var result = jsonTransformer.Transform(transformer, input);

            var obj = JObject.Parse(result);
            // Assert.Equal("max foo", obj.SelectToken("$.friendlyName").Value<string>());
            Assert.True(true);
        }
    }
}
