using Spyro.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace Spyro.Debug
{
    [RequireComponent(typeof(UIDocument))]
    public class Console : MonoBehaviour
    {
        struct DebugLine
        {
            public string header;
            public string body;
            public Color color;
            public DateTime timePoint;
            public LogType type;
            public int count;
        }

        private bool renderConsole;
        private bool collapseLines;
        private bool addAutoCompleteOption;
        private int argumentCounter;

        private TextField userInputField;
        private ListView outputField;
        private ListView autoCompleteField;
        private VisualElement rootElement;

        private List<DebugLine> lines = new List<DebugLine>(1000);
        private List<string> autoCompleteOptions = new List<string>();

        private void Awake()
        {
            collapseLines = false;
            InitLines();
            UIDocument doc = InitDoc();
            InitVisualElements(doc);

            CommandSystem.AddKeyword("$help_command_arg", (args) =>
            {
                var strBuilder = args[0] as StringBuilder;
                strBuilder.Append("command");
            });

            CommandSystem.AddCommand("help", "Gives information on available commands", OnHelpCommand, new Arg("$help_command_arg"));
            CommandSystem.AddCommand("clear", "Clears the console", ClearConsole);
            CommandSystem.AddCommand("collapse", "Collapses any duplicate message in the console.", ChangeCollapseLines, new Arg(true, false));
        }

        private void OnEnable()
        {
            CommandSystem.onFailedCommandExecution += HandleFailedCommands;
            Application.logMessageReceived += HandleLog;
            //Ended up using the KeyDownEvent callback
            userInputField.RegisterCallback<KeyDownEvent>(ParsingInput, TrickleDown.TrickleDown);
            userInputField.RegisterCallback<ChangeEvent<string>>(OnInputChanged, TrickleDown.TrickleDown);
            userInputField.RegisterCallback<NavigationMoveEvent>(OnInputMove, TrickleDown.TrickleDown);
        }


        private void OnDisable()
        {
            CommandSystem.onFailedCommandExecution -= HandleFailedCommands;
            Application.logMessageReceived -= HandleLog;
            userInputField.UnregisterCallback<KeyDownEvent>(ParsingInput, TrickleDown.TrickleDown);
            userInputField.UnregisterCallback<ChangeEvent<string>>(OnInputChanged, TrickleDown.TrickleDown);
            userInputField.UnregisterCallback<NavigationMoveEvent>(OnInputMove, TrickleDown.TrickleDown);
        }

        private void OnInputMove(NavigationMoveEvent evt)
        {
            InteruptDefaultEvent(evt);
        }

        private bool ChangeCollapseLines(object[] args)
        {
            if (args == null || args.Length == 0)
            {
                return false;
            }
            collapseLines = (bool)args[0];
            outputField.Rebuild();
            var msg = collapseLines ? "Console: Debug lines are now collapsed!" : "Console: Debug lines are now not collapsed!";
            ParseDebugLine(msg, "", LogType.Log, DateTime.Now);
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

            ParseDebugLine(msg.ToString(), "", LogType.Error, DateTime.Now);
        }

        private bool ClearConsole(object[] args)
        {
            RebuildLines();
            return true;
        }

        private bool OnHelpCommand(object[] args)
        {
            var commandReg = CommandSystem.GetCommandRegistry();
            if (args == null || args.Length == 0)
            {
                var line = PrintAllCommands(commandReg);
                ParseDebugLine("---Command List---", line.ToString(), LogType.Log, DateTime.Now);
                return true;
            }


            if (args[0] is string && commandReg.ContainsKey(args[0] as string))
            {
                var msg = PrintCommand(args, commandReg);
                ParseDebugLine(msg, "", LogType.Log, DateTime.Now);
                return true;
            }

            return false;
        }

        private string PrintCommand(object[] args, Dictionary<string, Command> commandReg)
        {
            var id = args[0] as string;
            var command = commandReg[id];
            var definitions = PrintCommandArguments(command);
            var msg = $"{id} {definitions} - {command.desc}";
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

                if (!CommandSystem.TryParseKeyword(v, values))
                {
                    values.Append(v.ToString().ToLower());
                }
                if (i != d.values.Length - 1)
                {
                    values.Append("|");
                }
            }
            arguments.Append(values);
        }

        private void InitLines()
        {
            if (lines.Count == 0)
            {
                for (var i = 0; i < lines.Capacity; ++i)
                {
                    lines.Add(new DebugLine());
                }
                return;
            }
        }

        private void RebuildLines()
        {
            for (var i = 0; i < lines.Capacity; ++i)
            {
                lines[i] = default;
            }
            outputField.Rebuild();
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

            userInputField = rootElement.Q<TextField>("input_field");
            userInputField.value = string.Empty;

            outputField = rootElement.Q<ListView>("output_field");
            outputField.itemsSource = lines;
            outputField.makeItem = () => new Label();
            outputField.bindItem = BindLine;
            outputField.Q<ScrollView>().verticalScrollerVisibility = ScrollerVisibility.Hidden;

            autoCompleteField = rootElement.Q<ListView>("auto_complete_list");
            autoCompleteField.itemsSource = autoCompleteOptions;
            autoCompleteField.makeItem = () => new Label();
            autoCompleteField.bindItem = BindAutoComplete;
            autoCompleteField.Q<Label>(null, BaseListView.emptyLabelUssClassName).text = string.Empty;
            autoCompleteField.Q<ScrollView>().verticalScrollerVisibility = ScrollerVisibility.Hidden;
            autoCompleteField.selectedIndex = 0;
        }

        private void BindAutoComplete(VisualElement element, int index)
        {
            var label = element as Label;
            label.text = autoCompleteOptions[index];
        }

        private void BindLine(VisualElement element, int index)
        {
            var label = element as Label;
            var line = lines[index];

            var counter = line.count > 0 && collapseLines ? $"[x{line.count}]" : "";

            label.text = $"[{line.type}][{GetTimePoint(line.timePoint)}]: {line.header} {counter}\n{line.body}";
            label.style.color = line.color;
        }

        private void ParsingInput(KeyDownEvent evt)
        {
            var hasSubmitted = (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter || evt.character == '\n');
            var hasTabbed = (evt.keyCode == KeyCode.Tab);
            var isSelectingAutoCompleteOptions = (evt.keyCode == KeyCode.UpArrow || evt.keyCode == KeyCode.DownArrow);

            if (hasSubmitted || hasTabbed || isSelectingAutoCompleteOptions)
            {
                InteruptDefaultEvent(evt);
            }

            if (string.IsNullOrEmpty(userInputField.value))
            {
                return;
            }

            if (isSelectingAutoCompleteOptions)
            {
                var offset = evt.keyCode == KeyCode.UpArrow ? 1 : -1;
                autoCompleteField.selectedIndex += offset;
                autoCompleteField.selectedIndex =
                    autoCompleteField.selectedIndex < 0 ? autoCompleteOptions.Count - 1 :
                    autoCompleteField.selectedIndex > autoCompleteOptions.Count - 1 ? 0 : autoCompleteField.selectedIndex;
            }

            if (hasSubmitted)
            {
                var userInput = userInputField.value;
                userInputField.SetValueWithoutNotify(string.Empty);
                ParseCommand(userInput);
                return;
            }

            if (hasTabbed)
            {
                AutoCompleteInput(autoCompleteField.selectedIndex);
            }
        }

        private void ParseCommand(string input)
        {
            var sections = input.Split(' ');
            var command = sections[0];

            if (sections.Length - 1 > 0)
            {
                var args = new object[sections.Length - 1];
                for (int i = 1; i < sections.Length; ++i)
                {
                    var arg = sections[i];

                    args[i - 1] = ParseValue(arg);
                }

                ClearAutoComplete();
                CommandSystem.Execute(command, args);
                return;
            }

            ClearAutoComplete();
            CommandSystem.Execute(command, null);
        }

        private void OnInputChanged(ChangeEvent<string> evt)
        {
            UpdateAutoCompleteOptions();
        }

        private void ClearAutoComplete()
        {
            autoCompleteOptions.Clear();
            autoCompleteField.Rebuild();
            UpdateAutoCompleteOptions();
        }

        private void AutoCompleteInput(int index)
        {
            if (autoCompleteOptions.Count > 0 && index < autoCompleteOptions.Count && index >= 0)
            {
                if (addAutoCompleteOption)
                {
                    userInputField.value += $" {autoCompleteOptions[index]} ";
                    argumentCounter++;
                }
                else
                {
                    userInputField.value = autoCompleteOptions[index];
                }
                userInputField.SelectRange(userInputField.text.Length, userInputField.text.Length);
            }
        }

        private void UpdateAutoCompleteOptions()
        {
            var isEmpty = !TryPopulateOptions(userInputField.text);
            autoCompleteField.Rebuild();
            if (isEmpty)
            {
                autoCompleteField.Q<Label>(null, BaseListView.emptyLabelUssClassName).text = string.Empty;
            }
        }

        private bool TryPopulateOptions(string input)
        {
            autoCompleteOptions.Clear();
            addAutoCompleteOption = false;
            if (string.IsNullOrEmpty(input))
            {
                argumentCounter = 0;
                return false;
            }

            var reg = CommandSystem.GetCommandRegistry();

            input = input.Replace(" ", "");

            if (reg.ContainsKey(input) && reg[input].arguments.Length > argumentCounter)
            {
                var arg = reg[input].arguments[argumentCounter];
                foreach (var v in arg.values)
                {
                    autoCompleteOptions.Add(v.ToString().ToLower());
                    addAutoCompleteOption = true;
                }
                return autoCompleteOptions.Count > 0;
            }

            foreach (var (id, _) in reg)
            {
                if (id.StartsWith(userInputField.text))
                {
                    autoCompleteOptions.Add(id);
                }
            }


            return autoCompleteOptions.Count > 0;
        }

        private object ParseValue(string arg)
        {
            if (bool.TryParse(arg, out var boolean))
            {
                return boolean;
            }

            if (int.TryParse(arg, out var integer))
            {
                return integer;
            }

            return arg;
        }

        private Color GetColor(LogType type)
        {
            return type switch
            {
                LogType.Error => Color.red,
                LogType.Exception => Color.red,
                LogType.Assert => Color.cyan,
                LogType.Warning => Color.yellow,
                LogType.Log => Color.white,
                _ => Color.white
            };
        }

        private void InteruptDefaultEvent(EventBase evt)
        {
            evt.StopImmediatePropagation();
            evt.StopPropagation();
            evt.PreventDefault();
        }



        private void HandleLog(string condition, string stackTrace, LogType type)
        {
            //outputField.AddItem(AppendLineToTree(condition, stackTrace, type, DateTime.Now));

            ParseDebugLine(condition, stackTrace, type, DateTime.Now);
        }

        private void ParseDebugLine(string header, string body, LogType type, DateTime timePoint)
        {
            if (AlreadyContains(header, type, out var foundIndx) && collapseLines)
            {
                var l = lines[foundIndx];
                l.count++;
                l.timePoint = timePoint;
                lines[foundIndx] = l;
                outputField.RefreshItem(foundIndx);
                return;
            }

            var line = new DebugLine
            {
                header = header,
                body = body,
                type = type,
                timePoint = timePoint,
                color = GetColor(type)
            };


            if (lines.Count == lines.Capacity)
            {
                ShiftLines();
                var indx = lines.Count - 1;
                lines[indx] = line;
                outputField.RefreshItems();
                outputField.ScrollToItem(indx);
                return;
            }

            lines.Add(line);
            outputField.RefreshItem(lines.Count - 1);
            outputField.ScrollToItem(lines.Count - 1);


        }

        private bool AlreadyContains(string header, LogType type, out int foundIndx)
        {
            foundIndx = lines.FindLastIndex((DebugLine line) => !string.IsNullOrEmpty(line.header) && line.header.Equals(header) && line.type == type);
            return foundIndx != -1;
        }

        private void ShiftLines()
        {
            for (int i = 0; i < (lines.Count - 1); i++)
            {
                lines[i] = lines[i + 1];
            }
        }

        private string GetTimePoint(DateTime timePoint)
        {
            return string.Format("{0:HH:mm:ss}", timePoint);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tilde) || Input.inputString.Contains("\u00A7"))
            {
                renderConsole = !renderConsole;
                rootElement.SetActive(renderConsole);
                userInputField.SetValueWithoutNotify(string.Empty);
                if (renderConsole)
                {
                    outputField.Rebuild();
                    outputField.ScrollToItem(lines.Count - 1);
                }

            }
        }
    }
}

