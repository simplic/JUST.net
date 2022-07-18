using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace JUST.Net.Test
{
    public class DynamicExpressionRegexTest
    {
        [Fact]
        public void SimpleNumber_Test()
        {
            var input = @"{ }";
            var transformer = @"{ ""rg"": ""~(regex(\""V-12345\"", \""@@\\d+\"", \""0\""))"" }";

            var jsonTransformer = new JsonTransformer();
            var result = jsonTransformer.Transform(transformer, input);

            var obj = JObject.Parse(result);
            Assert.Equal("12345", obj.SelectToken("$.rg").Value<string>());
        }

        [Fact]
        public void SimpleNumber_Test_Defaukt()
        {
            var input = @"{ }";
            var transformer = @"{ ""rg"": ""~(regex(\""VHHG\"", \""d+\"", \""111\""))"" }";

            var jsonTransformer = new JsonTransformer();
            var result = jsonTransformer.Transform(transformer, input);

            var obj = JObject.Parse(result);
            Assert.Equal("111", obj.SelectToken("$.rg").Value<string>());
        }
    }
}
