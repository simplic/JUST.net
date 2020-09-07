using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace JUST
{
    internal static class Transformer
    {
        public static object ValueOf(string jsonPath, JToken input, bool strictPathHandling, object defaultValue)
        {
            JToken selectedToken = input.SelectToken(jsonPath);

            if (strictPathHandling)
            {
                if (selectedToken == null)
                {
                    throw new PathNotFoundException($"Could not find object at path: {jsonPath},");
                }
            }

            return GetValue(selectedToken, defaultValue);
        }

        public static object getarray(string document, string jsonPath, string inputJson)
        {
            JsonReader reader = new JsonTextReader(new StringReader(document))
            {
                DateParseHandling = DateParseHandling.None
            };
            JToken token = JObject.Load(reader);

            var jsonArrayToken = token.SelectTokens(jsonPath).ToList();
            var array = new JArray();
            foreach (var arrayToken in jsonArrayToken)
                array.Add(arrayToken);

            var arrayAsString = array.ToString();

            return arrayAsString;
        }

        public static string exists(string jsonPath, string inputJson)
        {
            JsonReader reader = new JsonTextReader(new StringReader(inputJson));
            reader.DateParseHandling = DateParseHandling.None;
            JToken token = JObject.Load(reader);

            JToken selectedToken = token.SelectToken(jsonPath);

            if (selectedToken != null)
                return "true";
            else
                return "false";
        }

        public static string existsandnotempty(string jsonPath, string inputJson)
        {
            JsonReader reader = new JsonTextReader(new StringReader(inputJson));
            reader.DateParseHandling = DateParseHandling.None;
            JToken token = JObject.Load(reader);

            JToken selectedToken = token.SelectToken(jsonPath);

            if (selectedToken != null)
            {
                if (selectedToken.ToString().Trim() != string.Empty)
                    return "true";
                else
                    return "false";
            }
            else
                return "false";
        }

        public static object ifcondition(object condition, object value, object trueResult, object falseResult, string inputJson)
        {
            object output = falseResult;

            if (condition.ToString().ToLower() == value.ToString().ToLower())
                output = trueResult;

            return output;
        }

        #region string functions

        public static string firstindexof(string stringRef, string searchString, string inputJson)
        {
            return stringRef.IndexOf(searchString, 0).ToString();
        }

        public static string lastindexof(string stringRef, string searchString, string inputJson)
        {
            return stringRef.LastIndexOf(searchString).ToString();
        }

        public static string listall(string array, string separator, string inputJson)
        {
            string result = null;

            JArray parsedArray = Utilities.ParseOrGetEmpty(array);

            if (parsedArray != null)
            {
                var isFirstItem = true;

                foreach (JToken token in parsedArray.Children())
                {
                    if (result == null)
                        result = string.Empty;

                    if (!isFirstItem && !string.IsNullOrWhiteSpace(result))
                    {
                        result += separator;
                    }

                    isFirstItem = false;

                    result += token.ToString();
                }
            }

            return result;
        }

        public static string listallatpath(string array, string jsonPath, string separator, string inputJson)
        {
            string result = null;

            JArray parsedArray = Utilities.ParseOrGetEmpty(array);

            if (parsedArray != null)
            {
                var isFirstItem = true;
                foreach (JToken token in parsedArray.Children())
                {
                    JToken selectedToken = token.SelectToken(jsonPath);

                    if (selectedToken == null)
                        continue;

                    if (result == null)
                        result = string.Empty;

                    if (!isFirstItem && !string.IsNullOrWhiteSpace(result))
                    {
                        result += separator;
                    }

                    isFirstItem = false;

                    result += selectedToken.ToString();
                }
            }

            return result;
        }
        #endregion

        #region aggregate functions
        public static object count(string array, string inputJson)
        {
            try
            {
                JArray parsedArray = Utilities.ParseOrGetEmpty(array);
                return parsedArray.Count;
            }
            catch { return null; }
        }

        public static string sum(string array, string inputJson)
        {
            try
            {
                JArray parsedArray = Utilities.ParseOrGetEmpty(array);

                double integerresult = 0;

                if (parsedArray != null)
                {
                    foreach (JToken token in parsedArray.Children())
                    {

                        integerresult += Convert.ToDouble(token.ToString());
                    }
                }

                return integerresult.ToString();
            }
            catch { return null; }
        }

        public static string sumatpath(string array, string jsonPath, string inputJson)
        {
            try
            {
                double integerresult = 0;

                JArray parsedArray = Utilities.ParseOrGetEmpty(array);

                if (parsedArray != null)
                {

                    foreach (JToken token in parsedArray.Children())
                    {

                        JToken selectedToken = token.SelectToken(jsonPath);


                        integerresult += Convert.ToDouble(selectedToken.ToString());
                    }
                }

                return integerresult.ToString();
            }
            catch { return null; }
        }

        public static string firstatpath(string array, string jsonPath, string inputJson)
        {
            try
            {
                JArray parsedArray = Utilities.ParseOrGetEmpty(array);

                if (parsedArray != null)
                {

                    foreach (JToken token in parsedArray.Children())
                    {

                        JToken selectedToken = token.SelectToken(jsonPath);

                        if (selectedToken != null)
                            return selectedToken?.ToString();
                    }
                }

                return null;
            }
            catch { return null; }
        }

        public static string average(string array, string inputJson)
        {
            try
            {
                JArray parsedArray = Utilities.ParseOrGetEmpty(array);

                double integerresult = 0;

                if (parsedArray != null)
                {
                    foreach (JToken token in parsedArray.Children())
                    {

                        integerresult += Convert.ToDouble(token.ToString());
                    }
                }

                return ((double)integerresult / (double)parsedArray.Count).ToString();
            }
            catch { return null; }
        }

        public static string averageatpath(string array, string jsonPath, string inputJson)
        {
            try
            {
                double integerresult = 0;

                JArray parsedArray = Utilities.ParseOrGetEmpty(array);

                if (parsedArray != null)
                {

                    foreach (JToken token in parsedArray.Children())
                    {

                        JToken selectedToken = token.SelectToken(jsonPath);


                        integerresult += Convert.ToDouble(selectedToken.ToString());
                    }
                }

                return ((double)integerresult / (double)parsedArray.Count).ToString();
            }
            catch { return null; }
        }

        public static string max(string array, string inputJson)
        {
            try
            {
                JArray parsedArray = Utilities.ParseOrGetEmpty(array);

                double integerresult = 0;
                int i = 0;
                if (parsedArray != null)
                {
                    foreach (JToken token in parsedArray.Children())
                    {

                        double thisValue = Convert.ToDouble(token.ToString());

                        if (i == 0)
                            integerresult = thisValue;
                        else
                        {
                            if (integerresult < thisValue)
                                integerresult = thisValue;
                        }

                        i++;
                    }
                }

                return integerresult.ToString();
            }
            catch { return null; }
        }

        public static string maxatpath(string array, string jsonPath, string inputJson)
        {
            try
            {
                double integerresult = 0;
                int i = 0;

                JArray parsedArray = Utilities.ParseOrGetEmpty(array);

                if (parsedArray != null)
                {

                    foreach (JToken token in parsedArray.Children())
                    {

                        JToken selectedToken = token.SelectToken(jsonPath);


                        double thisValue = Convert.ToDouble(selectedToken.ToString());

                        if (i == 0)
                            integerresult = thisValue;
                        else
                        {
                            if (integerresult < thisValue)
                                integerresult = thisValue;
                        }

                        i++;
                    }
                }

                return integerresult.ToString();
            }
            catch
            {
                return null;
            }
        }


        public static string min(string array, string inputJson)
        {
            try
            {
                JArray parsedArray = Utilities.ParseOrGetEmpty(array);

                double integerresult = 0;
                int i = 0;
                if (parsedArray != null)
                {
                    foreach (JToken token in parsedArray.Children())
                    {

                        double thisValue = Convert.ToDouble(token.ToString());

                        if (i == 0)
                            integerresult = thisValue;
                        else
                        {
                            if (integerresult > thisValue)
                                integerresult = thisValue;
                        }

                        i++;
                    }
                }

                return integerresult.ToString();
            }
            catch { return null; }
        }

        public static string minatpath(string array, string jsonPath, string inputJson)
        {
            try
            {
                double integerresult = 0;
                int i = 0;

                JArray parsedArray = Utilities.ParseOrGetEmpty(array);

                if (parsedArray != null)
                {

                    foreach (JToken token in parsedArray.Children())
                    {

                        JToken selectedToken = token.SelectToken(jsonPath);


                        double thisValue = Convert.ToDouble(selectedToken.ToString());

                        if (i == 0)
                            integerresult = thisValue;
                        else
                        {
                            if (integerresult > thisValue)
                                integerresult = thisValue;
                        }

                        i++;
                    }
                }

                return integerresult.ToString();
            }
            catch { return null; }
        }

        public static string arraylength(string array, string inputJson)
        {
            try
            {
                JArray parsedArray = Utilities.ParseOrGetEmpty(array);


                return parsedArray.Count.ToString();
            }
            catch { return null; }
        }

        #endregion

        public static object GetValue(JToken selectedToken, object defaultValue)
        {
            object output = defaultValue;
            if (selectedToken != null)
            {
                if (selectedToken.Type == JTokenType.Date)
                {
                    DateTime value = Convert.ToDateTime(selectedToken.Value<DateTime>());

                    output = value.ToString("yyyy-MM-ddTHH:mm:sszzzz");
                }
                else
                    output = selectedToken.ToString();

                if (selectedToken.Type == JTokenType.Object)
                {
                    output = JsonConvert.SerializeObject(selectedToken);
                }
                if (selectedToken.Type == JTokenType.Boolean)
                {
                    output = selectedToken.ToObject<bool>();
                }
                if (selectedToken.Type == JTokenType.Integer)
                {
                    output = selectedToken.ToObject<Int64>();
                }
                if (selectedToken.Type == JTokenType.Float)
                {
                    output = selectedToken.ToObject<float>();
                }

            }
            return output;
        }

        #region grouparrayby
        public static object grouparrayby(string jsonPath, string groupingElement, string groupedElement, string inputJson)
        {
            if (!groupingElement.Contains(":"))
            {

                JObject inObj = JObject.Parse(inputJson);

                JArray arr = (JArray)inObj.SelectToken(jsonPath);

                JArray result = Utilities.GroupArray(arr, groupingElement, groupedElement);

                return JsonConvert.SerializeObject(result);
            }
            else
            {
                string[] groupingElements = groupingElement.Split(':');

                JObject inObj = JObject.Parse(inputJson);

                JArray arr = (JArray)inObj.SelectToken(jsonPath);

                JArray result = Utilities.GroupArrayMultipleProperties(arr, groupingElements, groupedElement);

                return JsonConvert.SerializeObject(result);
            }
        }

        #endregion
    }
}
