using Spyro.EditorExtensions;
using Spyro.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

[CustomPropertyDrawer(typeof(RecursiveAttribute))]
public class RecursiveScriptableObjectPropertyDrawer : PropertyDrawer
{
    struct SubEditorArgs
    {
        public ScrollView currentSubEditor;
        public SerializedProperty currentProperty;
        public ToolbarToggle currentEditButton;
        public SerializedObject currentOwner;
    }

    struct SubEditorInfo
    {
        public bool activeState;
        public Color backgroundColor;
    }

    private static Dictionary<int, SubEditorInfo> _editorInfo = new Dictionary<int, SubEditorInfo>();
    private ToolbarToggle _subEditorToggle;
    private SerializedProperty _scriptField;
    private SubEditorArgs _subEditorArgs;
    private int _maxRenderingCount = 10;

    private const int INVALID_KEY = -1;

    public Color GenerateRandomColor()
    {
        Color color = Random.ColorHSV(1.0f, 1.0f, 1.0f, 1.0f, 0.15f, 0.85f, 1.0f, 1.0f);

        return color;
    }

    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        // if (_root == null)
        // {
        //     _root = InitializeProperty(property);
        // }
        return InitializeProperty(property);
    }

    private VisualElement InitializeProperty(SerializedProperty property)
    {
        var hashCode = !property.objectReferenceValue ? -1 : property.objectReferenceValue.GetInstanceID();
        var root = new VisualElement();

        var uxmlAsset = UIToolkitUtility.GetUXML("URecursiveScriptableEditor");  /*AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.spyro.extendedunity/UI/URecursiveScriptableEditor.uxml");*/
        uxmlAsset.CloneTree(root);

        var scriptDef = root.Q<PropertyField>("script_def");
        var subEditor = root.Q<ScrollView>("sub_editor");
        var subEditorToggle = root.Q<ToolbarToggle>("sub_editor_toggle");

        scriptDef.bindingPath = property.propertyPath;
        scriptDef.Bind(property.serializedObject);
        scriptDef.RegisterValueChangeCallback(OnPropertyUpdated);

        _subEditorArgs = new SubEditorArgs
        {
            currentSubEditor = subEditor,
            currentProperty = property,
            currentEditButton = subEditorToggle,
            currentOwner = property.serializedObject
        };

        subEditorToggle.RegisterCallback<ChangeEvent<bool>, SubEditorArgs>(ViewSubEditor, _subEditorArgs);
        subEditor.style.display = DisplayStyle.None;
        subEditor.verticalScroller.Adjust(0);

        var key = GetKey(_subEditorArgs);

        if (IsKeyValid(key))
        {
            var info = _editorInfo[key];
            subEditor.style.backgroundColor = info.backgroundColor;
        }

        _subEditorToggle = subEditorToggle;
        _scriptField = property;

        ViewSubEditor(_subEditorArgs);

        root.RegisterCallback<DetachFromPanelEvent>(EditorCleanup);

        return root;
    }

    private void EditorCleanup(DetachFromPanelEvent evt)
    {
        VisualElement root = evt.currentTarget as VisualElement;
        root.ClearBindings();


    }

    private void OnPropertyUpdated(SerializedPropertyChangeEvent evt)
    {
        _subEditorToggle.style.display = _scriptField.objectReferenceValue != null ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private PropertyField DrawProperty(SerializedProperty property, SerializedObject owner)
    {
        var propertyField = new PropertyField(property);
        propertyField.Bind(owner);
        propertyField.style.width = new Length(95.0f, LengthUnit.Percent);
        return propertyField;
    }

    private void ViewSubEditor(SubEditorArgs args)
    {
        args.currentSubEditor.Clear();
        if (!CanViewSubEditor(args))
        {
            args.currentSubEditor.verticalScroller.Adjust(0);
            args.currentSubEditor.style.display = DisplayStyle.None;
            args.currentSubEditor.style.display = args.currentProperty.objectReferenceValue != null ? DisplayStyle.Flex : DisplayStyle.None;
            return;
        }

        if (!args.currentProperty.objectReferenceValue)
        {
            return;
        }

        var serializedObject = new SerializedObject(args.currentProperty.objectReferenceValue);
        args.currentSubEditor.style.display = DisplayStyle.Flex;


        var element = new InspectorElement();
        element.Bind(serializedObject);
        args.currentSubEditor.Add(element);
    }

    private int GetKey(SubEditorArgs args)
    {
        if (!args.currentProperty.objectReferenceValue)
        {
            return INVALID_KEY;
        }

        var instanceID = args.currentProperty.objectReferenceValue.GetInstanceID();
        var hashCode = args.currentProperty.name.GetHashCode();

        var key = instanceID + hashCode;
        return key;
    }

    private bool IsKeyValid(int key)
    {
        if (key == INVALID_KEY)
        {
            return false;
        }

        return _editorInfo.ContainsKey(key);
    }

    private bool CanViewSubEditor(SubEditorArgs args)
    {
        if (!args.currentProperty.objectReferenceValue)
        {
            return false;
        }


        var key = GetKey(args);
        _editorInfo.TryAdd(key, new SubEditorInfo
        {
            activeState = false,
            backgroundColor = GenerateRandomColor()
        });

        return _editorInfo[key].activeState;

    }

    private void ViewSubEditor(ChangeEvent<bool> env, SubEditorArgs args)
    {
        var key = GetKey(args);
        if (_editorInfo.ContainsKey(key))
        {
            var temp = _editorInfo[key];
            temp.activeState = !_editorInfo[key].activeState;
            _editorInfo[key] = temp;
        }

        ViewSubEditor(args);
        //Debug.Log($"Editor {hashCode} is {(_activeStates[hashCode] ? "Visible" : "Not Visible")}");
    }
}
