using Spyro;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Spyro
{
    [CustomPropertyDrawer(typeof(TagAttribute))]
    public class CustomTagsPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if(property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.HelpBox(position, $"Field is not a string!", MessageType.Error);
                return;
            }
            var att = attribute as TagAttribute;
            att.UpdateData();
            att.UpdateSelectedTag(property.stringValue);
            if (!att.foundTagList)
            {
                EditorGUI.HelpBox(position, $"Could not find custom tags of name {att.tagName}", MessageType.Error);
                return;
            }


            att.selectedTag = EditorGUI.Popup(position, $"{att.foundTagList.name}'s Tags", att.selectedTag, att.foundTagList.tags.ToArray());

            property.stringValue = att.foundTagList.tags[att.selectedTag];

            property.serializedObject.ApplyModifiedProperties();
        }
    }
}

