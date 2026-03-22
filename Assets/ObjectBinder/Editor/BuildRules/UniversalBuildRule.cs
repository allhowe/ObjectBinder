using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace ObjectBinderEditor {
    public class UniversalBuildRule : IBuildRule
    {
        public const string GENERATED_CODE_REGION_START = "#region ObjectBinder Auto Generated";
        public const string GENERATED_CODE_REGION_END = "#endregion ObjectBinder Auto Generated";
        public const string INDENT = "    ";

        public int Priority => 0;

        public TextAsset Bind(GameObject target)
        {
            return null;
        }

        public bool IsValid(TextAsset asset)
        {
            if (asset == null)
            {
                return false;
            }

            var monoScript = asset as MonoScript;
            if (monoScript == null)
            {
                return false;
            }

            Type scriptType = monoScript.GetClass();

            if (scriptType == null)
            {
                return false;
            }
            if (scriptType == typeof(ObjectBinder))
            {
                return false;
            }

            var scriptPath = AssetDatabase.GetAssetPath(monoScript);
            return scriptPath.StartsWith("Assets");
        }


        public void Build(ObjectBinder binder)
        {
            var scriptPath = AssetDatabase.GetAssetPath(binder.asset);
            var className = ((MonoScript)binder.asset).GetClass().Name;

            var originContent = File.ReadAllText(scriptPath);

            var content = BuildContent(binder, className, originContent);

            ObjectBinderHelper.WriteCode(content, scriptPath);
        }

        public string BuildContent(ObjectBinder binder, string className, string originContent)
        {
            var content = originContent;

            content = ObjectBinderHelper.EnsureUsingStatements(content, binder.items);
            string classPattern = $@"(public\s+(?:partial\s+)?class\s+{Regex.Escape(className)})(\s*:\s*[^{{]+)?(\s*{{)";
            Match classMatch = Regex.Match(content, classPattern);
            if (!classMatch.Success)
            {
                throw new InvalidOperationException($"Class definition not found: {className}");
            }
            var classBodyStartIndex = classMatch.Index + classMatch.Length;

            content = RemoveOldCode(content, classBodyStartIndex);
            classMatch = Regex.Match(content, classPattern);
            if (!classMatch.Success)
            {
                throw new InvalidOperationException($"Class definition not found after removing old code: {className}");
            }
            classBodyStartIndex = classMatch.Index + classMatch.Length;

            var code = GenerateCode(binder, originContent);
            content = GenerateNewCode(content, code, classBodyStartIndex);

            return content;
        }

        public StringBuilder GenerateFieldCode(ObjectBinder binder)
        {
            var binderField = "binder";

            var builder = new StringBuilder();
            builder.AppendLine($"private ObjectBinder {binderField};");
            foreach (var item in binder.items)
            {
                builder.AppendLine($"public {item.target.GetType().Name} {item.name} {{ get; private set; }}");
            }
            builder.AppendLine();
            return builder;
        }

        public StringBuilder GenerateInitCode(ObjectBinder binder)
        {
            var tab = INDENT;

            var binderField = "binder";

            var builder = new StringBuilder();
            builder.AppendLine("public void InitBind(GameObject target)");
            builder.AppendLine("{");
            builder.AppendLine($"{tab}{binderField} = target.GetComponent<{nameof(ObjectBinder)}>();");
            foreach (var item in binder.items)
            {
                builder.AppendLine($"{tab}{item.name} = {binderField}.{nameof(binder.Get)}<{item.target.GetType().Name}>(nameof({item.name}));");
            }
            builder.AppendLine("}");
            return builder;
        }

        public string GenerateCode(ObjectBinder binder, string content)
        {
            var rules = ObjectBinderHelper.LoadAllInstance<IEventRule>();
            var existingMethods = ObjectBinderHelper.ExtractMethods(content);

            var builder = GenerateFieldCode(binder);
            builder.Append(GenerateInitCode(binder));

            var eventHandlers = binder.items
                .Select(item => (rule: rules.FirstOrDefault(p => p.IsMatch(item)), item))
                .Where(x => x.rule != null)
                .ToList();

            if (eventHandlers.Count > 0)
            {
                var eventCodeBuidlder = new StringBuilder();
                eventCodeBuidlder.AppendLine();
                foreach (var handlerInfo in eventHandlers)
                {
                    string addEventCode = handlerInfo.rule.GenerateEventCode(handlerInfo.item);
                    eventCodeBuidlder.AppendLine($"{INDENT}{addEventCode}");
                }

                var insetIndex = builder.ToString().LastIndexOf('}');
                builder.Insert(insetIndex, eventCodeBuidlder.ToString());
            }


            if (eventHandlers.Count > 0)
            {
                builder.AppendLine();
                GenerateEventHandlers(builder, eventHandlers, existingMethods);
            }

            return builder.ToString();
        }
        public void GenerateEventHandlers(StringBuilder builder, List<(IEventRule rule, ObjectBinder.Item item)> eventHandlers, Dictionary<string, string> existingMethods)
        {
            foreach (var handlerInfo in eventHandlers)
            {
                IEventRule rule = handlerInfo.rule;
                string signature = rule.GenerateEventMethodSignature(handlerInfo.item);

                builder.AppendLine(signature);
                builder.AppendLine("{");
                if (existingMethods.TryGetValue(signature, out var methodBody))
                {
                    string[] bodyLines = methodBody.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                    foreach (string line in bodyLines)
                    {
                        if (string.IsNullOrWhiteSpace(line))
                        {
                            builder.AppendLine();
                        }
                        else
                        {
                            builder.AppendLine($"{INDENT}{line.Trim()}");
                        }
                    }
                }
                else
                {
                    builder.AppendLine();
                }

                builder.AppendLine("}");
                builder.AppendLine();
            }
        }


        public string RemoveOldCode(string content, int searchStartIndex)
        {
            string escapedStart = Regex.Escape(GENERATED_CODE_REGION_START);
            string escapedEnd = Regex.Escape(GENERATED_CODE_REGION_END);

            string pattern = $@"\r?\n\s*{escapedStart}.*?{escapedEnd}\r?\n?";

            string searchContent = content.Substring(searchStartIndex);
            Match match = Regex.Match(searchContent, pattern, RegexOptions.Singleline);

            if (match.Success)
            {
                Debug.Log($"Detected old generated code region, replacing...");
                string beforeOldCode = content.Substring(0, searchStartIndex);
                string afterMatch = searchContent.Substring(match.Index + match.Length);
                return beforeOldCode + searchContent.Substring(0, match.Index) + afterMatch;
            }

            return content;
        }
        public string GenerateNewCode(string content, string code, int classBodyStartIndex)
        {
            var nameSpace = string.Empty;
            Match namespaceMatch = Regex.Match(content, @"namespace\s+([\w.]+)\s*\{?");
            if (namespaceMatch.Success)
            {
                nameSpace = namespaceMatch.Groups[1].Value;
            }
            bool hasNameSpace = !string.IsNullOrEmpty(nameSpace);
            string baseIndent = hasNameSpace ? INDENT : "";
            string codeIndent = baseIndent + INDENT;

            StringBuilder insertCode = new StringBuilder();
            insertCode.AppendLine();
            insertCode.AppendLine($"{codeIndent}{GENERATED_CODE_REGION_START}");
            insertCode.AppendLine();

            string[] lines = code.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                if (!string.IsNullOrEmpty(line.Trim()))
                {
                    insertCode.AppendLine($"{codeIndent}{line}");
                }
                else
                {
                    insertCode.AppendLine();
                }
            }

            insertCode.AppendLine($"{codeIndent}{GENERATED_CODE_REGION_END}");

            var modifiedContent = content.Insert(classBodyStartIndex, insertCode.ToString());

            return modifiedContent;
        }
    }
}