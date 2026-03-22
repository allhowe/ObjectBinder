using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ObjectBinderEditor
{

    [CustomEditor(typeof(ObjectBinder), true)]
    public class ObjectBinderEditor : Editor
    {
        private VisualElement dropArea;
        private Label dropAreaLabel;
        private Object draggedObject;
        private List<string> componentChoices;
        private string selectedComponentType;
        private VisualElement root;

        private ObjectBinder binder;


        private void OnEnable()
        {
            binder = (ObjectBinder)target;

            TryBindScript();
        }

        private void TryBindScript()
        {
            if (binder.asset != null)
                return;

            foreach (var rule in ObjectBinderHelper.BuildRules)
            {
                var asset = rule.Bind(binder.gameObject);

                if (asset != null)
                {
                    binder.asset = asset;
                    break;
                }
            }
        }

        public override VisualElement CreateInspectorGUI()
        {
            root = new VisualElement();

            // Create drag and drop hint area
            dropArea = CreateDropArea();
            root.Add(dropArea);

            var itemsProperty = serializedObject.FindProperty("items");
            var itemsField = new PropertyField(itemsProperty);
            root.Add(itemsField);

            var bindField = CreateBindField();
            root.Add(bindField);

            return root;
        }

        private VisualElement CreateDropArea()
        {
            var area = new VisualElement();
            area.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f);
            area.style.borderBottomWidth = 1;
            area.style.borderTopWidth = 1;
            area.style.borderLeftWidth = 1;
            area.style.borderRightWidth = 1;
            area.style.borderBottomColor = new Color(0.15f, 0.15f, 0.15f);
            area.style.borderTopColor = new Color(0.15f, 0.15f, 0.15f);
            area.style.borderLeftColor = new Color(0.15f, 0.15f, 0.15f);
            area.style.borderRightColor = new Color(0.15f, 0.15f, 0.15f);
            area.style.paddingTop = 8;
            area.style.paddingBottom = 8;
            area.style.paddingLeft = 8;
            area.style.paddingRight = 8;
            area.style.marginTop = 8;
            area.style.marginBottom = 8;

            dropAreaLabel = new Label("Drag object here to add binding");
            dropAreaLabel.style.fontSize = 11;
            dropAreaLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            area.Add(dropAreaLabel);

            // Register drag and drop events
            area.RegisterCallback<DragUpdatedEvent>(evt =>
            {
                if (DragAndDrop.objectReferences.Length > 0)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    area.style.backgroundColor = new Color(0.35f, 0.35f, 0.35f);
                    evt.StopPropagation();
                }
            });

            area.RegisterCallback<DragLeaveEvent>(evt =>
            {
                area.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f);
            });

            area.RegisterCallback<DragPerformEvent>(evt =>
            {
                area.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f);

                if (DragAndDrop.objectReferences.Length > 0)
                {
                    OnObjectDropped(DragAndDrop.objectReferences[0]);
                }

                evt.StopPropagation();
            });

            return area;
        }

        private void OnObjectDropped(UnityEngine.Object obj)
        {
            if (obj == null) return;

            draggedObject = obj;

            // Collect available component types
            componentChoices = new List<string>();

            GameObject go = null;

            if (obj is GameObject gameObject)
            {
                go = gameObject;
            }
            else if (obj is Component component)
            {
                go = component.gameObject;
            }

            if (go != null)
            {
                componentChoices.Add("GameObject");
                var components = go.GetComponents<Component>();
                foreach (var c in components)
                {
                    if (c != null)
                    {
                        componentChoices.Add(c.GetType().Name);
                    }
                }
                ShowTypeSelectionUI();
            }
            else
            {
                // Other type objects, directly add with Custom naming
                AddNonGameObjectItem(obj);
                ResetDropArea();
            }
        }

        private void ShowTypeSelectionUI()
        {
            dropArea.Clear();

            var container = new VisualElement();
            container.style.width = Length.Percent(100);

            var titleLabel = new Label("Select object type:");
            titleLabel.style.marginBottom = 4;
            container.Add(titleLabel);

            // Create type selection buttons
            var buttonContainer = new VisualElement();
            buttonContainer.style.flexDirection = FlexDirection.Row;
            buttonContainer.style.flexWrap = Wrap.Wrap;

            foreach (var componentType in componentChoices)
            {
                var button = new Button(() => OnTypeSelected(componentType))
                {
                    text = componentType,
                    style =
                {
                    marginRight = 4,
                    marginBottom = 4
                }
                };
                buttonContainer.Add(button);
            }

            container.Add(buttonContainer);

            // Add cancel button
            var cancelButton = new Button(ResetDropArea)
            {
                text = "Cancel",
                style =
            {
                marginTop = 4
            }
            };
            container.Add(cancelButton);

            dropArea.Add(container);
        }

        private void OnTypeSelected(string typeName)
        {
            selectedComponentType = typeName;

            var targetObj = draggedObject;

            if (selectedComponentType != "GameObject" && selectedComponentType != draggedObject.GetType().Name)
            {
                GameObject go = null;
                if (draggedObject is GameObject gameObject)
                {
                    go = gameObject;
                }
                else if (draggedObject is Component comp)
                {
                    go = comp.gameObject;
                }

                if (go != null)
                {
                    var components = go.GetComponents<Component>();
                    foreach (var component in components)
                    {
                        if (component != null && component.GetType().Name == selectedComponentType)
                        {
                            targetObj = component;
                            break;
                        }
                    }
                }
            }
            else if (selectedComponentType == "GameObject")
            {
                if (draggedObject is Component comp)
                {
                    targetObj = comp.gameObject;
                }
            }

            draggedObject = targetObj;

            ShowNamingSelectionUI();
        }

        private void ShowNamingSelectionUI()
        {
            dropArea.Clear();

            var container = new VisualElement();
            container.style.width = Length.Percent(100);

            var titleLabel = new Label("Select naming method:");
            titleLabel.style.marginBottom = 4;
            container.Add(titleLabel);

            // Show preview
            var previewLabel = new Label();
            previewLabel.style.fontSize = 10;
            previewLabel.style.marginBottom = 6;
            container.Add(previewLabel);

            var namingOptions = ObjectBinderHelper.NamingRules.Select(p => p.Name);

            var buttonContainer = new VisualElement();
            buttonContainer.style.flexDirection = FlexDirection.Row;
            buttonContainer.style.flexWrap = Wrap.Wrap;

            foreach (var namingType in namingOptions)
            {
                var button = new Button(() => OnNamingSelected(namingType))
                {
                    text = namingType,
                    style =
                {
                    marginRight = 4,
                    marginBottom = 4
                }
                };

                // Show preview on mouse hover
                button.RegisterCallback<MouseEnterEvent>(evt =>
                {
                    previewLabel.text = $"Preview: {GetNamePreview(namingType)}";
                });

                buttonContainer.Add(button);
            }

            container.Add(buttonContainer);

            // Add back and cancel buttons
            var bottomButtons = new VisualElement();
            bottomButtons.style.flexDirection = FlexDirection.Row;
            bottomButtons.style.marginTop = 4;

            var backButton = new Button(ShowTypeSelectionUI)
            {
                text = "Back"
            };
            backButton.style.flexGrow = 1;
            bottomButtons.Add(backButton);

            var cancelButton = new Button(ResetDropArea)
            {
                text = "Cancel"
            };
            cancelButton.style.flexGrow = 1;
            bottomButtons.Add(cancelButton);

            container.Add(bottomButtons);
            dropArea.Add(container);
        }

        private string GetNamePreview(string namingType)
        {
            var rule = ObjectBinderHelper.NamingRules.FirstOrDefault(r => r.Name == namingType);
            return rule.Naming(draggedObject);
        }

        private void OnNamingSelected(string namingType)
        {
            AddItemWithSelection(namingType);
            ResetDropArea();
        }

        private void AddItemWithSelection(string namingType)
        {
            if (draggedObject == null) return;

            serializedObject.Update();
            var itemsProperty = serializedObject.FindProperty("items");

            // Add new item
            int newIndex = itemsProperty.arraySize;
            itemsProperty.InsertArrayElementAtIndex(newIndex);

            var newItem = itemsProperty.GetArrayElementAtIndex(newIndex);
            var nameProp = newItem.FindPropertyRelative("name");
            var objProp = newItem.FindPropertyRelative("target");


            // Set object based on selected type
            Object targetObj = draggedObject;
            objProp.objectReferenceValue = targetObj;

            // Set name based on naming method
            var rule = ObjectBinderHelper.NamingRules.FirstOrDefault(r => r.Name == namingType);
            string finalName = rule.Naming(draggedObject); ;

            nameProp.stringValue = finalName;
            serializedObject.ApplyModifiedProperties();

            Debug.Log($"Added new binding: {finalName} (Type: {selectedComponentType}, Naming method: {namingType})");
        }

        private void AddNonGameObjectItem(UnityEngine.Object obj)
        {
            if (obj == null) return;

            serializedObject.Update();
            var itemsProperty = serializedObject.FindProperty("items");

            // Add new item
            int newIndex = itemsProperty.arraySize;
            itemsProperty.InsertArrayElementAtIndex(newIndex);

            var newItem = itemsProperty.GetArrayElementAtIndex(newIndex);
            var nameProp = newItem.FindPropertyRelative("name");
            var objProp = newItem.FindPropertyRelative("target");

            objProp.objectReferenceValue = obj;
            nameProp.stringValue = obj.name;

            serializedObject.ApplyModifiedProperties();

            Debug.Log($"Added new binding: {obj.name} (Type: {obj.GetType().Name}, Naming method: Custom)");
        }

        private void ResetDropArea()
        {
            draggedObject = null;
            selectedComponentType = null;
            componentChoices = null;

            dropArea.Clear();
            dropAreaLabel = new Label("Drag object here to add binding");
            dropAreaLabel.style.fontSize = 11;
            dropAreaLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            dropArea.Add(dropAreaLabel);
        }

        private VisualElement CreateBindField()
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.alignItems = Align.Center;
            container.style.marginTop = 10;
            container.style.marginBottom = 10;

            var label = new Label("Bind Asset");
            label.style.minWidth = 80;
            label.style.marginRight = 5;
            container.Add(label);

            var objectField = new ObjectField()
            {
                objectType = typeof(TextAsset),
                value = binder.asset,
            };
            objectField.style.flexGrow = 1;
            objectField.style.flexShrink = 1;
            objectField.style.marginRight = 5;
            container.Add(objectField);

            var generateCodeButton = new Button();
            generateCodeButton.text = "Generate Code";
            generateCodeButton.style.minWidth = 120;
            generateCodeButton.style.height = 24;
            generateCodeButton.style.fontSize = 12;
            generateCodeButton.style.unityFontStyleAndWeight = FontStyle.Bold;
            generateCodeButton.style.marginRight = -1;
            generateCodeButton.style.borderTopLeftRadius = 3;
            generateCodeButton.style.borderTopRightRadius = 3;
            generateCodeButton.style.borderBottomLeftRadius = 3;
            generateCodeButton.style.borderBottomRightRadius = 3;
            container.Add(generateCodeButton);

            generateCodeButton.tooltip = "BuildRule: " + ObjectBinderHelper.GetBuildRule(binder.asset).GetType().Name;

            generateCodeButton.RegisterCallback<ClickEvent>(evt =>
            {
                ObjectBinderHelper.ExecuteBinding(binder);
            });

            objectField.RegisterValueChangedCallback(evt =>
            {
                var newObject = evt.newValue as TextAsset;
                var currentObject = evt.previousValue;

                if (newObject != currentObject)
                {
                    if (newObject == null)
                    {
                        binder.asset = null;
                        EditorUtility.SetDirty(binder);
                    }
                    else
                    {
                        if (ObjectBinderHelper.GetBuildRule(newObject) != null)
                        {
                            binder.asset = newObject;
                            EditorUtility.SetDirty(binder);
                        }
                        else
                        {
                            Debug.LogError("The selected Object does not match any build rules.");
                            objectField.SetValueWithoutNotify(currentObject);
                        }
                    }
                }

                generateCodeButton.tooltip = "BuildRule: " + ObjectBinderHelper.GetBuildRule(binder.asset).GetType().Name;
            });

            var settingButton = new Button();
            settingButton.style.backgroundImage = EditorGUIUtility.IconContent("_Menu").image as Texture2D;
            settingButton.style.width = 24;
            settingButton.style.height = 24;
            settingButton.style.marginLeft = 0;
            settingButton.style.borderTopLeftRadius = 3;
            settingButton.style.borderTopRightRadius = 3;
            settingButton.style.borderBottomLeftRadius = 3;
            settingButton.style.borderBottomRightRadius = 3;

            settingButton.RegisterCallback<ClickEvent>(evt =>
            {
                var menu = new GenericMenu();

                var enable = ObjectBinderHierarchyIcon.Enable;
                var itemsProperty = serializedObject.FindProperty("items");

                menu.AddItem(new GUIContent("Hierarchy Icon"), enable, () =>
                {
                    ObjectBinderHierarchyIcon.SetEnable(!enable);
                });

                menu.AddSeparator("");

                menu.AddItem(new GUIContent("Clear All Items"), false, () =>
                {
                    itemsProperty.ClearArray();
                    itemsProperty.serializedObject.ApplyModifiedProperties();
                });

                menu.AddItem(new GUIContent("Remove Invalid Items"), false, () =>
                {
                    for (int i = itemsProperty.arraySize - 1; i >= 0; i--)
                    {
                        var element = itemsProperty.GetArrayElementAtIndex(i);
                        var nameProp = element.FindPropertyRelative("name");
                        var targetProp = element.FindPropertyRelative("target");

                        var name = nameProp.stringValue;
                        var target = targetProp.objectReferenceValue;

                        var isValid = ObjectBinderHelper.Validate(binder, name, target);

                        if (!isValid)
                        {
                            itemsProperty.DeleteArrayElementAtIndex(i);
                        }
                    }

                    serializedObject.ApplyModifiedProperties();
                });

                menu.ShowAsContext();
            });

            container.Add(settingButton);



            return container;
        }
    }
}