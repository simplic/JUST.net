using JUST.net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace JUST
{
    public class JsonTransformer
    {
        #region Fields
        private const string DefaultTransformerNamespace = "JUST.Transformer";
        private readonly ExpressionInterpreter expressionInterpreter;
        private JToken inputObject;
        #endregion

        #region Constructor
        /// <summary>
        /// Initialize json transformer
        /// </summary>
        public JsonTransformer()
        {
            expressionInterpreter = new ExpressionInterpreter();
        }
        #endregion

        #region [Transform]
        /// <summary>
        /// Transform json by json as string
        /// </summary>
        /// <param name="transformerJson">Transformer as .net string</param>
        /// <param name="inputJson">Input as .net string (json)</param>
        /// <returns></returns>
        public string Transform(string transformerJson, string inputJson)
        {
            JToken transformerToken = JToken.Parse(transformerJson);
            JToken result;
            switch (transformerToken.Type)
            {
                case JTokenType.Object:
                    result = Transform(transformerToken as JObject, inputJson);
                    break;
                case JTokenType.Array:
                    result = Transform(transformerToken as JArray, inputJson);
                    break;
                default:
                    throw new NotSupportedException($"Transformer of type '{transformerToken.Type}' not supported!");
            }

            string output = JsonConvert.SerializeObject(result, Formatting.Indented);

            return output;
        }

        /// <summary>
        /// Transform json array (JArray)
        /// </summary>
        /// <param name="transformerArray">Transformer as array</param>
        /// <param name="inputJson">Input as .net string (json)</param>
        /// <returns>Transformed json</returns>
        public JArray Transform(JArray transformerArray, string input)
        {
            var result = new JArray();
            foreach (var transformer in transformerArray)
            {
                if (transformer.Type != JTokenType.Object)
                {
                    throw new NotSupportedException($"Transformer of type '{transformer.Type}' not supported!");
                }
                Transform(transformer as JObject, input);
                result.Add(transformer);
            }
            return result;
        }

        /// <summary>
        /// Transform JObject by using a JObject transformer
        /// </summary>
        /// <param name="transformer">Transformer as NewtonsoftJson object</param>
        /// <param name="input">Input as Newtonsoft.Json object</param>
        /// <returns>Transformed json as JObject</returns>
        public JObject Transform(JObject transformer, JObject input)
        {
            string inputJson = JsonConvert.SerializeObject(input);
            return Transform(transformer, inputJson);
        }

        /// <summary>
        /// Transform json by using a JObject
        /// </summary>
        /// <param name="transformer">Transformer as JObject</param>
        /// <param name="input">Json to transform</param>
        /// <returns>Transformed JObject</returns>
        public JObject Transform(JObject transformer, string input)
        {
            JsonReader reader = new JsonTextReader(new StringReader(input));
            reader.DateParseHandling = DateParseHandling.None;
            inputObject = JObject.Load(reader);

            expressionInterpreter.Setup(inputObject, StrictPathHandling);

            RecursiveEvaluate(transformer, input, null, null);
            return transformer;
        }
        #endregion

        #region RecursiveEvaluate
        private void RecursiveEvaluate(JToken parentToken, string inputJson, JArray parentArray, JToken currentArrayToken)
        {
            if (parentToken == null)
                return;

            JEnumerable<JToken> tokens = parentToken.Children();

            List<JToken> selectedTokens = null;
            Dictionary<string, JToken> tokensToReplace = null;
            List<JToken> tokensToDelete = null;

            List<string> loopProperties = null;
            JArray arrayToForm = null;
            List<JToken> tokenToForm = null;
            List<JToken> tokensToAdd = null;

            bool isLoop = false;

            foreach (JToken childToken in tokens)
            {
                if (childToken.Type == JTokenType.Array && (parentToken as JProperty).Name.Trim() != "#")
                {
                    JArray arrayToken = childToken as JArray;

                    List<object> itemsToAdd = new List<object>();

                    foreach (JToken arrEl in childToken.Children())
                    {
                        object itemToAdd = arrEl.Value<JToken>();

                        if (arrEl.Type == JTokenType.String && arrEl.ToString().Trim().StartsWith("#"))
                        {
                            object value = ParseFunction(arrEl.ToString(), inputJson, parentArray, currentArrayToken);
                            itemToAdd = value;
                        }

                        itemsToAdd.Add(itemToAdd);
                    }

                    arrayToken.RemoveAll();

                    foreach (object itemToAdd in itemsToAdd)
                    {
                        arrayToken.Add(itemToAdd);
                    }
                }

                if (childToken.Type == JTokenType.Property)
                {
                    JProperty property = childToken as JProperty;

                    if (property.Name != null && property.Name == "#" && property.Value.Type == JTokenType.Array)
                    {

                        JArray values = property.Value as JArray;

                        JEnumerable<JToken> arrayValues = values.Children();

                        foreach (JToken arrayValue in arrayValues)
                        {
                            if (arrayValue.Type == JTokenType.String && arrayValue.Value<string>().Trim().StartsWith("#copy"))
                            {
                                if (selectedTokens == null)
                                    selectedTokens = new List<JToken>();

                                selectedTokens.Add(Copy(arrayValue.Value<string>(), inputJson));


                            }

                            if (arrayValue.Type == JTokenType.String && arrayValue.Value<string>().Trim().StartsWith("#replace"))
                            {
                                if (tokensToReplace == null)
                                    tokensToReplace = new Dictionary<string, JToken>();
                                string value = arrayValue.Value<string>();

                                tokensToReplace.Add(GetTokenStringToReplace(value), Replace(value, inputJson));


                            }

                            if (arrayValue.Type == JTokenType.String && arrayValue.Value<string>().Trim().StartsWith("#delete"))
                            {
                                if (tokensToDelete == null)
                                    tokensToDelete = new List<JToken>();

                                tokensToDelete.Add(Delete(arrayValue.Value<string>()));
                            }
                        }
                    }

                    if (ExpressionParserUtilities.IsExpression(property.Value.ToString().Trim(), out string expression))
                    {
                        expressionInterpreter.SetContext(currentArrayToken, parentArray, expression);

                        try
                        {
                            expression = expression.Replace("@@\\", @"\\");

                            var result = expressionInterpreter.Eval(expression);
                            property.Value = new JValue(result);
                        }
                        catch (PathNotFoundException ex)
                        {
                            throw;
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"Could not execute expression: {expression}. Property: {property}.", ex);
                        }
                    }

                    // TODO: Not required anymore
                    // if (property.Name != null && property.Value.ToString().Trim().StartsWith("#")
                    //     && !property.Name.Contains("#eval") && !property.Name.Contains("#ifgroup")
                    //     && !property.Name.Contains("#loop"))
                    // {
                    //     object newValue = ParseFunction(property.Value.ToString(), inputJson, parentArray, currentArrayToken);
                    // 
                    //     if (newValue != null && newValue.ToString().Contains("\""))
                    //     {
                    //         try
                    //         {
                    //             JToken newToken = JToken.Parse(newValue.ToString());
                    //             property.Value = newToken;
                    //         }
                    //         catch
                    //         {
                    //             property.Value = new JValue(newValue);
                    //         }
                    //     }
                    //     else
                    //         property.Value = new JValue(newValue);
                    // }

                    /* For looping*/
                    isLoop = false;

                    if (property.Name != null && property.Name.Contains("#eval"))
                    {
                        int startIndex = property.Name.IndexOf("(");
                        int endIndex = property.Name.LastIndexOf(")");

                        string functionString = property.Name.Substring(startIndex + 1, endIndex - startIndex - 1);

                        object functionResult = ParseFunction(functionString, inputJson, null, null);

                        JProperty clonedProperty = new JProperty(functionResult.ToString(), property.Value);

                        if (loopProperties == null)
                            loopProperties = new List<string>();

                        loopProperties.Add(property.Name);

                        if (tokensToAdd == null)
                        {
                            tokensToAdd = new List<JToken>
                            {
                                clonedProperty
                            };
                        }
                    }

                    if (property.Name != null && property.Name.Contains("#ifgroup"))
                    {
                        int startIndex = property.Name.IndexOf("(");
                        int endIndex = property.Name.LastIndexOf(")");

                        string functionString = property.Name.Substring(startIndex + 1, endIndex - startIndex - 1);
                        bool result = false;

                        if (ExpressionParserUtilities.IsExpression(functionString, out string exp))
                        {
                            try
                            {
                                exp = exp.Replace("@@\\", @"\\");

                                try
                                {
                                    result = (bool)expressionInterpreter.Eval(exp);
                                }
                                catch (Exception ex)
                                {
                                    throw new Exception("#ifgroup expression can only handle boolean data types. E.g. ~(1 == 1)", ex);
                                }
                            }
                            catch (PathNotFoundException ex)
                            {
                                throw;
                            }
                            catch (Exception ex)
                            {
                                throw new Exception($"Could not execute expression: {expression}. Property: {property}.", ex);
                            }
                        }
                        else
                        {
                            object functionResult = ParseFunction(functionString, inputJson, null, null);

                            try
                            {
                                result = Convert.ToBoolean(functionResult);
                            }
                            catch
                            {
                                result = false;
                            }
                        }

                        if (result == true)
                        {
                            if (loopProperties == null)
                                loopProperties = new List<string>();

                            loopProperties.Add(property.Name);

                            RecursiveEvaluate(childToken, inputJson, parentArray, currentArrayToken);

                            if (tokenToForm == null)
                            {
                                tokenToForm = new List<JToken>();
                            }

                            foreach (JToken grandChildToken in childToken.Children())
                                tokenToForm.Add(grandChildToken.DeepClone());
                        }
                        else
                        {
                            if (loopProperties == null)
                                loopProperties = new List<string>();

                            loopProperties.Add(property.Name);
                        }

                        isLoop = true;
                    }

                    if (property.Name != null && property.Name.Contains("#loop"))
                    {
                        string strArrayToken = property.Name.Substring(6, property.Name.Length - 7);

                        JsonReader reader = null;
                        if (currentArrayToken != null && property.Name.Contains("#loopwithincontext"))
                        {
                            strArrayToken = property.Name.Substring(19, property.Name.Length - 20);
                            reader = new JsonTextReader(new StringReader(JsonConvert.SerializeObject(currentArrayToken)));
                        }
                        else
                            reader = new JsonTextReader(new StringReader(inputJson));
                        reader.DateParseHandling = DateParseHandling.None;
                        JToken token = JObject.Load(reader);
                        JToken arrayToken = null;

                        if (strArrayToken.Contains("#") && !strArrayToken.Trim().StartsWith("#"))
                        {
                            int sIndex = strArrayToken.IndexOf("#");
                            string sub1 = strArrayToken.Substring(0, sIndex);

                            int indexOfEndFunction = GetIndexOfFunctionEnd(strArrayToken);

                            if (indexOfEndFunction > sIndex && sIndex > 0)
                            {
                                string sub2 = strArrayToken.Substring(indexOfEndFunction + 1, strArrayToken.Length - indexOfEndFunction - 1);

                                string functionResult = ParseFunction(strArrayToken.Substring(sIndex, indexOfEndFunction - sIndex + 1), inputJson, parentArray, currentArrayToken).ToString();

                                strArrayToken = sub1 + functionResult + sub2;
                            }
                        }

                        if (strArrayToken.Trim().StartsWith("#"))
                        {
                            arrayToken = ParseFunction(strArrayToken, inputJson, parentArray, currentArrayToken) as JArray;
                        }

                        if (arrayToken == null)
                        {
                            try
                            {
                                arrayToken = token.SelectToken(strArrayToken);

                                if (arrayToken is JObject)
                                {
                                    arrayToken = new JArray(arrayToken);
                                }
                            }
                            catch
                            {
                                var multipleTokens = token.SelectTokens(strArrayToken);

                                arrayToken = new JArray(multipleTokens);
                            }
                        }

                        if (arrayToken == null)
                        {
                            arrayToForm = new JArray();
                        }
                        else
                        {

                            JArray array = (JArray)arrayToken;

                            IEnumerator<JToken> elements = array.GetEnumerator();



                            while (elements.MoveNext())
                            {
                                if (arrayToForm == null)
                                    arrayToForm = new JArray();

                                JToken clonedToken = childToken.DeepClone();

                                RecursiveEvaluate(clonedToken, inputJson, array, elements.Current);

                                foreach (JToken replacedProperty in clonedToken.Children())
                                {
                                    arrayToForm.Add(replacedProperty);
                                }


                            }
                        }
                        if (loopProperties == null)
                            loopProperties = new List<string>();

                        loopProperties.Add(property.Name);
                        isLoop = true;
                    }
                    /*End looping */
                }

                if (childToken.Type == JTokenType.String && childToken.Value<string>().Trim().StartsWith("#")
                    && parentArray != null && currentArrayToken != null)
                {
                    object newValue = ParseFunction(childToken.Value<string>(), inputJson, parentArray, currentArrayToken);

                    if (newValue != null && newValue.ToString().Contains("\""))
                    {
                        try
                        {
                            JToken newToken = JToken.Parse(newValue.ToString());
                            childToken.Replace(new JValue(newValue));
                        }
                        catch
                        {

                        }
                    }
                    else
                        childToken.Replace(new JValue(newValue));
                }

                if (!isLoop)
                    RecursiveEvaluate(childToken, inputJson, parentArray, currentArrayToken);

            }


            if (selectedTokens != null)
            {
                foreach (JToken selectedToken in selectedTokens)
                {
                    if (selectedToken != null)
                    {
                        JEnumerable<JToken> copyChildren = selectedToken.Children();

                        foreach (JToken copyChild in copyChildren)
                        {
                            JProperty property = copyChild as JProperty;

                            (parentToken as JObject).Add(property.Name, property.Value);
                        }
                    }
                }
            }

            if (tokensToReplace != null)
            {
                foreach (KeyValuePair<string, JToken> tokenToReplace in tokensToReplace)
                {
                    JToken selectedToken = (parentToken as JObject).SelectToken(tokenToReplace.Key);

                    if (selectedToken != null && selectedToken is JObject)
                    {
                        JObject selectedObject = selectedToken as JObject;
                        selectedObject.RemoveAll();


                        JEnumerable<JToken> copyChildren = tokenToReplace.Value.Children();

                        foreach (JToken copyChild in copyChildren)
                        {
                            JProperty property = copyChild as JProperty;

                            selectedObject.Add(property.Name, property.Value);
                        }
                    }
                    if (selectedToken != null && selectedToken is JValue)
                    {
                        JValue selectedObject = selectedToken as JValue;

                        selectedObject.Value = tokenToReplace.Value.ToString();
                    }
                }
            }

            if (tokensToDelete != null)
            {
                foreach (string selectedToken in tokensToDelete)
                {
                    JToken tokenToRemove = parentToken.SelectToken(selectedToken);

                    if (tokenToRemove != null)
                        tokenToRemove.Ancestors().First().Remove();

                }
            }
            if (tokensToAdd != null)
            {
                foreach (JToken token in tokensToAdd)
                {
                    (parentToken as JObject).Add((token as JProperty).Name, (token as JProperty).Value);
                }
            }
            if (tokenToForm != null)
            {
                foreach (JToken token in tokenToForm)
                {
                    foreach (JProperty childToken in token.Children())
                        (parentToken as JObject).Add(childToken.Name, childToken.Value);
                }
            }
            if (parentToken is JObject)
            {
                (parentToken as JObject).Remove("#");
            }

            if (loopProperties != null)
            {
                foreach (string propertyToDelete in loopProperties)
                    (parentToken as JObject).Remove(propertyToDelete);
            }
            if (arrayToForm != null)
            {
                parentToken.Replace(arrayToForm);
            }

        }
        #endregion

        #region Copy
        private JToken Copy(string inputString, string inputJson)
        {
            int indexOfStart = inputString.IndexOf("(", 0);
            int indexOfEnd = inputString.LastIndexOf(")");

            string jsonPath = inputString.Substring(indexOfStart + 1, indexOfEnd - indexOfStart - 1);

            JToken token = JObject.Parse(inputJson);

            JToken selectedToken = token.SelectToken(jsonPath);

            return selectedToken;


        }

        #endregion

        #region Delete
        private string Delete(string inputString)
        {
            int indexOfStart = inputString.IndexOf("(", 0);
            int indexOfEnd = inputString.LastIndexOf(")");

            string path = inputString.Substring(indexOfStart + 1, indexOfEnd - indexOfStart - 1);


            return path;


        }

        #endregion

        #region Replace
        private JToken Replace(string inputString, string inputJson)
        {
            int indexOfStart = inputString.IndexOf("(", 0);
            int indexOfEnd = inputString.LastIndexOf(")");

            string argumentString = inputString.Substring(indexOfStart + 1, indexOfEnd - indexOfStart - 1);

            string[] arguments = argumentString.Split(',');

            if (arguments == null || arguments.Length != 2)
                throw new Exception("#replace needs exactly two arguments - 1. xpath to be replaced, 2. token to replace with.");

            JToken newToken = null;
            object str = ParseFunction(arguments[1], inputJson, null, null);
            if (str != null && str.ToString().Contains("\""))
            {
                newToken = JToken.Parse(str.ToString());

            }
            else
                newToken = str.ToString();

            return newToken;

        }

        private string GetTokenStringToReplace(string inputString)
        {
            int indexOfStart = inputString.IndexOf("(", 0);
            int indexOfEnd = inputString.LastIndexOf(")");

            string argumentString = inputString.Substring(indexOfStart + 1, indexOfEnd - indexOfStart - 1);

            string[] arguments = argumentString.Split(',');

            if (arguments == null || arguments.Length != 2)
                throw new Exception("#replace needs exactly two arguments - 1. xpath to be replaced, 2. token to replace with.");
            return arguments[0];

        }

        #endregion

        #region ParseFunction

        private object ParseFunction(string functionString, string inputJson, JArray array, JToken currentArrayElement)
        {
            try
            {
                object output = null;
                functionString = functionString.Trim();
                output = functionString.Substring(1);

                int indexOfStart = output.ToString().IndexOf("(", 0);
                int indexOfEnd = output.ToString().LastIndexOf(")");

                if (indexOfStart == -1 || indexOfEnd == -1)
                    return functionString;

                string functionName = output.ToString().Substring(0, indexOfStart);

                string argumentString = output.ToString().Substring(indexOfStart + 1, indexOfEnd - indexOfStart - 1);

                string[] arguments = GetArguments(argumentString);
                object[] parameters = new object[arguments.Length + 1];

                int i = 0;
                if (arguments != null && arguments.Length > 0)
                {
                    foreach (string argument in arguments)
                    {
                        string trimmedArgument = argument;

                        // Missing contains here!
                        if (argument.Contains("#"))
                            trimmedArgument = argument.Trim();

                        if (trimmedArgument.StartsWith("#"))
                        {
                            parameters[i] = ParseFunction(trimmedArgument, inputJson, array, currentArrayElement);
                        }
                        else
                            parameters[i] = trimmedArgument;
                        i++;
                    }

                }

                parameters[i] = inputJson;

                if (functionName == "getroot")
                    output = inputJson;
                else if (functionName == "getparent")
                {
                    if (currentArrayElement == null)
                        throw new Exception("getparent is only allowed inside loop.");

                    output = currentArrayElement.Parent.ToString();
                }
                else if (functionName == "createarray")
                {
                    var newArray = new JArray();

                    var arrayCount = int.Parse(parameters[0].ToString());
                    var arrayItemName = parameters[1].ToString();
                    var makeArrayValueParameters = parameters.Skip(2).ToList();

                    for (int elementId = 0; elementId < arrayCount; elementId++)
                    {
                        var arrayItem = new JObject();

                        string lastParameterName = null;
                        for (int parameterId = 0; parameterId < makeArrayValueParameters.Count; parameterId++)
                        {
                            if (parameterId % 2 == 0)
                                lastParameterName = makeArrayValueParameters[parameterId].ToString();
                            if (parameterId % 2 != 0)
                            {
                                arrayItem.Add(new JProperty(lastParameterName, makeArrayValueParameters[parameterId]));

                                lastParameterName = "";
                            }
                        }

                        if (lastParameterName != "")
                            arrayItem.Add(new JProperty(lastParameterName, null));

                        newArray.Add(arrayItem);
                    }

                    output = newArray;
                }
                // TODO: Remove
                ///else if (functionName == "currentvalue" || functionName == "currentindex" || functionName == "lastindex"
                ///    || functionName == "lastvalue")
                ///    output = ReflectionHelper.InvokeFunction(null, "JUST.Transformer", functionName, new object[] { array, currentArrayElement });
                ///else if (functionName == "currentvalueatpath" || functionName == "lastvalueatpath")
                ///    output = ReflectionHelper.InvokeFunction(null, "JUST.Transformer", functionName, new object[] { array, currentArrayElement, arguments[0] });
                ///else if (functionName == "customfunction")
                ///    output = CallCustomFunction(parameters);
                ///else
                ///{
                ///    if (currentArrayElement != null && functionName != "valueof")
                ///    {
                ///        parameters[i] = JsonConvert.SerializeObject(currentArrayElement);
                ///    }
                ///    output = ReflectionHelper.InvokeFunction(null, DefaultTransformerNamespace, functionName, parameters);
                ///}

                return output;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while calling function : " + functionString + " - " + ex.Message, ex);
            }
        }
        #endregion

        #region GetArguments
        private string[] GetArguments(string rawArgument)
        {
            if (rawArgument.Trim() == "")
                return new string[] { };

            bool brackettOpen = false;

            List<string> arguments = null;
            int index = 0;

            int openBrackettCount = 0;
            int closebrackettCount = 0;
            for (int i = 0; i < rawArgument.Length; i++)
            {
                string currentArgument;
                if (index != 0)
                    currentArgument = rawArgument.Substring(index + 1, i - index - 1);
                else
                    currentArgument = rawArgument.Substring(index, i);

                char currentChar = rawArgument[i];

                if (currentArgument.Trim().StartsWith("#"))
                {
                    if (currentChar == '(')
                        openBrackettCount++;

                    if (currentChar == ')')
                        closebrackettCount++;
                }

                if (openBrackettCount == closebrackettCount)
                    brackettOpen = false;
                else
                    brackettOpen = true;

                if ((currentChar == ',') && (!brackettOpen))
                {
                    if (arguments == null)
                        arguments = new List<string>();

                    arguments.Add(currentArgument);

                    index = i;
                }

            }

            if (index > 0)
            {
                arguments.Add(rawArgument.Substring(index + 1, rawArgument.Length - index - 1));
            }
            else
            {
                if (arguments == null)
                    arguments = new List<string>();
                arguments.Add(rawArgument);
            }

            return arguments.ToArray();
        }
        #endregion

        private int GetIndexOfFunctionEnd(string totalString)
        {
            int index = -1;

            int startIndex = totalString.IndexOf("#");

            int startBrackettCount = 0;
            int endBrackettCount = 0;

            for (int i = startIndex; i < totalString.Length; i++)
            {
                if (totalString[i] == '(')
                    startBrackettCount++;
                if (totalString[i] == ')')
                    endBrackettCount++;

                if (endBrackettCount == startBrackettCount && endBrackettCount > 0)
                {
                    index = i;
                    break;
                }
            }

            return index;
        }

        /// <summary>
        /// Gets or sets whether to use strict path handling. If set to true, an exception will be caused
        /// if a path is not existing.
        /// </summary>
        public bool StrictPathHandling { get; set; } = false;
    }
}
