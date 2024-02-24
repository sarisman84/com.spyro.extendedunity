using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Spyro.Debug
{
    public class Console : MonoBehaviour
    {
        struct ConsoleLog
        {
            public int count;
            public DateTime date;
            public LogType type;
            public GUIStyle style;
        }

        private string userInput;
        private bool renderConsole;
        private Vector2 consoleScroll;
        private int linesToSave = 35;

        private Dictionary<string, ConsoleLog> consoleLines;
        private void Awake()
        {
            consoleLines = new Dictionary<string, ConsoleLog>();
            CommandSystem.AddCommand("help", "Gives information on available commands", "help,help <command>", OnHelpCommand);
            CommandSystem.AddCommand("clear", "Clears the console", "clear", ClearConsole);

            Application.logMessageReceived += HandleLog;
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= HandleLog;
        }

        private void HandleLog(string condition, string stackTrace, LogType type)
        {
#if !UNITY_EDITOR
            var b = new StringBuilder();
            b.AppendLine($"{condition}");

            var lines = stackTrace.Split('\n');
            foreach (var line in lines)
            {
                var l = line.Replace("\n", "").Replace(" ", "");
                if (!string.IsNullOrEmpty(l))
                {
                    b.AppendLine($">{l}");
                }
            }

            AppendLine(ref consoleLines, b.ToString(), type);
#endif
        }

        private void AppendLine(ref Dictionary<string, ConsoleLog> list, string input, LogType type)
        {

            if (list.ContainsKey(input))
            {
                var data = list[input];
                data.count++;
                data.date = DateTime.Now;
                list[input] = data;
                return;
            }

            if (list.Count > linesToSave)
            {
                list.Remove(list.Keys.ElementAt(0));
            }
            var bgColor = Color.black;
            bgColor.a = 0.75f;
            var inputHeight = 40.0f;
            var style = GetFontStyle(inputHeight / 2.0f, bgColor);
            list.Add(input, new ConsoleLog { count = 0, date = DateTime.Now, type = type, style = style });
        }

        private void ClearConsole(object[] obj)
        {
            consoleLines.Clear();
        }

        private void OnHelpCommand(object[] args)
        {
            var cmd = CommandSystem.GetCommandRegistry();
            if (args.Length == 0)
            {

                foreach (var (id, command) in cmd)
                {
                    AppendLine(ref consoleLines, $"Cmd:[{command.definition}] {command.desc}", LogType.Log);
                }
                return;
            }

            var arg = args[0] as string;

            if (string.IsNullOrEmpty(arg) || !cmd.ContainsKey(arg))
            {
                AppendLine(ref consoleLines, $"Could not find command: {arg}", LogType.Error);
                return;
            }
            AppendLine(ref consoleLines, $"Cmd:[{cmd[arg].definition}] {cmd[arg].desc}", LogType.Log);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tilde) || Input.inputString.Contains("\u00A7"))
            {
                renderConsole = !renderConsole;
                if(!renderConsole)
                {
                    userInput = string.Empty;
                    consoleLines.Clear();
                }
            }
        }
        private void OnGUI()
        {
            if (!renderConsole)
                return;

            var bgColor = Color.black;
            bgColor.a = 0.75f;
            var inputHeight = 40.0f;
            var style = GetFontStyle(inputHeight / 2.0f, bgColor);
            GUI.SetNextControlName("console");
            userInput = GUILayout.TextField(userInput, style, GUILayout.Width(Screen.width), GUILayout.Height(inputHeight));
            GUI.FocusControl("console");

            //string[] lines = consoleOutput.Length == 0 ? default : consoleOutput.ToString().Split('\n');
            consoleScroll = ViewLines(consoleScroll, consoleLines, style);

            TryApplyingCommand();
        }

        private Vector2 ViewLines(Vector2 scroll, Dictionary<string, ConsoleLog> lines, GUIStyle style)
        {
            scroll = GUILayout.BeginScrollView(scroll, GUILayout.Width(Screen.width), GUILayout.Height(240.0f));
            PrintLines(lines);
            GUILayout.EndScrollView();

            return scroll;
        }

        private void PrintLines(Dictionary<string, ConsoleLog> lines)
        {
            foreach (var (line, data) in lines)
            {
                switch (data.type)
                {
                    case LogType.Error:
                        data.style.normal.textColor = Color.red;
                        break;
                    case LogType.Assert:
                        data.style.normal.textColor = Color.blue;
                        break;
                    case LogType.Warning:
                        data.style.normal.textColor = Color.yellow;
                        break;
                    case LogType.Log:
                        data.style.normal.textColor = Color.white;
                        break;
                    case LogType.Exception:
                        data.style.normal.textColor = Color.red;
                        break;
                    default:
                        break;
                }

                var sublines = line.Split('\n');
                for (int i = 0; i < sublines.Length; i++)
                {
                    var sl = sublines[i];
                    if (i == 0)
                        GUILayout.Label($"[{data.date.Hour}:{data.date.Minute}:{data.date.Second}][{data.type}] {sl} {(data.count > 0 ? $"({data.count})" : "")}", data.style);
                    else
                        GUILayout.Label($"{sl}", data.style);
                }
            }
        }

        private void TryApplyingCommand()
        {
            if (Event.current.character == '\n' && Event.current.type == EventType.KeyDown)
            {
                var input = userInput.Split(' ');
                var args = new string[input.Length - 1];
                for (int i = 1; i < input.Length; i++)
                {
                    args[i - 1] = input[i];
                }
                userInput = "";
                CommandSystem.Execute(input[0], args);
            }
        }

        private static GUIStyle GetFontStyle(float height, Color backgroundColor)
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = Mathf.CeilToInt(height);
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.MiddleLeft;
            style.contentOffset = new Vector2(style.contentOffset.x + 5.0f, style.contentOffset.y);

            // Set the background color
            Texture2D backgroundTexture = new Texture2D(1, 1);
            backgroundTexture.SetPixel(0, 0, backgroundColor);
            backgroundTexture.Apply();
            style.normal.background = backgroundTexture;
            return style;
        }

    }
}

