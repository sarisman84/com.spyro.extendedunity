using Spyro.EditorExtensions;
using Spyro.UI;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Spyro.Editor.RecursiveScriptableObject
{
    [CustomPropertyDrawer(typeof(RecursiveAttribute))]
    public class RecursivePropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            if (property.objectReferenceValue != null
            && property.objectReferenceValue != property.serializedObject.targetObject
            && !RecursiveEditorService.Instance.IsAlreadyRendered(property.objectReferenceValue.GetInstanceID())
            )
            {
                return RenderEditor(property);
            }
            var root = new VisualElement();
            var field = new PropertyField(property);
            field.Bind(property.serializedObject);
            root.Add(field);
            return root;

        }

        private (VisualElement root, PropertyField scriptDef, ScrollView subEditor, ToolbarToggle subEditorToggle) SetupUXML()
        {
            var root = new VisualElement();
            var uxmlAsset = UIToolkitUtility.GetUXML("URecursiveScriptableEditor");
            uxmlAsset.CloneTree(root);

            var scriptDef = root.Q<PropertyField>("script_def");
            var subEditor = root.Q<ScrollView>("sub_editor");
            var subEditorToggle = root.Q<ToolbarToggle>("sub_editor_toggle");

            return (root, scriptDef, subEditor, subEditorToggle);
        }
        private void RenderSubEditor(VisualElement root, SerializedProperty property)
        {
            var serializedObject = new SerializedObject(property.objectReferenceValue);

            root.style.display = DisplayStyle.Flex;

            var element = new InspectorElement();
            element.Bind(serializedObject);
            root.Add(element);
        }
        private VisualElement RenderEditor(SerializedProperty property)
        {
            var instanceID = property.objectReferenceValue.GetInstanceID();
            RecursiveEditorService.Instance.CacheIdentifier(instanceID);
            RecursiveEditorService.Instance.CacheIdentifier(property.serializedObject.targetObject.GetInstanceID());

            var uxmlData = SetupUXML();

            uxmlData.scriptDef.bindingPath = property.propertyPath;
            uxmlData.scriptDef.Bind(property.serializedObject);

            uxmlData.subEditor.style.display = DisplayStyle.None;
            uxmlData.subEditor.verticalScroller.Adjust(0);

            uxmlData.subEditorToggle.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                var state = RecursiveEditorService.Instance.GetCachedIdentifierState(instanceID);
                state = !state;
                RecursiveEditorService.Instance.UpdateCachedIdentifierState(instanceID, state);

                uxmlData.subEditor.Clear();
                if (RecursiveEditorService.Instance.GetCachedIdentifierState(instanceID))
                {
                    RenderSubEditor(uxmlData.subEditor, property);
                }


            });
            uxmlData.subEditor.style.display = DisplayStyle.None;
            uxmlData.subEditor.verticalScroller.Adjust(0);


            if (RecursiveEditorService.Instance.GetCachedIdentifierState(instanceID))
            {
                RenderSubEditor(uxmlData.subEditor, property);
            }

            uxmlData.root.RegisterCallback<DetachFromPanelEvent>((evt) =>
            {
                RecursiveEditorService.Instance.RemoveCachedIdentifier(instanceID);
                //(evt.currentTarget as VisualElement).ClearBindings();
            });
            return uxmlData.root;
        }
    }
}



