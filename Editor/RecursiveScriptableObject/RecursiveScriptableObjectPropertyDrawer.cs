using Spyro.EditorExtensions;
using Spyro.UI;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(RecursiveAttribute))]
public class RecursiveScriptableObjectPropertyDrawer : PropertyDrawer
{
    private static Dictionary<int, bool> _activeStates = new Dictionary<int, bool>();
    private static Dictionary<int, int> _recursiveCount = new Dictionary<int, int>();
    private ToolbarToggle _subEditorToggle;
    private SerializedProperty _scriptField;
    private SubEditorArgs _subEditorArgs;
    private int _maxRenderingCount = 10;

    private static VisualElement _root;

    struct SubEditorArgs
    {
        public ScrollView currentSubEditor;
        public UnityEngine.Object currentObjectInstance;
        public ToolbarToggle currentEditButton;
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
        Debug.Log("Recursive Scripting init");

        var hashCode = !property.objectReferenceValue ? -1 : property.objectReferenceValue.GetInstanceID();
        if (!_activeStates.ContainsKey(hashCode))
        {
            _activeStates.Add(hashCode, false);
        }

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
            currentObjectInstance = property.objectReferenceValue,
            currentEditButton = subEditorToggle
        };

        subEditorToggle.RegisterCallback<ChangeEvent<bool>, SubEditorArgs>((evt, args) => ViewSubEditor(evt, args, hashCode), _subEditorArgs);
        subEditor.style.display = DisplayStyle.None;
        subEditor.verticalScroller.Adjust(0);

        _subEditorToggle = subEditorToggle;
        _scriptField = property;

        ViewSubEditor(_subEditorArgs, hashCode);

        return root;
    }

    private void OnPropertyUpdated(SerializedPropertyChangeEvent evt)
    {
        _subEditorToggle.style.display = _scriptField.objectReferenceValue != null ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void ViewSubEditor(SubEditorArgs args, int hashCode)
    {
        args.currentSubEditor.Clear();
        if (!_activeStates[hashCode] || args.currentObjectInstance == null)
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
    private void ViewSubEditor(ChangeEvent<bool> env, SubEditorArgs args, int hashCode)
    {
        _activeStates[hashCode] = !_activeStates[hashCode];
        ViewSubEditor(args, hashCode);
    }
}
