using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ObjectBinderEditor
{
    public static class ObjectBinderHelper
    {
        public static List<IBuildRule> BuildRules;

        public static List<INamingRule> NamingRules;

        static ObjectBinderHelper()
        {
            BuildRules = LoadAllInstance<IBuildRule>().OrderByDescending(p => p.Priority).ToList();
            NamingRules = LoadAllInstance<INamingRule>().OrderByDescending(p => (p is CustomNamingRule)).ToList();
        }

        public static IBuildRule GetBuildRule(TextAsset asset)
        {
            return BuildRules.FirstOrDefault(p => p.Validate(asset));
        }

        public static void ExecuteBinding(ObjectBinder binder)
        {
            IBuildRule buildRule = GetBuildRule(binder.asset)
                ?? throw new InvalidOperationException("Cannot find buildRule.");

            Validate(binder);

            buildRule.Build(binder);
        }

        public static void Validate(ObjectBinder binder)
        {
            if (binder == null)
            {
                throw new ArgumentNullException(nameof(binder), "ObjectBinder component not found, please select a PrefabSlot object first");
            }

            List<ObjectBinder.Item> itemList = binder.items;

            if (itemList == null || itemList.Count == 0)
            {
                throw new ArgumentNullException(nameof(itemList), "Item list cannot be null or empty");
            }

            if (binder.asset is MonoScript script)
            {
                var className = script.GetClass().Name;
                var names = binder.items.Where(p => p.name == className);
                if (names.Any())
                {
                    throw new InvalidOperationException($"Found item names that conflict with class name: {string.Join(", ", names)}");
                }
            }

            foreach (ObjectBinder.Item item in itemList)
            {
                bool isValid = Validate(binder, item.name, item.target);

                if (!isValid)
                {
                    throw new InvalidOperationException($"Found item with invalid: {item.name}");
                }
            }
        }

        public static bool Validate(ObjectBinder binder,string name,Object target)
        {
            var isValid = true;
            if (target == null || IsEditorType(target.GetType()))
                isValid = false;
            if (!IsValidIdentifier(name) || IsNameDuplicate(binder, name))
                isValid = false;
            return isValid;
        }

        public static bool IsEditorType(Type type)
        {
            if (type == null)
                return false;

            // Check if the type is in UnityEditor namespace or its sub-namespaces
            string typeNamespace = type.Namespace;
            if (!string.IsNullOrEmpty(typeNamespace) &&
                (typeNamespace == "UnityEditor" || typeNamespace.StartsWith("UnityEditor.")))
            {
                return true;
            }

            // Check if the type is defined in an editor assembly
            string assemblyName = type.Assembly.GetName().Name;
            if (assemblyName.Contains("Editor"))
            {
                return true;
            }

            return false;
        }

        public static bool IsValidIdentifier(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            if (!char.IsLetter(name[0]) && name[0] != '_')
                return false;

            for (int i = 1; i < name.Length; i++)
            {
                if (!char.IsLetterOrDigit(name[i]) && name[i] != '_')
                    return false;
            }

            string[] csharpKeywords = new string[]
            {
            "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked",
            "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else",
            "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for",
            "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock",
            "long", "namespace", "new", "null", "object", "operator", "out", "override", "params",
            "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed",
            "short", "sizeof", "stackalloc", "static", "string", "struct", "switch", "this",
            "throw", "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort",
            "using", "virtual", "void", "volatile", "while"
            };

            if (csharpKeywords.Contains(name.ToLower()))
                return false;

            return true;
        }

        public static bool IsNameDuplicate(ObjectBinder binder,string name)
        {
            return binder.items.Select(p => p.name).Count(p => p == name) > 1;
        }


        public static IEnumerable<T> LoadAllInstance<T>() where T : class
        {
            var targetType = typeof(T);

            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly =>
                {
                    try
                    {
                        return assembly.GetTypes();
                    }
                    catch
                    {
                        return Enumerable.Empty<Type>();
                    }
                })
                .Where(type =>
                    targetType.IsAssignableFrom(type) &&
                    !type.IsAbstract &&
                    !type.IsInterface &&
                    type != targetType)
                .Select(type => Activator.CreateInstance(type) as T)
                .Where(p => p != null);
            return types;
        }

        public static string EnsureUsingStatements(string content, List<ObjectBinder.Item> items)
        {
            var requiredNamespaces = items
                .Select(i => i.target.GetType().Namespace)
                .Where(ns => !string.IsNullOrEmpty(ns))
                .Distinct()
                .OrderBy(ns => ns)
                .ToList();

            if (requiredNamespaces.Count == 0)
                return content;

            var existingUsings = new HashSet<string>(
                Regex.Matches(content, @"^\s*using\s+([^\s;]+)\s*;", RegexOptions.Multiline)
                     .Cast<Match>()
                     .Select(m => m.Groups[1].Value)
            );

            var newUsings = requiredNamespaces
                .Where(ns => !existingUsings.Contains(ns))
                .Select(ns => $"using {ns};")
                .ToList();

            if (newUsings.Count == 0)
                return content;

            string newLine = content.Contains("\r\n") ? "\r\n" : "\n";
            string usingBlock = string.Join(newLine, newUsings) + newLine;

            var usingMatches = Regex.Matches(content, @"^\s*using\s+[^\s;]+\s*;", RegexOptions.Multiline);

            if (usingMatches.Count > 0)
            {
                var lastUsing = usingMatches[usingMatches.Count - 1];
                int insertPos = lastUsing.Index + lastUsing.Length;
                return content.Insert(insertPos, newLine + usingBlock);
            }

            var namespaceMatch = Regex.Match(content, @"^\s*namespace\s+[^{]+\{", RegexOptions.Multiline);
            if (namespaceMatch.Success)
            {
                int insertPos = namespaceMatch.Index + namespaceMatch.Length;
                return content.Insert(insertPos, newLine + usingBlock);
            }

            return usingBlock + content;
        }
        public static Dictionary<string, string> ExtractMethods(string source)
        {
            var result = new Dictionary<string, string>();

            var methodHeaderRegex = new Regex(
                @"(?<signature>" +
                @"(?:public|private|protected|internal|static|virtual|override|async|sealed|new|\s)+" +
                @"[\w<>\[\],\s]+\s+" +
                @"\w+\s*" +
                @"\([^\)]*\)\s*" +
                @")\{",
                RegexOptions.Compiled
            );

            foreach (Match match in methodHeaderRegex.Matches(source))
            {
                string signature = match.Groups["signature"].Value.Trim();

                int bodyStart = match.Index + match.Length;
                int braceCount = 1;
                int i = bodyStart;

                while (i < source.Length && braceCount > 0)
                {
                    if (source[i] == '{') braceCount++;
                    else if (source[i] == '}') braceCount--;
                    i++;
                }

                if (braceCount != 0)
                    continue;

                string body = source.Substring(bodyStart, i - bodyStart - 1).Trim();
                result[signature] = body;
            }

            return result;
        }
        public static void WriteCode(string modifiedContent, string scriptPath)
        {
            if (string.IsNullOrEmpty(modifiedContent))
            {
                throw new InvalidOperationException("Code generation failed, please check if the class definition is correct");
            }

            File.WriteAllText(scriptPath, modifiedContent);

            Debug.Log($"Updated class file: {scriptPath}");
        }
    }

}