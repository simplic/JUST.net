using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace JUST.Net.Test
{
    public class PathNotExistsTest
    {
        [Fact]
        public void PathNotExists_Test_Str()
        {
            var input = @"{  }";

            var transformer = @"{ ""val"": ""~(valueOfStr(\""$.test.test.test\""))"" }";

            var jsonTransformer = new JsonTransformer();
            var result = jsonTransformer.Transform(transformer, input);

            var obj = JObject.Parse(result);
            Assert.Equal("", obj.SelectToken("$.val").Value<string>());
            Assert.True(true);
        }

        [Fact]
        public void PathNotExists_Test_Int()
        {
            var input = @"{  }";

            var transformer = @"{ ""val"": ""~(valueOfInt(\""$.test.test.test\""))"" }";

            var jsonTransformer = new JsonTransformer();
            var result = jsonTransformer.Transform(transformer, input);

            var obj = JObject.Parse(result);
            Assert.Equal(0, obj.SelectToken("$.val").Value<int>());
            Assert.True(true);
        }

        [Fact]
        public void PathNotExists_Test_Double()
        {
            var input = @"{  }";

            var transformer = @"{ ""val"": ""~(valueOfDouble(\""$.test.test.test\""))"" }";

            var jsonTransformer = new JsonTransformer();
            var result = jsonTransformer.Transform(transformer, input);

            var obj = JObject.Parse(result);
            Assert.Equal(0.0, obj.SelectToken("$.val").Value<double>());
            Assert.True(true);
        }

        [Fact]
        public void PathNotExists_Test_Str_Exception()
        {
            var input = @"{  }";

            var transformer = @"{ ""val"": ""~(valueOfStr(\""$.test.test.test\""))"" }";

            var jsonTransformer = new JsonTransformer
            {
                StrictPathHandling = true
            };

            var exceptionCaused = false;

            try
            {
                jsonTransformer.Transform(transformer, input);
            }
            catch (PathNotFoundException ex)
            {
                exceptionCaused = true;
                Assert.Contains("$.test.test.test", ex.Message);
            }

            Assert.True(exceptionCaused);
        }
    }
}
