using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace ObjectBinderEditor
{
    public class NewFileBuildRule : IBuildRule
    {
        private UniversalBuildRule universal = new UniversalBuildRule();


        public int Priority => -1;
        public TextAsset Bind(GameObject target)
        {
            return null;
        }
        public bool Validate(TextAsset asset)
        {
            return asset == null;
        }

        public void Build(ObjectBinder binder)
        {
            var path = EditorUtility.SaveFilePanelInProject("Create New File", "NewFile", "cs", "Please enter a file name to create");

            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning("NewFileBuildRule: Create Cancel.");
                return;
            }

            var scriptPath = Application.dataPath + path.Substring("Assets".Length);

            Debug.Log(scriptPath);
            if (!File.Exists(scriptPath))
            {
                File.Create(scriptPath).Close();
            }

            var className = Path.GetFileNameWithoutExtension(scriptPath);

            var originContent = CreateOriginConent(className);

            var content = universal.BuildContent(binder, className, originContent);

            ObjectBinderHelper.WriteCode(content, scriptPath);

            AssetDatabase.Refresh();


            CompilationPipeline.compilationFinished += OnCompilationFinished;

            void OnCompilationFinished(object obj)
            {
                CompilationPipeline.compilationFinished -= OnCompilationFinished;

                SessionState.SetBool("AutoAddComponent_Pending", true);
                SessionState.SetString("AutoAddComponent_Path", path);
                SessionState.SetInt("AutoAddComponent_BinderId", binder.GetInstanceID());
            }
        }

        public string CreateOriginConent(string className)
        {
            var builder = new StringBuilder();
            builder.AppendLine("using UnityEngine;");
            builder.AppendLine();
            builder.AppendLine($"public class {className} : MonoBehaviour");
            builder.AppendLine("{");
            builder.AppendLine();
            builder.AppendLine("}");
            return builder.ToString();
        }


        [InitializeOnLoadMethod]
        static void AutoAddComponentHandler()
        {
            EditorApplication.delayCall += TryAddComponentAfterReload;
        }
        static void TryAddComponentAfterReload()
        {
            if (!SessionState.GetBool("AutoAddComponent_Pending", false))
                return;
            SessionState.EraseBool("AutoAddComponent_Pending");

            var path = SessionState.GetString("AutoAddComponent_Path", "");
            var binderId = SessionState.GetInt("AutoAddComponent_BinderId", 0);

            try
            {
                var binder = EditorUtility.InstanceIDToObject(binderId) as ObjectBinder;
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                var type = script.GetClass();

                binder.asset = script;
                binder.gameObject.AddComponent(type);
                EditorUtility.SetDirty(binder);

                Debug.Log($"Component added. {type.Name}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to add component after compilation. Please add it manually. {e.Message}");
            }
            finally
            {
                Debug.Log($"Successfully create new file at: {path}");
            }

        }
    }

}