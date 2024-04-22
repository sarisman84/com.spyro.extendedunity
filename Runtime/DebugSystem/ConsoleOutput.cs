using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Spyro.Debug
{
    public struct DebugLine : IEquatable<DebugLine>
    {
        public string header;
        public string body;
        public Color color;
        public DateTime timePoint;
        public LogType type;
        public int count;

        public bool Equals(DebugLine other)
        {
            return header == other.header && body == other.body && type == other.type;
        }
    }

    public class ConsoleOutput
    {
        private bool _activeState;
        private float _refreshDelayInSeconds;
        private ListView _outputField;
        private DebugLine[] _lines;
        private bool[] _markedLinesToRefresh;
        public bool ActiveState
        {
            get => _activeState;
            set
            {
                _activeState = value;
                if (_activeState)
                {
                    Array.Clear(_lines, 0, _lines.Length);
                    Array.Clear(_markedLinesToRefresh, 0, _markedLinesToRefresh.Length);
                }
            }
        }

        public bool CollapseLines
        {
            set; get;
        }

        public ConsoleOutput(VisualElement rootElement, int amountOfLines, float refreshDelayInSeconds = 0.15f)
        {
            _refreshDelayInSeconds = refreshDelayInSeconds;
            _lines = new DebugLine[amountOfLines];
            _markedLinesToRefresh = new bool[amountOfLines];
    
            InitOutputField(rootElement);
        }

        private void InitOutputField(VisualElement rootElement)
        {
            _outputField = rootElement.Q<ListView>("output_field");
            _outputField.itemsSource = _lines;
            _outputField.makeItem = () => new Label();
            _outputField.bindItem = BindLine;
            _outputField.Q<ScrollView>().verticalScrollerVisibility = ScrollerVisibility.Hidden;
        }

        private void BindLine(VisualElement element, int index)
        {
            var label = element as Label;
            var line = _lines[index];

            var counter = line.count > 0 && CollapseLines ? $"[x{line.count}]" : "";

            label.text = $"[{line.type}][{GetTimePoint(line.timePoint)}]: {line.header} {counter}\n{line.body}";
            label.style.color = line.color;
        }

        private string GetTimePoint(DateTime timePoint)
        {
            return string.Format("{0:HH:mm:ss}", timePoint);
        }

        private Color GetColorOfType(LogType type)
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

        public void AddLine(string _header, string _body, LogType _type, DateTime _timePoint)
        {
            var line = new DebugLine()
            {
                header = _header,
                body = _body,
                type = _type,
                timePoint = _timePoint,
                color = GetColorOfType(_type)
            };
            AddLine(line);

        }

        public void AddLine(string _header, string _body, LogType _type)
        {
            var line = new DebugLine()
            {
                header = _header,
                body = _body,
                type = _type,
                timePoint = DateTime.Now,
                color = GetColorOfType(_type)
            };
            AddLine(line);

        }

        public void AddLine(DebugLine line)
        {
            if (CollapseLines)
            {
                var foundLine = Array.FindLastIndex(_lines, (d) => d.Equals(line));
                if (foundLine != -1)
                {
                    var count = _lines[foundLine].count;
                    _lines[foundLine] = line;
                    _lines[foundLine].count = count + 1;
                    _markedLinesToRefresh[foundLine] = true;
                    return;
                }
            }
            InsertNewLine(line);
        }

        private void InsertNewLine(DebugLine line)
        {

            Array.Copy(_lines, 0, _lines, 1, _lines.Length - 1);
            _lines[0] = line;
            MarkAllLinesForRefresh();
        }

        public IEnumerator Update()
        {
            while (true)
            {
                yield return new WaitUntil(() => _activeState && Array.Find(_markedLinesToRefresh, (r) => r));
                yield return new WaitForSeconds(_refreshDelayInSeconds);
                RefreshConsole();
            }

        }

        private void RefreshConsole()
        {
            for (var i = 0; i < _markedLinesToRefresh.Length; ++i)
            {
                if (_markedLinesToRefresh[i])
                {
                    _outputField.RefreshItem(i);
                }
            }

            Array.Clear(_markedLinesToRefresh, 0, _markedLinesToRefresh.Length);
        }

        public void Clear()
        {
            Array.Clear(_lines, 0, _lines.Length);
            MarkAllLinesForRefresh();
        }

        private void MarkAllLinesForRefresh()
        {
            for (var i = 0; i < _lines.Length; ++i)
            {
                _markedLinesToRefresh[i] = true;
            }
        }
    }
}
