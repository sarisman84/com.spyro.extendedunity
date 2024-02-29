using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Spyro
{
    public static class CursorManager
    {
        private static PropertyInfo textFieldTextEditorProperty;
        private static object textFieldTextEditorInstance;
        private static FieldInfo editorTextIndexField;

        public static void MoveCursorToEnd(string input)
        {
            // Get the TextEditor instance
            TextEditor te = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
            if (te != null)
            {
                te.MoveTextEnd();
            }
        }
    }
}
