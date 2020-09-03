using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JUST.net
{
    public static class JsonUtilities
    {
        public static IEnumerable<string> SplitJson(string input, string arrayPath)
        {
            JObject inputJObject = JObject.Parse(input);

            List<JObject> jObjects = SplitJson(inputJObject, arrayPath).ToList<JObject>();

            List<string> output = null;

            foreach (JObject jObject in jObjects)
            {
                if (output == null)
                    output = new List<string>();

                output.Add(JsonConvert.SerializeObject(jObject));
            }

            return output;
        }

        public static IEnumerable<JObject> SplitJson(JObject input, string arrayPath)
        {
            List<JObject> jsonObjects = null;

            JToken tokenArr = input.SelectToken(arrayPath);

            string pathToReplace = tokenArr.Path;

            if (tokenArr != null && tokenArr is JArray)
            {
                JArray array = tokenArr as JArray;

                foreach (JToken tokenInd in array)
                {

                    string path = tokenInd.Path;

                    JToken clonedToken = input.DeepClone();

                    JToken foundToken = clonedToken.SelectToken("$." + path);
                    JToken tokenToReplcae = clonedToken.SelectToken("$." + pathToReplace);

                    tokenToReplcae.Replace(foundToken);

                    if (jsonObjects == null)
                        jsonObjects = new List<JObject>();

                    jsonObjects.Add(clonedToken as JObject);


                }
            }
            else
                throw new Exception("ArrayPath must be a valid JSON path to a JSON array.");

            return jsonObjects;
        }
    }
}
