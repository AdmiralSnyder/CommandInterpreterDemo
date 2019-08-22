using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ReflectionTestApp
{
    public abstract class CommandHandler
    {
        public static void RegisterType<TType>(params string[] properties)
        {
            RegisteredTypes.Add(typeof(TType).Name, typeof(TType));
            foreach (var pi in typeof(TType).GetProperties())
            {
                if (properties.Contains(pi.Name))
                {
                    ValidProperties[(typeof(TType), pi.Name)] = pi;
                }
            }
        }

        public static Dictionary<(Type type, string propertyName), PropertyInfo> ValidProperties = new Dictionary<(Type type, string propertyName), PropertyInfo>();

        public static Dictionary<Type, Func<string, (bool, object)>> KnownConversions = new Dictionary<Type, Func<string, (bool, object)>>
        {
            [typeof(int)] = val => (int.TryParse(val, out var parsed), parsed),
            [typeof(string)] = val => (true, val),
            [typeof(DateTime)] = val => (DateTime.TryParse(val, out var parsed), parsed),
            [typeof(bool)] = val => (bool.TryParse(val, out var parsed), parsed),
            //[typeof(int)] = val => (DateTime.TryParse(val, out var parsed), parsed),
            //[typeof(int)] = val => (DateTime.TryParse(val, out var parsed), parsed),
        };


        public static readonly IFetch<string, Commands> CommandsByName = new Fetch<string, Commands>
        {
            [nameof(Commands.Create)] = Commands.Create,
            [nameof(Commands.Set)] = Commands.Set,
            [nameof(Commands.Get)] = Commands.Get,
            [nameof(Commands.Delete)] = Commands.Delete,
        };

        public static readonly IFetch<Commands, Func<string[], ICommandHandler>> CommandHandlers = new Fetch<Commands, Func<string[], ICommandHandler>>
        {
            [Commands.Create] = CreateCommandHandler.Create,
            [Commands.Set] = SetCommandHandler.Create,
            [Commands.Get] = GetCommandHandler.Create,
            [Commands.Delete] = DeleteCommandHandler.Create,
        };

        public static Dictionary<string, Type> RegisteredTypes = new Dictionary<string, Type>();

        internal static string HandleCommand(string command)
        {
            var parts = command.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) throw new Exception("command too short");
            if (!CommandsByName.TryGetValue(parts[0], out var cmd)) throw new Exception("Invalid command " + parts[0]);

            var handler = CommandHandlers[cmd](parts.Skip(1).ToArray());
            if (!handler.CommandOK) throw new Exception(handler.CommandError);

            return handler.Execute();
        }
    }
}
