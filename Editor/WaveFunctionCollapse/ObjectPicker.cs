using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Spyro.ProcedualGeneration
{
    public class ObjectPicker
    {
        private const float previewWidth = 50.0f;
        private const float previewHeight = 50.0f;


        private ToolbarSearchField searchBar;
        private VisualElement rootVisualElement;
        private LevelGenerationData data;
        private LevelGenerationEditor rootEditor;


        public event Action<string> onObjectSelect;

        public ObjectPicker(LevelGenerationEditor rootEditor)
        {
            searchBar = new ToolbarSearchField();
            searchBar.style.alignSelf = Align.Center;
            searchBar.style.height = 15.0f;
            searchBar.style.width = rootEditor.position.width - 15.0f;
            searchBar.RegisterValueChangedCallback(OnSearchEntryModified);
            rootEditor.rootVisualElement.Add(searchBar);


            rootVisualElement = new VisualElement();
            rootVisualElement.style.flexDirection = FlexDirection.Row;
            rootVisualElement.style.flexWrap = Wrap.Wrap;
            rootEditor.rootVisualElement.Add(rootVisualElement);

            data = rootEditor.Data;
            DisplayListOfPrefabs();

            EditorApplication.update += UpdateSearchBarSize;
            this.rootEditor = rootEditor;
        }

        private void UpdateSearchBarSize()
        {
            searchBar.style.width = rootEditor.position.width - 15.0f;
        }

        ~ObjectPicker()
        {
            EditorApplication.update -= UpdateSearchBarSize;
        }

        private void DisplayListOfPrefabs(string searchTerms = "")
        {
            rootVisualElement.Clear();

            var prefabs = AssetDatabase.FindAssets("t:prefab");

            foreach (var guid in prefabs)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains(searchTerms))
                {
                    SetPreview(path, data.gameObjectData.Any(x => path.Contains(x.name)));
                }
            }
        }

        private void SetPreview(string prefabPath, bool hasBeenSelected)
        {
            var previewButton = new Button();
            previewButton.style.backgroundImage = GetPrefabPreview(prefabPath);
            previewButton.RegisterCallback<ClickEvent, string>(OnPreviewButtonPressed, prefabPath);
            if (hasBeenSelected)
                previewButton.style.backgroundColor = Color.yellow;

            previewButton.style.width = previewWidth;
            previewButton.style.height = previewHeight;
            previewButton.tooltip = prefabPath;
            rootVisualElement.Add(previewButton);
        }

        private void OnPreviewButtonPressed(ClickEvent evt, string prefabPath)
        {
            onObjectSelect?.Invoke(prefabPath);
        }

        private void OnSearchEntryModified(ChangeEvent<string> evt)
        {
            DisplayListOfPrefabs(evt.newValue);
        }


        static Texture2D GetPrefabPreview(string path)
        {
            Debug.Log("Generate preview for " + path);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var editor = UnityEditor.Editor.CreateEditor(prefab);
            Texture2D tex = editor.RenderStaticPreview(path, null, 200, 200);
            EditorWindow.DestroyImmediate(editor);
            return tex;
        }
    }
}

