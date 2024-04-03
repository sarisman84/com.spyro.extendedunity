using Spyro.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(RecursiveAttribute))]
public class RecursiveScriptableObjectPropertyDrawer : PropertyDrawer
{
    struct SubEditorArgs
    {
        public ScrollView currentSubEditor;
        public UnityEngine.Object currentObjectInstance;
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

        var args = new SubEditorArgs
        {
            currentSubEditor = subEditor,
            currentObjectInstance = property.objectReferenceValue
        };
        subEditorToggle.RegisterCallback<ChangeEvent<bool>, SubEditorArgs>(ViewSubEditor, args);
        subEditor.style.display = DisplayStyle.None;
        subEditor.verticalScroller.Adjust(0);
        return root;
    }

    private void ViewSubEditor(ChangeEvent<bool> evt, SubEditorArgs args)
    {
        var active = evt.newValue;
        args.currentSubEditor.Clear();
        if (!active || args.currentObjectInstance == null)
        {
            args.currentSubEditor.verticalScroller.Adjust(0);
            args.currentSubEditor.style.display = DisplayStyle.None;
            return;
        }

       
        args.currentSubEditor.style.display = DisplayStyle.Flex;

        //Traverse through the property's children if it contains an entry and add them to the sub editor object.
        var serializedObject = new SerializedObject(args.currentObjectInstance);
        var propertyIterator = serializedObject.GetIterator();

        while (propertyIterator.NextVisible(true))
        {
            // Exclude script field to prevent displaying it
            if (propertyIterator.name == "m_Script" || propertyIterator.propertyPath == "m_Script")
                continue;

            var propertyField = new PropertyField(propertyIterator);
            propertyField.Bind(serializedObject);
            propertyField.style.width = new Length(99.6f, LengthUnit.Percent);
            args.currentSubEditor.Add(propertyField);
        }

    }


    //private bool isExpanded = false;
    //public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    //{
    //    var ogPos = position;
    //    // Begin property
    //    position = DrawMainProperty(position, property, label);
    //    // Draw the array opener element
    //    if (property.objectReferenceValue != null)
    //        DrawArrayOpener(ogPos);
    //    if (isExpanded)
    //        TryRenderContents(position, property);

    //    // End property
    //    EditorGUI.EndProperty();
    //}

    //private void DrawArrayOpener(Rect position)
    //{
    //    Rect openerRect = new Rect(position.x + EditorGUIUtility.labelWidth - 15.0f, position.y, 20f, EditorGUIUtility.singleLineHeight);
    //    isExpanded = EditorGUI.Toggle(openerRect, isExpanded, EditorStyles.foldout);

    //    // Adjust position for property fields
    //    position.x += 20f;
    //    position.width -= 20f;
    //}

    //private void TryRenderContents(Rect position, SerializedProperty property)
    //{
    //    var cachedPath = string.Empty;
    //    if (property.objectReferenceValue != null)
    //    {
    //        EditorGUI.indentLevel++;
    //        // Get the serialized object of the target property
    //        var serializedObject = new SerializedObject(property.objectReferenceValue);

    //        // Get the iterator for the serialized properties of the target property
    //        var iterator = serializedObject.GetIterator();

    //        // Move to the first property
    //        bool enterChildren = true;
    //        if (iterator.NextVisible(enterChildren))
    //        {
    //            // Iterate through all properties
    //            do
    //            {
    //                if (iterator.propertyPath.Contains(cachedPath) && !string.IsNullOrEmpty(cachedPath))
    //                {
    //                    continue;
    //                }

    //                if (IsMultidimentionalValue(iterator))
    //                {
    //                    cachedPath = iterator.propertyPath;
    //                }
    //                else
    //                {
    //                    cachedPath = string.Empty;
    //                }
    //                // Draw the property field within the box
    //                DrawProperty(property, iterator, position);
    //                position.y += EditorGUI.GetPropertyHeight(iterator, true) * 1.15f;
    //            }
    //            while (iterator.NextVisible(enterChildren));
    //        }

    //        // Apply modifications
    //        serializedObject.ApplyModifiedProperties();
    //        EditorGUI.indentLevel--;
    //    }
    //}

    //private Rect DrawMainProperty(Rect position, SerializedProperty property, GUIContent label)
    //{
    //    position.height = GetPropertyHeight(property, label);
    //    EditorGUI.BeginProperty(position, label, property);

