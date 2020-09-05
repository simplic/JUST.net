using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JUST
{
    public class Utilities
    {
        public static JArray ParseOrGetEmpty(string arrayTokens)
        {
            if (string.IsNullOrWhiteSpace(arrayTokens))
                arrayTokens = "[]";

            return JArray.Parse(arrayTokens);
        }

        public static JArray GroupArray(JArray array, string groupingPropertyName, string groupedPropertyName)
        {

            Dictionary<string, JArray> groupedPair = null;

            if (array != null)
            {
                foreach (JObject eachObj in array.Children())
                {
                    JToken groupToken = eachObj.SelectToken("$." + groupingPropertyName);

                    if (groupedPair == null)
                        groupedPair = new Dictionary<string, JArray>();


                    if (groupToken != null)
                    {
                        object valueOfToken = Transformer.GetValue(groupToken, null);

                        if (groupedPair.ContainsKey(valueOfToken.ToString()))
                        {
                            JArray oldArr = groupedPair[valueOfToken.ToString()];
                            JObject clonedObj = (JObject)eachObj.DeepClone();
                            clonedObj.Remove(groupingPropertyName);

                            oldArr.Add(clonedObj);

                            groupedPair[valueOfToken.ToString()] = oldArr;
                        }
                        else
                        {
                            JObject clonedObj = (JObject)eachObj.DeepClone();

                            JArray newArr = new JArray();
                            clonedObj.Remove(groupingPropertyName);
                            newArr.Add(clonedObj);

                            groupedPair.Add(valueOfToken.ToString(), newArr);
                        }
                    }
                }
            }

            JArray resultObj = null;
            foreach (KeyValuePair<string, JArray> pair in groupedPair)
            {
                if (resultObj == null)
                    resultObj = new JArray();

                JObject groupObj = new JObject();
                groupObj.Add(groupingPropertyName, pair.Key);
                groupObj.Add(groupedPropertyName, pair.Value);

                resultObj.Add(groupObj);

            }

            return resultObj;
        }

        public static JArray GroupArrayMultipleProperties(JArray array, string[] groupingPropertyNames, string groupedPropertyName)
        {

            Dictionary<string, JArray> groupedPair = null;

            if (array != null)
            {
                foreach (JObject eachObj in array.Children())
                {
                    List<JToken> groupTokens = new List<JToken>();

                    foreach (string groupPropertyName in groupingPropertyNames)
                        groupTokens.Add(eachObj.SelectToken("$." + groupPropertyName));

                    if (groupedPair == null)
                        groupedPair = new Dictionary<string, JArray>();


                    if (groupTokens.Count > 0)
                    {
                        string key = string.Empty;

                        foreach (JToken groupToken in groupTokens)
                        {
                            object valueOfToken = Transformer.GetValue(groupToken, null);
                            if (key == string.Empty)
                                key += valueOfToken;
                            else
                                key += ":" + valueOfToken;
                        }


                        if (groupedPair.ContainsKey(key))
                        {
                            JArray oldArr = groupedPair[key];
                            JObject clonedObj = (JObject)eachObj.DeepClone();

                            foreach (string groupPropertyName in groupingPropertyNames)
                                clonedObj.Remove(groupPropertyName);

                            oldArr.Add(clonedObj);

                            groupedPair[key] = oldArr;
                        }
                        else
                        {
                            JObject clonedObj = (JObject)eachObj.DeepClone();

                            JArray newArr = new JArray();

                            foreach (string groupPropertyName in groupingPropertyNames)
                                clonedObj.Remove(groupPropertyName);
                            newArr.Add(clonedObj);

                            groupedPair.Add(key, newArr);
                        }
                    }
                }
            }

            JArray resultObj = null;
            foreach (KeyValuePair<string, JArray> pair in groupedPair)
            {
                if (resultObj == null)
                    resultObj = new JArray();

                JObject groupObj = new JObject();

                string[] keys = pair.Key.Split(':');

                int i = 0;
                foreach (string groupPropertyName in groupingPropertyNames)
                {
                    groupObj.Add(groupPropertyName, keys[i]);
                    i++;
                }

                groupObj.Add(groupedPropertyName, pair.Value);

                resultObj.Add(groupObj);

            }

            return resultObj;
        }
    }
}
