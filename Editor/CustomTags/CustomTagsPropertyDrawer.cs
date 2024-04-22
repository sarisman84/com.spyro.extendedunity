using Spyro;
using Spyro.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.UI.InputField;

namespace Spyro
{


    [CustomPropertyDrawer(typeof(TagAttribute))]
    public class CustomTagsPropertyDrawer : PropertyDrawer
    {
        struct DropdownFieldArgs
        {
            public DropdownField field;
            public TagAttribute info;
        }
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();

            if (property.propertyType != SerializedPropertyType.String)
            {
                var group = new VisualElement();
                AddErrorField(group, "Assigned field for custom tags is not a string", property);
                root.Add(group);
                return root;
            }

            var att = attribute as TagAttribute;
            att.UpdateData();
            att.UpdateSelectedTag(property.stringValue);

            if (!att.foundTagList)
            {
                var group = new VisualElement();
                AddErrorField(group, $"Could not find custom tags of name [{att.tagName}]", property);
                root.Add(group);
                return root;
            }

            var uxmlAsset = UIToolkitUtility.GetUXML("UCustomTagsEditor");
            uxmlAsset.CloneTree(root);

            var fieldDropdown = root.Q<DropdownField>("dropdown_field");
            var args = new DropdownFieldArgs
            {
                field = fieldDropdown,
                info = att
            };
            fieldDropdown.RegisterCallback<ChangeEvent<int>, DropdownFieldArgs>(UpdateDropdown, args);

            fieldDropdown.value = att.foundTagList.tags[Mathf.Max(0, att.selectedTag)];
            fieldDropdown.choices = att.foundTagList.tags;
            fieldDropdown.label = property.displayName;

            return root;
        }

        private void UpdateDropdown(ChangeEvent<int> evt, DropdownFieldArgs args)
        {
            var newIndx = evt.newValue;
            args.field.value = args.info.foundTagList.tags[newIndx];
            args.field.choices = args.info.foundTagList.tags;
            args.field.label = args.info.tagName;
        }

        private static void AddErrorField(VisualElement root, string message, SerializedProperty property)
        {
            var box = UIToolkitUtility.AddErrorField(root, message);
            box.style.marginRight = new Length(1, LengthUnit.Percent);
            box.style.width = new Length(25f, LengthUnit.Percent);
            var field = new PropertyField(property);
            field.style.flexGrow = 1;
            root.Add(field);

            root.style.flexDirection = FlexDirection.Row;
            root.style.flexGrow = 1;
            root.style.alignSelf = Align.Center;
            root.style.alignItems = Align.Center;
        }
        //public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        //{
        //    if (property.propertyType != SerializedPropertyType.String)
        //    {
        //        EditorGUI.HelpBox(position, $"Field is not a string!", MessageType.Error);
        //        return;
        //    }
        //    var att = attribute as TagAttribute;
        //    att.UpdateData();
        //    att.UpdateSelectedTag(property.stringValue);
        //    if (!att.foundTagList)
        //    {
        //        EditorGUI.HelpBox(position, $"Could not find custom tags of name {att.tagName}", MessageType.Error);
        //        return;
        //    }


        //    att.selectedTag = EditorGUI.Popup(position, property.displayName, att.selectedTag, att.foundTagList.tags.ToArray());

        //    property.stringValue = att.foundTagList.tags[att.selectedTag];

        //    property.serializedObject.ApplyModifiedProperties();
        //}
    }
}