    //    position.height = EditorGUIUtility.singleLineHeight;
    //    // Draw the main property field
    //    EditorGUI.PropertyField(position, property, label);
    //    position.y += position.height * 1.15f;

    //    var alt = position;
    //    alt.y -= 2.0f;
    //    alt.height = isExpanded ?
    //        GetPropertyHeight(property, label) - EditorGUIUtility.singleLineHeight + 2.0f :
    //        GetPropertyHeight(property, label) - EditorGUIUtility.singleLineHeight;
    //    alt.width += 2.0f;
    //    GUI.Box(EditorGUI.IndentedRect(alt), GUIContent.none, EditorStyles.helpBox);
    //    return position;
    //}

    //private bool IsMultidimentionalValue(SerializedProperty iterator)
    //{
    //    return
    //        iterator.propertyType == SerializedPropertyType.Vector2 ||
    //        iterator.propertyType == SerializedPropertyType.Vector3 ||
    //        iterator.propertyType == SerializedPropertyType.Vector4 ||
    //        iterator.propertyType == SerializedPropertyType.Quaternion;
    //}

    //private void DrawProperty(SerializedProperty self, SerializedProperty nextProp, Rect pos)
    //{
    //    using (new EditorGUI.DisabledGroupScope(nextProp.propertyPath == "m_Script"))
    //    {
    //        EditorGUI.PropertyField(pos, nextProp, IsNotTheSameObject(self, nextProp));
    //    }

    //}

    //private static bool IsNotTheSameObject(SerializedProperty self, SerializedProperty nextProp)
    //{
    //    var isObjectType =
    //                nextProp.propertyType == SerializedPropertyType.ObjectReference;
    //    var renderChildren = isObjectType && self.objectReferenceValue != nextProp.objectReferenceValue;
    //    return renderChildren;
    //}

    //public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    //{
    //    if (property.serializedObject == null)
    //    {
    //        return base.GetPropertyHeight(property, label);
    //    }

    //    var totalHeight = CalculateFullHeight(property, label);

    //    return totalHeight;
    //}

    //private float CalculateFullHeight(SerializedProperty property, GUIContent label)
    //{
    //    var totalHeight = base.GetPropertyHeight(property, label);
    //    var cachedPath = string.Empty;
    //    // Check if the property is not null and is an object reference
    //    if (property.objectReferenceValue != null && isExpanded)
    //    {
    //        // Get the serialized object of the target property
    //        var serializedObject = new SerializedObject(property.objectReferenceValue);

    //        // Get the iterator for the serialized properties of the target property
    //        var iterator = serializedObject.GetIterator();

    //        // Move to the first property
    //        bool enterChildren = true;
    //        if (iterator.NextVisible(enterChildren))
    //        {
    //            // Check if the property is an array or a struct
    //            bool isArrayOrStruct = iterator.isArray || iterator.propertyType == SerializedPropertyType.Generic;

    //            // If it's an array or struct, use the default height calculation
    //            if (isArrayOrStruct)
    //            {
    //                totalHeight += EditorGUIUtility.singleLineHeight * 1.15f;
    //            }
    //            else
    //            {
    //                IterateThroughPropertyAndCalculateTotalHeight(property, ref totalHeight, ref cachedPath, iterator, enterChildren);
    //            }
    //        }
    //    }

    //    return totalHeight;
    //}

    //private void IterateThroughPropertyAndCalculateTotalHeight(SerializedProperty property, ref float totalHeight, ref string cachedPath, SerializedProperty iterator, bool enterChildren)
    //{
    //    // Iterate through all properties
    //    do
    //    {
    //        var skip = (iterator.propertyPath.Contains(cachedPath) && !string.IsNullOrEmpty(cachedPath));

    //        if (skip)
    //        {
    //            continue;
    //        }

    //        if (IsMultidimentionalValue(iterator))
    //        {
    //            cachedPath = iterator.propertyPath;
    //        }
    //        else
    //        {
    //            cachedPath = string.Empty;
    //        }

    //        // Accumulate the height of each property
    //        totalHeight += EditorGUI.GetPropertyHeight(iterator, true) * 1.15f;
    //    }
    //    while (iterator.NextVisible(enterChildren));
    //}
}
