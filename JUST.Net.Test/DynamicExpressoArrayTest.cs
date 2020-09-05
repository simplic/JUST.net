using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace JUST.Net.Test
{
    public class DynamicExpressoArrayTest
    {
        [Fact]
        public void Array_ValueOf_Object_Test()
        {
            var input = @"{ ""array"": [ { ""name"": ""hans"" }, { ""name"": ""simplic"" } ] }";

            var transformer = @"{ ""ar"": { ""#loop($.array)"": { ""n"": ""~(valueOfIterStr(\""$.name\""))"" } } }";

            var jsonTransformer = new JsonTransformer();
            var result = jsonTransformer.Transform(transformer, input);

            var obj = JObject.Parse(result);
            Assert.Equal("hans", obj.SelectToken("$.ar[0].n").Value<string>());
            Assert.Equal("simplic", obj.SelectToken("$.ar[1].n").Value<string>());
            Assert.True(true);
        }
    }
}
