using Spyro.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

namespace Spyro.Debug
{
    [RequireComponent(typeof(UIDocument))]
    public class Console : MonoBehaviour
    {
        private bool renderConsole;
        private VisualElement rootElement;

        private CursorLockMode cachedCursorLockedMode;
        private bool cachedCursorVisibility;

        private ConsoleOutput consoleOutput;
        private ConsoleInput consoleInput;

        private void Awake()
        {
            UIDocument doc = InitDoc();
            InitVisualElements(doc);

            consoleOutput = new ConsoleOutput(rootElement, 1000);
            consoleInput = new ConsoleInput(rootElement);

            CommandSystem.AddKeyword("$help_command_arg", (args) =>
            {
                const int printAllCommands = 1;
                var mode = (int)args[0];

                if(mode == printAllCommands)
                {
                    var result = new List<string>();
                    foreach (var (id, command) in CommandSystem.GetCommandRegistry())
                    {
                        result.Add(id);
                    }
                    return result;
                }


                return new List<string> { "command" };
                
            });

            CommandSystem.AddCommand("help", "Gives information on available commands", OnHelpCommand, new Arg("$help_command_arg"));
            CommandSystem.AddCommand("clear", "Clears the console", ClearConsole);
            CommandSystem.AddCommand("collapse", "Collapses any duplicate message in the console.", ChangeCollapseLines, new Arg(true, false));

            StartCoroutine(UpdateOutput());
        }

        private IEnumerator UpdateOutput()
        {
            yield return consoleOutput.Update();
        }

        private void OnEnable()
        {
            CommandSystem.onFailedCommandExecution += HandleFailedCommands;
            Application.logMessageReceived += HandleLog;

            consoleInput.RegisterCallbacks();
        }


        private void OnDisable()
        {
            CommandSystem.onFailedCommandExecution -= HandleFailedCommands;
            Application.logMessageReceived -= HandleLog;

            consoleInput.UnregisterCallbacks();
        }
        private bool ChangeCollapseLines(object[] args)
        {
            if (args == null || args.Length == 0)
            {
                return false;
            }
            consoleOutput.CollapseLines = (bool)args[0];
            consoleOutput.Clear();
            var msg = consoleOutput.CollapseLines ? "Console: Debug lines are now collapsed!" : "Console: Debug lines are now not collapsed!";
            consoleOutput.AddLine(msg, "", LogType.Log, DateTime.Now);
            return true;
        }

        private void HandleFailedCommands(string mainCommand, object[] args)
        {
            var msg = new StringBuilder();
            msg.Append($"Invalid command: {mainCommand}");
            if (args != null && args.Length > 0)
                foreach (var arg in args)
                {
                    msg.Append($" {arg as string}");
                }

            consoleOutput.AddLine(msg.ToString(), "", LogType.Error, DateTime.Now);
        }

        private bool ClearConsole(object[] args)
        {
            consoleOutput.Clear();
            return true;
        }

        private bool OnHelpCommand(object[] args)
        {
            var commandReg = CommandSystem.GetCommandRegistry();
            if (args == null || args.Length == 0)
            {
                var line = PrintAllCommands(commandReg);
                consoleOutput.AddLine("---Command List---", line.ToString(), LogType.Log, DateTime.Now);
                return true;
            }


            if (args[0] is string && commandReg.ContainsKey(args[0] as string))
            {
                var msg = PrintCommand(args, commandReg);
                consoleOutput.AddLine(msg, "", LogType.Log, DateTime.Now);
                return true;
            }

            return false;
        }

        private string PrintCommand(object[] args, Dictionary<string, Command> commandReg)
        {
            var id = args[0] as string;
            var command = commandReg[id];
            var definitions = PrintCommandArguments(command);
            var msg = $"{id} {definitions}: {command.desc}";
            return msg;
        }

        private StringBuilder PrintAllCommands(Dictionary<string, Command> commandReg)
        {
            var line = new StringBuilder();

            foreach (var keypair in commandReg)
            {
                var command = keypair.Value;
                var definitions = PrintCommandArguments(command);
                var msg = $"-{keypair.Key} {definitions}: {command.desc}";

                line.Append(msg);
                line.AppendLine();
            }

            return line;
        }

        private StringBuilder PrintCommandArguments(Command command)
        {
            var arguments = new StringBuilder();

            for (int i = 0; i < command.arguments.Length; i++)
            {
                var d = command.arguments[i];

                arguments.Append("[");
                PrintArgumentValues(arguments, d);
                arguments.Append("]");

                if (i != command.arguments.Length - 1)
                {
                    arguments.Append(" ");
                }
            }

            return arguments;
        }

        private static void PrintArgumentValues(StringBuilder arguments, Arg d)
        {
            var values = new StringBuilder();
            for (int i = 0; i < d.values.Length; i++)
            {
                var v = d.values[i];

                if (!CommandSystem.IsInputKeyword(v))
                {
                    values.Append(v.ToString().ToLower());
                }
                else
                {
                    var result = CommandSystem.ParseKeyword(v, 0);
                    if (result is List<string>)
                    { 
                        foreach(var line in result as List<string>)
                        {
                            values.Append(line);
                        }
                    }
                }
                if (i != d.values.Length - 1)
                {
                    values.Append("|");
                }
            }
            arguments.Append(values);
        }

        private UIDocument InitDoc()
        {
            var doc = GetComponent<UIDocument>();
            doc.visualTreeAsset = UIToolkitUtility.GetUXML("UConsole");
            doc.panelSettings = UIToolkitUtility.GetPanelSettings("ExtendedUnity_PanelSettings");
            return doc;
        }

        private void InitVisualElements(UIDocument doc)
        {
            rootElement = doc.rootVisualElement;

            renderConsole = false;
            rootElement.SetActive(renderConsole);
        }

        private void HandleLog(string condition, string stackTrace, LogType type)
        {
            //outputField.AddItem(AppendLineToTree(condition, stackTrace, type, DateTime.Now));
            consoleOutput.AddLine(condition, stackTrace, type);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tilde) || Input.inputString.Contains("\u00A7"))
            {
                renderConsole = !renderConsole;
                rootElement.SetActive(renderConsole);
                consoleInput.ResetInputField();
                if (renderConsole)
                {
                    cachedCursorLockedMode = Cursor.lockState;
                    cachedCursorVisibility = Cursor.visible;
                }
                Cursor.lockState = renderConsole ? CursorLockMode.None : cachedCursorLockedMode;
                Cursor.visible = renderConsole ? true : cachedCursorVisibility;
                consoleOutput.ActiveState = renderConsole;

            }
        }
    }
}

