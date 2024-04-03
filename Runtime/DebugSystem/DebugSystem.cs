
#if UNITY_EDITOR || DEBUG_BUILD
#define COMMANDSYSTEM
#endif

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


namespace Spyro.Debug
{
    public struct Arg
    {
        public object[] values;

        public Arg(params object[] values)
        {
            this.values = values;
        }
    }
    public struct Command
    {
        public Arg[] arguments;
        public string desc;
        public Func<object[], bool> effect;
    }
    public class CommandSystem
    {
        private static CommandSystem instance;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void OnInit()
        {
#if COMMANDSYSTEM
            instance = new CommandSystem();
            var command = new GameObject("[DEBUG] Console", typeof(Console));
            GameObject.DontDestroyOnLoad(command);
#endif
        }

        private Dictionary<string, Command> commandRegistry = new Dictionary<string, Command>();
        private Dictionary<string, Action<object[]>> keywords = new Dictionary<string, Action<object[]>>();

        public static bool HasInitialized => instance != null;

        public static event Action<string, object[]> onFailedCommandExecution;

        public static void AddCommand(string syntax, string desc, Func<object[], bool> effect, params Arg[] def)
        {
#if COMMANDSYSTEM
            instance.commandRegistry.Add(syntax, new Command { arguments = def, desc = desc, effect = effect });
#endif
        }

        public static void AddKeyword(string keyword, Action<object[]> keywordEvent)
        {
#if COMMANDSYSTEM
            instance.keywords.Add(keyword.ToLower(), keywordEvent);
#endif
        }


        public static bool TryParseKeyword(object input, params object[] args)
        {
#if COMMANDSYSTEM
            var result = IsInputKeyword(input);
            if (result)
            {
                instance.keywords[(input as string).ToLower()](args);
            }
            return result;
#else
            return false;
#endif
        }

        public static bool IsInputKeyword(object input)
        {
#if COMMANDSYSTEM
            return input is string && instance.keywords.ContainsKey((input as string).ToLower());
#else
            return false;
#endif

        }


        public static bool Execute(string id, params object[] args)
        {
            var result = false;
#if COMMANDSYSTEM

            if (instance.commandRegistry.ContainsKey(id))
            {
                result = instance.commandRegistry[id].effect(args);
            }

            if (!result)
            {
                onFailedCommandExecution?.Invoke(id, args);
            }
#endif

            return result;
        }

        public static Dictionary<string, Command> GetCommandRegistry()
        {
#if COMMANDSYSTEM
            return instance.commandRegistry;
#else
            return default;
#endif
        }
    }
}


