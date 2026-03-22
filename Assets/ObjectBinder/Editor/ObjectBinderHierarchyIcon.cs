using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ObjectBinderEditor
{
    [InitializeOnLoad]
    public static class ObjectBinderHierarchyIcon
    {

        private static GUIStyle iconStyle;
        private static GUIStyle countStyle;
        private static GUIStyle bindedStyle;
        private const float ICON_WIDTH = 14f;
        private const float COUNT_WIDTH = 30f;
        private const float MARGIN = 0f;

        private static Dictionary<int,int> invalidCache = new Dictionary<int,int>();
        private static HashSet<int> bindedObjectsCache = new HashSet<int>();
        private static bool cacheNeedsUpdate = true;


        public static bool Enable { get; internal set; }

        static ObjectBinderHierarchyIcon()
        {
            Enable = EditorPrefs.GetBool("ObjectBinderHierarchyIcon", true);
            SetEnable(Enable);
        }

        public static void SetEnable(bool enable)
        {
            Enable = enable;
            EditorPrefs.SetBool("ObjectBinderHierarchyIcon", enable);

            if (!enable)
            {
                RemoveEvents();
                bindedObjectsCache.Clear();
            }
            else
            {
                InitStyles();
                AddEvents();
                UpdateCache();
            }
        }

        private static void AddEvents()
        {
            EditorApplication.hierarchyWindowItemOnGUI += DrawHierarchyIcon;
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            PrefabStage.prefabStageOpened += OnPrefabStageChanged;
            PrefabStage.prefabStageClosing += OnPrefabStageClosing;
        }
        private static void RemoveEvents()
        {
            EditorApplication.hierarchyWindowItemOnGUI -= DrawHierarchyIcon;
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            PrefabStage.prefabStageOpened -= OnPrefabStageChanged;
            PrefabStage.prefabStageClosing -= OnPrefabStageClosing;
        }

        private static void InitStyles()
        {
            iconStyle = new GUIStyle()
            {
                fontSize = 10,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.8f, 0.8f, 0.8f) }
            };

            countStyle = new GUIStyle()
            {
                fontSize = 9,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleRight,
                normal = { textColor = new Color(0.8f, 0.8f, 0.8f) }
            };

            bindedStyle = new GUIStyle()
            {
                fontSize = 32,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.75f, 0.75f, 0.75f) }
            };
        }

        private static void OnHierarchyChanged()
        {
            cacheNeedsUpdate = true;
            EditorApplication.RepaintHierarchyWindow();
        }

        private static void OnPrefabStageChanged(PrefabStage stage)
        {
            cacheNeedsUpdate = true;
            EditorApplication.RepaintHierarchyWindow();
        }

        private static void OnPrefabStageClosing(PrefabStage stage)
        {
            cacheNeedsUpdate = true;
            EditorApplication.RepaintHierarchyWindow();
        }

        private static void DrawHierarchyIcon(int instanceID, Rect selectionRect)
        {
            if (cacheNeedsUpdate)
            {
                cacheNeedsUpdate = false;
                UpdateCache();
            }

            GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

            if (obj == null)
                return;

            if (obj.TryGetComponent<ObjectBinder>(out var binder))
            {
                DrawBinderIcon(instanceID, binder, selectionRect);
                return;
            }

            if (bindedObjectsCache.Contains(instanceID))
            {
                DrawBindedIcon(selectionRect);
            }
        }

        private static void DrawBinderIcon(int instanceId, ObjectBinder binder, Rect selectionRect)
        {
            GUIStyle currentIconStyle = new GUIStyle(iconStyle);
            GUIStyle currentCountStyle = new GUIStyle(countStyle);
            Color bgColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

            int totalCount = binder.items.Count;
            bool hasMissing = invalidCache.TryGetValue(instanceId, out int missingCount);

            if (hasMissing)
            {
                bgColor = new Color(1f, 0.2f, 0.2f, 0.3f);
                currentIconStyle.normal.textColor = new Color(1f, 0.3f, 0.3f);
                currentCountStyle.normal.textColor = new Color(1f, 0.3f, 0.3f);
            }


            float xPos = selectionRect.xMax - ICON_WIDTH - COUNT_WIDTH - MARGIN * 2;

            if (totalCount > 0)
            {
                Rect countRect = new Rect(xPos, selectionRect.y, COUNT_WIDTH, selectionRect.height);
                string countText = hasMissing ? $"({totalCount}/{missingCount})" : $"({totalCount})";

                GUI.Label(countRect, countText, currentCountStyle);
            }

            Rect iconRect = new Rect(selectionRect.xMax - ICON_WIDTH - MARGIN, selectionRect.y, ICON_WIDTH, selectionRect.height);

            EditorGUI.DrawRect(new Rect(iconRect.x, iconRect.y + 2, iconRect.width, iconRect.height - 4), bgColor);

            GUI.Label(iconRect, "B", currentIconStyle);
        }

        private static void DrawBindedIcon(Rect selectionRect)
        {
            Rect iconRect = new Rect(selectionRect.xMax - ICON_WIDTH - MARGIN, selectionRect.y-2, ICON_WIDTH, selectionRect.height);
            GUI.Label(iconRect, "¡¤", bindedStyle);
        }

        private static void UpdateCache()
        {
            bindedObjectsCache.Clear();
            invalidCache.Clear();

            List<ObjectBinder> allBinders = new List<ObjectBinder>();

            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();

            if (prefabStage != null)
            {
                GameObject prefabRoot = prefabStage.prefabContentsRoot;
                if (prefabRoot != null)
                {
                    allBinders.AddRange(prefabRoot.GetComponentsInChildren<ObjectBinder>(true));
                }
            }
            else
            {
                allBinders.AddRange(Object.FindObjectsOfType<ObjectBinder>());
            }


            foreach (var binder in allBinders)
            {
                if (binder.items == null)
                    continue;

                foreach (var item in binder.items)
                {
                    if (item.target == null)
                        continue;

                    GameObject targetGameObject = null;

                    if (item.target is GameObject go)
                    {
                        targetGameObject = go;
                    }
                    else if (item.target is Component comp)
                    {
                        targetGameObject = comp.gameObject;
                    }

                    if (targetGameObject != null)
                    {
                        int instanceID = targetGameObject.GetInstanceID();
                        bindedObjectsCache.Add(instanceID);
                    }
                }

                UpdateInvalidCache(binder);
            }
        }

        private static void UpdateInvalidCache(ObjectBinder binder)
        {
            if (binder == null || binder.items == null)
                return;

            var count = 0;

            foreach (var item in binder.items)
            {
                var isValid = ObjectBinderHelper.Validate(binder, item.name, item.target);
                if (!isValid)
                {
                    count++;
                }
            }

            if (count > 0)
            {
                invalidCache[binder.gameObject.GetInstanceID()] = count;
            }
        }
    }
}