using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace JUST
{
    internal class ReflectionHelper
    {
        internal static object InvokeFunction(Assembly assembly, String myclass, String mymethod, object[] parameters, bool convertParameters = false)
        {
            Type type = assembly?.GetType(myclass) ?? Type.GetType(myclass);
            MethodInfo methodInfo = type.GetTypeInfo().GetMethod(mymethod);
            var instance = !methodInfo.IsStatic ? Activator.CreateInstance(type) : null;

            var typedParameters = new List<object>();
            if (convertParameters)
            {
                var parameterInfos = methodInfo.GetParameters();
                for (int i = 0; i < parameterInfos.Length; i++)
                {
                    var pType = parameterInfos[i].ParameterType;
                    typedParameters.Add(GetTypedValue(pType, parameters[i]));
                }
            }
            return methodInfo.Invoke(instance, convertParameters ? typedParameters.ToArray() : parameters);
        }

        private static Assembly GetAssembly(bool isAssemblyDefined, string assemblyName, string namespc, string methodName)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            if (isAssemblyDefined)
            {
                var assemblyFileName = !assemblyName.EndsWith(".dll") ? $"{assemblyName}.dll" : assemblyName;
                var assembly = assemblies.SingleOrDefault(a => a.ManifestModule.Name == assemblyFileName);
                if (assembly == null)
                {
                    var assemblyLocation = Path.Combine(Directory.GetCurrentDirectory(), assemblyFileName);
                    assembly = Assembly.LoadFile(assemblyLocation);
                    AppDomain.CurrentDomain.Load(assembly.GetName());
                }

                return assembly;
            }
            else
            {
                foreach (var assembly in assemblies.Where(a => !a.FullName.StartsWith("System.")))
                {
                    Type[] types = null;
                    try
                    {
                        types = assembly.GetTypes();
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        types = ex.Types;
                    }

                    foreach (var typeInfo in types)
                    {
                        if (string.Compare(typeInfo.FullName, namespc, StringComparison.InvariantCultureIgnoreCase) == 0)
                        {
                            return assembly;
                        }
                    }
                }
            }
            return null;
        }

        private static object[] FilterParameters(object[] parameters)
        {
            if (string.IsNullOrEmpty(parameters[0]?.ToString() ?? string.Empty))
            {
                parameters = parameters.Skip(1).ToArray();
            }
            if (parameters.Length > 0 && parameters.Last().ToString() == "{}")
            {
                parameters = parameters.Take(parameters.Length - 1).ToArray();
            }
            return parameters;
        }

        private static object GetTypedValue(Type pType, object val)
        {
            object typedValue = val;
            var converter = TypeDescriptor.GetConverter(pType);
            if (converter.CanConvertFrom(val.GetType()))
            {
                typedValue = converter.ConvertFrom(val);
            }
            else if (pType.IsPrimitive)
            {
                typedValue = Convert.ChangeType(val, pType);
            }
            else if (!pType.IsAbstract)
            {
                var parse = pType.GetMethod("Parse", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(string) }, null);
                typedValue = parse?.Invoke(null, new[] { val }) ?? pType.GetConstructor(new[] { typeof(string) })?.Invoke(new[] { val }) ?? val;
            }
            return typedValue;
        }
    }
}
