using Newtonsoft.Json.Linq;
using System;
using Xunit;

namespace JUST.Net.Test
{
    public class JsonStaticValueTest
    {
        [Fact]
        public void Converter_ValueOf_SimpleTest()
        {
            var input = @"{ ""name"": ""max"" }";
            var transformer = @"{ ""firstName"": ""#valueof($.name)"" }";

            var result = JsonTransformer.Transform(transformer, input);

            var obj = JObject.Parse(result);
            Assert.Equal("max", obj.SelectToken("$.firstName").Value<string>());
        }

        [Fact]
        public void Converter_Static_SimpleTest()
        {
            var input = @"{ ""name"": ""max"" }";
            var transformer = @"{ ""firstName"": ""#valueof($.name)"", ""lastName"": ""max2"" }";

            var result = JsonTransformer.Transform(transformer, input);

            var obj = JObject.Parse(result);
            Assert.Equal("max", obj.SelectToken("$.firstName").Value<string>());
            Assert.Equal("max2", obj.SelectToken("$.lastName").Value<string>());
        }
    }
}
