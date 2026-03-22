using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace ObjectBinderEditor
{
    [CustomPropertyDrawer(typeof(ObjectBinder.Item))]
    public class ObjectBinderItemDrawer : PropertyDrawer
    {
        private const string custom = "Custom";

        private static Action OnItemChanged;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var nameProp = property.FindPropertyRelative("name");
            var targetProp = property.FindPropertyRelative("target");

            var root = CreateRoot();

            var typeDropdown = CreateTypeDropdown(targetProp);
            var targetField = CreateTargetField(targetProp, typeDropdown);
            var nameField = CreateNameField(nameProp);
            var namingButton = CreateNamingButton(targetProp, nameProp);

            root.Add(targetField);
            root.Add(typeDropdown);
            root.Add(nameField);
            root.Add(namingButton);

            root.RegisterCallback<AttachToPanelEvent>(_ =>
            {
                OnItemChanged += Validate;
            });
            root.RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                OnItemChanged -= Validate;
            });

            root.TrackPropertyValue(property, evt =>
            {
                OnItemChanged?.Invoke();
            });

            Validate();

            void Validate()
            {
                var binder = (ObjectBinder)targetProp.serializedObject.targetObject;
                var target = targetProp.objectReferenceValue;
                var name = nameProp.stringValue;

                bool isValid = ObjectBinderHelper.Validate(binder,name,target);
                root.style.backgroundColor = isValid ? StyleKeyword.Null : new Color(1f, 0.2f, 0.2f, 0.3f);
            }

            return root;
        }

        private VisualElement CreateRoot()
        {
            var root = new VisualElement();
            root.style.flexDirection = FlexDirection.Row;
            root.style.alignItems = Align.Center;
            root.style.paddingTop = 1;
            root.style.paddingBottom = 1;
            root.style.minHeight = 20;
            return root;
        }

        private void ApplyFieldStyle(VisualElement element, float widthPercent)
        {
            element.style.width = Length.Percent(widthPercent);
            element.style.marginRight = 2;
            element.style.flexShrink = 0;
            element.style.flexGrow = 0;
        }

        private PropertyField CreateTargetField(SerializedProperty targetProp, DropdownField typeDropdown)
        {
            var targetField = new PropertyField(targetProp, "");
            ApplyFieldStyle(targetField, 30);
            targetField.RegisterValueChangeCallback(evt =>
            {
                UpdateTypeDropdown(targetProp, typeDropdown);
            });

            UpdateTypeDropdown(targetProp, typeDropdown);

            return targetField;
        }
        private DropdownField CreateTypeDropdown(SerializedProperty targetProp)
        {
            var dropdown = new DropdownField();
            ApplyFieldStyle(dropdown, 30);
            dropdown.RegisterValueChangedCallback(evt =>
            {
                OnTypeSelected(targetProp, evt.newValue);
            });
            return dropdown;
        }
        private PropertyField CreateNameField(SerializedProperty nameProp)
        {
            var nameField = new PropertyField(nameProp, "");
            nameField.style.flexGrow = 1;
            nameField.style.flexShrink = 1;
            nameField.style.marginRight = -4;
            return nameField;
        }
        private Button CreateNamingButton(SerializedProperty targetProp, SerializedProperty nameProp)
        {
            var button = new Button();
            button.style.width = 18;
            button.style.height = 19;
            button.style.minWidth = 18;
            button.style.backgroundImage = EditorGUIUtility.IconContent("d_dropdown").image as Texture2D;

            button.RegisterCallback<ClickEvent>(evt =>
            {
                ShowNamingMenu(targetProp, nameProp);
            });

            return button;
        }


        private void UpdateTypeDropdown(SerializedProperty targetProp, DropdownField dropdown)
        {
            var obj = targetProp.objectReferenceValue;

            if (obj == null)
            {
                dropdown.choices = new List<string> { "None" };
                dropdown.value = "None";
                dropdown.SetEnabled(false);
                return;
            }

            GameObject gameObject = null;
            string currentType = "";

            if (obj is GameObject go)
            {
                gameObject = go;
                currentType = "GameObject";
            }
            else if (obj is Component comp)
            {
                gameObject = comp.gameObject;
                currentType = comp.GetType().Name;
            }
            else
            {
                dropdown.choices = new List<string> { obj.GetType().Name };
                dropdown.value = obj.GetType().Name;
                dropdown.SetEnabled(false);
                return;
            }

            if (gameObject != null)
            {
                var components = gameObject.GetComponents<Component>();
                var choices = new List<string> { "GameObject" };

                foreach (var c in components)
                {
                    if (c != null)
                    {
                        choices.Add(c.GetType().Name);
                    }
                }

                dropdown.choices = choices;
                dropdown.value = currentType;
                dropdown.SetEnabled(true);
            }
        }
        private void OnTypeSelected(SerializedProperty targetProp, string typeName)
        {
            var obj = targetProp.objectReferenceValue;

            if (obj == null || typeName == "None")
                return;

            GameObject go = null;

            if (obj is GameObject gameObject)
            {
                go = gameObject;
            }
            else if (obj is Component comp)
            {
                go = comp.gameObject;
            }
            else
            {
                return;
            }

            if (go == null)
                return;

            Object newObj = null;
            if (typeName == "GameObject")
            {
                newObj = go;
            }
            else
            {
                var components = go.GetComponents<Component>();
                foreach (var component in components)
                {
                    if (component != null && component.GetType().Name == typeName)
                    {
                        newObj = component;
                        break;
                    }
                }
            }

            if (newObj != null)
            {
                targetProp.objectReferenceValue = newObj;
                targetProp.serializedObject.ApplyModifiedProperties();
            }
        }


        private void ShowNamingMenu(SerializedProperty targetProp, SerializedProperty nameProp)
        {
            var obj = targetProp.objectReferenceValue;
            if (obj == null)
                return;

            var menu = new GenericMenu();
            var currentNamingType = DetectNamingType(targetProp, nameProp);

            foreach (var rule in ObjectBinderHelper.NamingRules)
            {
                var ruleName = rule.Name;
                var isSelected = ruleName == currentNamingType;

                if (rule.Name == custom)
                {
                    menu.AddDisabledItem(new GUIContent(ruleName), isSelected);
                }
                else
                {
                    menu.AddItem(new GUIContent(ruleName), isSelected, () =>
                    {
                        nameProp.stringValue = rule.Naming(obj);
                        nameProp.serializedObject.ApplyModifiedProperties();
                    });
                }
            }

            menu.ShowAsContext();
        }
        private string DetectNamingType(SerializedProperty targetProp, SerializedProperty nameProp)
        {
            var obj = targetProp.objectReferenceValue;
            var currentName = nameProp.stringValue;

            if (obj == null || string.IsNullOrEmpty(currentName))
                return custom;

            foreach (var rule in ObjectBinderHelper.NamingRules)
            {
                string generatedName = rule.Naming(obj);
                if (currentName == generatedName)
                {
                    return rule.Name;
                }
            }

            return custom;
        }


    }
}