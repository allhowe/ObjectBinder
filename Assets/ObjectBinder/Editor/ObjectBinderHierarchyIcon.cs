using System.Collections.Generic;
using System.Linq;
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

        private static Dictionary<int, (int total,int missing)> binderCache = new Dictionary<int, (int, int)>();
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
                binderCache.Clear();
                bindedObjectsCache.Clear();
            }
            else
            {
                InitStyles();
                AddEvents();
                UpdateCache();
            }
        }

        public static void ForceUpdate()
        {
            if (!Enable)
                return;

            cacheNeedsUpdate = true;
            EditorApplication.RepaintHierarchyWindow();
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

            if (binderCache.ContainsKey(instanceID))
            {
                DrawBinderIcon(instanceID, selectionRect);
                return;
            }

            if (bindedObjectsCache.Contains(instanceID))
            {
                DrawBindedIcon(selectionRect);
            }
        }

        private static void DrawBinderIcon(int instanceId, Rect selectionRect)
        {
            GUIStyle currentIconStyle = new GUIStyle(iconStyle);
            GUIStyle currentCountStyle = new GUIStyle(countStyle);
            Color bgColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

            var (totalCount, missingCount) = binderCache[instanceId];

            bool hasMissing = missingCount > 0;

            if (hasMissing)
            {
                bgColor = new Color(1f, 0.2f, 0.2f, 0.3f);
                currentIconStyle.normal.textColor = new Color(1f, 0.3f, 0.3f);
                currentCountStyle.normal.textColor = new Color(1f, 0.3f, 0.3f);
            }

            if (totalCount > 0)
            {
                float xPos = selectionRect.xMax - ICON_WIDTH - COUNT_WIDTH - MARGIN * 2;
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

            binderCache.Clear();
            bindedObjectsCache.Clear();

            foreach (var binder in allBinders)
            {
                UpdateCache(binder);
                UpdateBindedCache(binder);
            }
        }

        private static void UpdateCache(ObjectBinder binder)
        {
            var instanceId = binder.gameObject.GetInstanceID();

            var count = binder.items.Count;
            var missingCount = binder.items.Count(p => !ObjectBinderHelper.Validate(binder, p.name, p.target));

            binderCache[instanceId] = (count, missingCount);
        }


        private static void UpdateBindedCache(ObjectBinder binder)
        {
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
        }
    }
}