
#if UNITY_EDITOR || DEBUG_BUILD
#define COMMANDSYSTEM
#endif

using System;
using System.Collections.Generic;
using UnityEngine;





namespace Spyro.Debug
{
    public struct Command
    {
        public string definition;
        public string desc;
        public Action<object[]> effect;
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

        public static bool HasInitialized => instance != null;

        public static void AddCommand(string syntax, string desc, string def, Action<object[]> effect)
        {
#if COMMANDSYSTEM
            instance.commandRegistry.Add(syntax, new Command { definition = def, desc = desc, effect = effect });
#endif
        }


        public static bool Execute(string id, params object[] args)
        {
#if COMMANDSYSTEM
            if (instance.commandRegistry.ContainsKey(id))
            {
                instance.commandRegistry[id].effect(args);
                return true;
            }
#endif
            return false;
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


