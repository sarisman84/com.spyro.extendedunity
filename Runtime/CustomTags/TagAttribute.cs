using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Spyro
{
    public class TagAttribute : PropertyAttribute
    {
        public string tagName;
        public CustomTag foundTagList;
        public int selectedTag;
        public TagAttribute(string tagName)
        {
            this.tagName = tagName;
            selectedTag = -1;
        }

        public void UpdateData()
        {
            var foundTags = AssetDatabase.FindAssets("t:CustomTag");
            foreach (var foundTagGUID in foundTags)
            {
                var path = AssetDatabase.GUIDToAssetPath(foundTagGUID);
                if (path.ToLower().Contains(tagName.ToLower()))
                {
                    foundTagList = AssetDatabase.LoadAssetAtPath<CustomTag>(path);
                }
            }
        }

        public void UpdateSelectedTag(string value)
        {
            if (selectedTag != -1 || !foundTagList || string.IsNullOrEmpty(value))
                return;

            selectedTag = Mathf.Max(0, foundTagList.tags.IndexOf(value));


        }
    }
}

