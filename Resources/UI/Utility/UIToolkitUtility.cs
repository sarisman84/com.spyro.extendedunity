using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Spyro.UI
{
    public static class UIToolkitUtility
    {
        public static VisualTreeAsset GetUXML(string path)
        {
#if UNITY_EDITOR
            var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"Packages/com.spyro.extendedunity/Resources/UI/UXML/{path}.uxml");
#else
            var asset = Resources.Load<VisualTreeAsset>($"UI/UXML/{path}");
#endif



            if (asset == null)
            {
                throw new System.NullReferenceException($"Could not find asset at path: Packages/com.spyro.extendedunity/Resources/UI/UXML/{path}");
            }

            return asset;
        }

        public static PanelSettings GetPanelSettings(string path)
        {


#if UNITY_EDITOR
            var settings = AssetDatabase.LoadAssetAtPath<PanelSettings>($"Packages/com.spyro.extendedunity/Resources/UI/{path}.asset");
#else
            var settings = Resources.Load<PanelSettings>($"UI/{path}");
#endif


            if (settings == null)
            {
                throw new System.NullReferenceException($"Could not find settings at path: Packages/com.spyro.extendedunity/Resources/UI/{path}");
            }
            return settings;
        }

        public static HelpBox AddErrorField(VisualElement root, string message)
        {
            var messageBox = new HelpBox(message, HelpBoxMessageType.Error);
            root.Add(messageBox);
            return messageBox;
        }

        public static HelpBox AddErrorWithTooltip(VisualElement root, string message, string tooltipMessage)
        {
            var messageBox = new HelpBox(message, HelpBoxMessageType.Error);
            messageBox.tooltip = tooltipMessage;
            root.Add(messageBox);
            return messageBox;
        }


        public static void SetActive(this VisualElement element, bool active)
        {
            element.style.display = active ? DisplayStyle.Flex : DisplayStyle.None;
            element.SetEnabled(active);
        }
    }
}

