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
        public void StringConcat_Static_Test()
        {
            var input = @"{ ""firstName"": ""max"", ""lastName"": ""foo"" }";
            //var transformer = @"{ ""friendlyName"": ""~(valueOf(c, \""$.firstName\"") + valueOf(c, \""$.lastName\""))"" }";
            var transformer = @"{ ""friendlyName"": ""~(\""max\"" + \"" \"" + \""foo\"")"" }";

            var jsonTransformer = new JsonTransformer();
            var result = jsonTransformer.Transform(transformer, input);

            var obj = JObject.Parse(result);
            Assert.Equal("max foo", obj.SelectToken("$.friendlyName").Value<string>());
            Assert.True(true);
        }

        [Fact]
        public void String_ValueOf_Test()
        {
            var input = @"{ ""firstName"": ""max"" }";
            //var transformer = @"{ ""friendlyName"": ""~(valueOf(c, \""$.firstName\"") + valueOf(c, \""$.lastName\""))"" }";
            var transformer = @"{ ""friendlyName"": ""~(valueOf(\""$.firstName\""))"" }";

            var jsonTransformer = new JsonTransformer();
            var result = jsonTransformer.Transform(transformer, input);

            var obj = JObject.Parse(result);
            Assert.Equal("max", obj.SelectToken("$.friendlyName").Value<string>());
            Assert.True(true);
        }

        [Fact]
        public void String_ValueOf_Concat_Test()
        {
            var input = @"{ ""firstName"": ""max"", ""lastName"": ""foo"" }";
            var transformer = @"{ ""friendlyName"": ""~(valueOfStr(\""$.firstName\"") + valueOfStr(\""$.lastName\""))"" }";

            var jsonTransformer = new JsonTransformer();
            var result = jsonTransformer.Transform(transformer, input);

            var obj = JObject.Parse(result);
            Assert.Equal("maxfoo", obj.SelectToken("$.friendlyName").Value<string>());
            Assert.True(true);
        }

        [Fact]
        public void String_ValueOf_Add_Test()
        {
            var input = @"{ ""val1"": 1, ""val2"": ""3"" }";
            var transformer = @"{ ""val"": ""~(valueOfInt(\""$.val1\"") + valueOfInt(\""$.val2\""))"" }";

            var jsonTransformer = new JsonTransformer();
            var result = jsonTransformer.Transform(transformer, input);

            var obj = JObject.Parse(result);
            Assert.Equal("4", obj.SelectToken("$.val").Value<string>());
            Assert.True(true);
        }
    }
}
