using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Spyro.Debug
{
    public class ConsoleInput
    {
        private TextField userInputField;
        private ListView autoCompleteField;

        private bool addAutoCompleteOption;
        private int argumentCounter;

        private List<string> autoCompleteOptions = new List<string>();

        public ConsoleInput(VisualElement rootElement)
        {
            InitInputFields(rootElement);
        }

        public void RegisterCallbacks()
        {
            userInputField.RegisterCallback<KeyDownEvent>(ParsingInput, TrickleDown.TrickleDown);
            userInputField.RegisterCallback<ChangeEvent<string>>(OnInputChanged, TrickleDown.TrickleDown);
            userInputField.RegisterCallback<NavigationMoveEvent>(OnInputMove, TrickleDown.TrickleDown);
        }

        public void UnregisterCallbacks()
        {
            userInputField.UnregisterCallback<KeyDownEvent>(ParsingInput, TrickleDown.TrickleDown);
            userInputField.UnregisterCallback<ChangeEvent<string>>(OnInputChanged, TrickleDown.TrickleDown);
            userInputField.UnregisterCallback<NavigationMoveEvent>(OnInputMove, TrickleDown.TrickleDown);
        }

        private void OnInputMove(NavigationMoveEvent evt)
        {
            InteruptDefaultEvent(evt);
        }

        private void InitInputFields(VisualElement rootElement)
        {
            userInputField = rootElement.Q<TextField>("input_field");
            userInputField.value = string.Empty;

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
                var offset = evt.keyCode == KeyCode.UpArrow ? -1 : 1;
                autoCompleteField.selectedIndex += offset;
                autoCompleteField.selectedIndex =
                    autoCompleteField.selectedIndex < 0 ? autoCompleteOptions.Count - 1 :
                    autoCompleteField.selectedIndex > autoCompleteOptions.Count - 1 ? 0 : autoCompleteField.selectedIndex;
                autoCompleteField.ScrollToItem(autoCompleteField.selectedIndex);
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
                    if (CommandSystem.IsInputKeyword(v))
                    {
                        var result = CommandSystem.ParseKeyword(v, 1);
                        if (result is List<string>)
                        {
                            autoCompleteOptions.AddRange(result as List<string>);
                        }

                    }
                    else
                    {
                        autoCompleteOptions.Add(v.ToString().ToLower());
                    }

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

        private void InteruptDefaultEvent(EventBase evt)
        {
            evt.StopImmediatePropagation();
            evt.StopPropagation();
            evt.PreventDefault();
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

        public void ResetInputField()
        {
            userInputField.SetValueWithoutNotify(string.Empty);
        }
    }
}
