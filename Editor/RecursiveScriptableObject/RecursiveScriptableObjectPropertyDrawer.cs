using Spyro.EditorExtensions;
using Spyro.UI;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

[CustomPropertyDrawer(typeof(RecursiveAttribute))]
public class RecursiveScriptableObjectPropertyDrawer : PropertyDrawer
{
    private ToolbarToggle _subEditorToggle;
    private SerializedProperty _scriptField;
    struct SubEditorArgs
    {
        public ScrollView currentSubEditor;
        public UnityEngine.Object currentObjectInstance;
        public ToolbarToggle currentEditButton;
    }
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        var root = new VisualElement();

        var uxmlAsset = UIToolkitUtility.GetUXML("URecursiveScriptableEditor");  /*AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.spyro.extendedunity/UI/URecursiveScriptableEditor.uxml");*/
        uxmlAsset.CloneTree(root);

        var scriptDef = root.Q<PropertyField>("script_def");
        var subEditor = root.Q<ScrollView>("sub_editor");
        var subEditorToggle = root.Q<ToolbarToggle>("sub_editor_toggle");

        scriptDef.bindingPath = property.propertyPath;
        scriptDef.RegisterValueChangeCallback(OnPropertyUpdated);

        var args = new SubEditorArgs
        {
            currentSubEditor = subEditor,
            currentObjectInstance = property.objectReferenceValue,
            currentEditButton = subEditorToggle
        };
        subEditorToggle.RegisterCallback<ChangeEvent<bool>, SubEditorArgs>(ViewSubEditor, args);
        subEditor.style.display = DisplayStyle.None;
        subEditor.verticalScroller.Adjust(0);

        _subEditorToggle = subEditorToggle;
        _scriptField = property;
        return root;
    }

    private void OnPropertyUpdated(SerializedPropertyChangeEvent evt)
    {
        _subEditorToggle.style.display = _scriptField.objectReferenceValue != null ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void ViewSubEditor(ChangeEvent<bool> evt, SubEditorArgs args)
    {
        var active = evt.newValue;
        args.currentSubEditor.Clear();
        if (!active || args.currentObjectInstance == null)
        {
            args.currentSubEditor.verticalScroller.Adjust(0);
            args.currentSubEditor.style.display = DisplayStyle.None;
            args.currentSubEditor.style.display = args.currentObjectInstance != null ? DisplayStyle.Flex : DisplayStyle.None;
            return;
        }
        args.currentSubEditor.style.display = DisplayStyle.Flex;
        //Traverse through the property's children if it contains an entry and add them to the sub editor object.
        var serializedObject = new SerializedObject(args.currentObjectInstance);
        foreach (var child in serializedObject.FindVisibleChildProperties())
        {
            if (child.name == "m_Script" || child.propertyPath == "m_Script")
                continue;
            var propertyField = new PropertyField(child);
            propertyField.Bind(serializedObject);
            propertyField.style.width = new Length(99.6f, LengthUnit.Percent);
            args.currentSubEditor.Add(propertyField);
        }
    }
}
