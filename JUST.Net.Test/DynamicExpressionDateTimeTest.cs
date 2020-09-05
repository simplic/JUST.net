using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace JUST.Net.Test
{
    public class DynamicExpressionDateTimeTest
    {
        [Fact]
        public void DateTime_Test()
        {
            var input = @"{ }";
            var transformer = @"{ ""dt"": ""~(DateTime.Now.Date)"" }";

            var jsonTransformer = new JsonTransformer();
            var result = jsonTransformer.Transform(transformer, input);

            var obj = JObject.Parse(result);
            Assert.Equal(DateTime.Now.Date, obj.SelectToken("$.dt").Value<DateTime>());
            Assert.True(true);
        }

        [Fact]
        public void DateTime_Format_Test()
        {
            var input = @"{ }";
            var transformer = @"{ ""dt"": ""~(DateTime.Now.ToString(\""ddMMyyyy\""))"" }";

            var jsonTransformer = new JsonTransformer();
            var result = jsonTransformer.Transform(transformer, input);

            var obj = JObject.Parse(result);
            Assert.Equal(DateTime.Now.ToString("ddMMyyyy"), obj.SelectToken("$.dt").Value<string>());
            Assert.True(true);
        }
    }
}
