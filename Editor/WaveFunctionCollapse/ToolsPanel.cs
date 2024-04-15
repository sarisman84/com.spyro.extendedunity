using Spyro.UI;
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Spyro.ProcedualGeneration
{
    public class ToolsPanel
    {
        private const float buttonWidth = 25.0f;
        private const float buttonHeight = 25.0f;

        private VisualElement rootVisualElement;
        public ToolsPanel(VisualElement root)
        {
            rootVisualElement = new VisualElement();
            rootVisualElement.style.flexDirection = FlexDirection.Row;
            rootVisualElement.style.flexWrap = Wrap.Wrap;
            rootVisualElement.style.height = buttonHeight;
            root.Add(rootVisualElement);
        }

        public void AddButton(string icon, Action buttonEvent)
        {
            var button = new ToolbarButton(buttonEvent);
            button.style.backgroundImage = new StyleBackground(EditorGUIUtility.IconContent(icon).image as Texture2D);
            button.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
            button.style.flexGrow = 1;
            button.style.flexShrink = 0;
            button.style.width = buttonWidth;
            button.style.height = buttonHeight;
            rootVisualElement.Add(button);
        }

    }
}