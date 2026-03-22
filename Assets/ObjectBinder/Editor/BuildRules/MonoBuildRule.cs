using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ObjectBinderEditor
{
    public class MonoBuildRule : IBuildRule
    {
        private UniversalBuildRule universal = new UniversalBuildRule();

        public int Priority => 10;
        public TextAsset Bind(GameObject target)
        {
            var monoBehaviours = target.GetComponents<MonoBehaviour>();

            foreach (var monoBehaviour in monoBehaviours)
            {
                if (monoBehaviour == null || monoBehaviour is ObjectBinder)
                    continue;

                var monoScript = MonoScript.FromMonoBehaviour(monoBehaviour);

                if (universal.IsValid(monoScript))
                {
                    return monoScript;
                }
            }
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
            if (!scriptType.IsSubclassOf(typeof(MonoBehaviour)))
            {
                return false;
            }

            return universal.IsValid(monoScript);
        }
        public void Build(ObjectBinder binder)
        {
            var scriptPath = AssetDatabase.GetAssetPath(binder.asset);
            var className = ((MonoScript)binder.asset).GetClass().Name;

            var originContent = File.ReadAllText(scriptPath);

            var content = universal.BuildContent(binder, className, originContent);

            content = ReplaceContent(content);

            ObjectBinderHelper.WriteCode(content, scriptPath);
        }

        public string ReplaceContent(string content)
        {
            content = content.Replace("public void InitBind(GameObject target)", "public void InitBind()");
            content = content.Replace($"target.GetComponent<{nameof(ObjectBinder)}>();", $"GetComponent<{nameof(ObjectBinder)}>();");
            return content;
        }
    }

}