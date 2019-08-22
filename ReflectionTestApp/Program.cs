using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ReflectionTestApp
{
    #region "Database"

    public interface IInDb
    {
        uint ID { get; }
    }

    public class Student : IInDb
    {
        public static uint NextID { get; internal set; }
        public string Name { get; set; }
        public DateTime Birthday { get; set; }

        public int ShoeSize { get; set; }

        public uint ID { get; set; }
    }

    public class Room : IInDb
    {
        public static uint NextID { get; internal set; }
        public int Floor { get; set; }
        public Subjects Subject { get; set; }

        public uint ID { get; set; }
    }

    public class DataContext
    {
        public Dictionary<uint, Student> Students = new Dictionary<uint, Student>();
        public Dictionary<uint, Room> Rooms = new Dictionary<uint, Room>();

        private readonly Dictionary<Type, Func<uint, bool>> ExistsDict = new Dictionary<Type, Func<uint, bool>>();
        private readonly Dictionary<Type, Func<uint, object>> LookupDict = new Dictionary<Type, Func<uint, object>>();
        private readonly Dictionary<Type, Func<uint, bool>> DeleteDict = new Dictionary<Type, Func<uint, bool>>();

        public DataContext()
        {
            ExistsDict[typeof(Student)] = Students.ContainsKey;
            ExistsDict[typeof(Room)] = Rooms.ContainsKey;

            LookupDict[typeof(Student)] = id => Students[id];
            LookupDict[typeof(Room)] = id => Rooms[id];
            
            DeleteDict[typeof(Student)] = Students.Remove;
            DeleteDict[typeof(Room)] = Rooms.Remove;
        }

        public static DataContext Current { get; } = new DataContext();

        internal string AddInstance(object newInstance)
        {
            if (newInstance is Student newStudent)
            {
                var newid = newStudent.ID = Student.NextID++;
                Students.Add(newid, newStudent);
                return newid.ToString();
            }
            else if (newInstance is Room newRoom)
            {
                var newid = newRoom.ID = Room.NextID++;
                Rooms.Add(newid, newRoom);
                return newid.ToString();
            }
            throw new Exception("Unknown object type");
        }

        internal bool Exists(Type dataType, uint id) => ExistsDict[dataType](id);
        internal object Get(Type dataType, uint id) => LookupDict[dataType](id);
        internal void Delete(Type dataType, uint id) => DeleteDict[dataType](id);
    }

    public enum Subjects
    {
        Maths,
        English,
        SingAndDance
    }

    #endregion

    #region Commands

    public enum Commands
    {
        Create,
        Set,
        Get,
        Delete,
    }

    

    public abstract class CommandHandler
    {
        public static void RegisterType<TType>(params string[] properties)
        {
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

        public static Dictionary<string, Type> RegisteredTypes = new Dictionary<string, Type>
        {
            [nameof(Student)] = typeof(Student),
            [nameof(Room)] = typeof(Room),
        };

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

    public abstract class CommandHandler<TThis> : CommandHandler, ICommandHandler
        where TThis : CommandHandler<TThis>
    {

        protected TThis WithArguments(string[] arguments)
        {
            Arguments = arguments;
            CommandError = CheckArguments();
            CommandOK = CommandError is null;
            return (TThis)this;
        }

        protected string[] Arguments { get; private set; }
        public Type DataType { get; private set; }

        protected string CheckRegisteredType() => RegisteredTypes.TryGetValue(Arguments[0], out var type) && ((DataType = type) == type) ? null : $"invalid type '{Arguments[0]}'";

        protected abstract string CheckArguments();

        public bool CommandOK { get; set; }
        public string CommandError { get; set; }
        public abstract string Execute();
    }

    public interface ICommandHandler
    {
        bool CommandOK { get; }
        string CommandError { get; }
        string Execute();
    }

    public class CreateCommandHandler : CommandHandler<CreateCommandHandler>
    {
        protected override string CheckArguments() => Arguments.Length switch
        {
            0 => "No Arguments given",
            1 => CheckRegisteredType(),
            _ => "too many arguments",
        };

        public static CreateCommandHandler Create(string[] arguments) => new CreateCommandHandler().WithArguments(arguments);
        public override string Execute() => DataContext.Current.AddInstance(Activator.CreateInstance(DataType));

    }

    public class SetCommandHandler : TypeIdPropertyCommandHandler<SetCommandHandler>
    {
        public static SetCommandHandler Create(string[] arguments) => new SetCommandHandler().WithArguments(arguments);

        public Func<string, (bool success, object value)> ConverterFunc { get; private set; }
        private object Value { get; set; }

        protected override string CheckArguments()
        {
            if (Arguments.Length != 4) return "4 Arguments needed";
            return base.CheckArguments()
                ?? CheckValidPropertyType()
                ?? CheckValidPropertyValue();
            //?? CheckAllowedPropertyValue(Arguments[3]);
        }

        private string CheckValidPropertyValue()
        {
            bool result;
            (result, Value) = ConverterFunc(Arguments[3]);
            return result ? null : "invalid value string";
        }
        private string CheckValidPropertyType() => KnownConversions.TryGetValue(Property.PropertyType, out var conv) && (ConverterFunc = conv) == conv ? null : "unknown property type";

        public override string Execute()
        {
            var obj = DataContext.Current.Get(DataType, Id);
            Property.SetValue(obj, Value);
            return "OK";
        }
    }

    public abstract class TypeIDCommandHandler<TThis> : CommandHandler<TThis>
        where TThis : TypeIDCommandHandler<TThis>
    {
        protected override string CheckArguments() =>
            CheckRegisteredType()
                ?? CheckValidID(Arguments[1])
                ?? CheckExistingID();
        protected uint Id { get; private set; }
        private string CheckValidID(string idString) => uint.TryParse(idString, out var id) && ((Id = id) == id) ? null : $"invalid id value '{idString}'";
        private string CheckExistingID() => DataContext.Current.Exists(DataType, Id) ? null : "Entry doesn't exist.";
    }

    public abstract class TypeIdPropertyCommandHandler<TThis> : TypeIDCommandHandler<TThis>
        where TThis : TypeIdPropertyCommandHandler<TThis>
    {
        protected PropertyInfo Property { get; private set; }

        private string CheckValidProperty() => ValidProperties.TryGetValue((DataType, Arguments[2]), out var pi) 
            && (Property = pi) == pi ? null : $"invalid property {Arguments[2]}";

        protected override string CheckArguments() => base.CheckArguments()
                ?? CheckValidProperty();
    }

    public class DeleteCommandHandler : TypeIDCommandHandler<DeleteCommandHandler>
    {
        public static DeleteCommandHandler Create(string[] arguments) => new DeleteCommandHandler().WithArguments(arguments);

        protected override string CheckArguments()
        {
            if (Arguments.Length != 2) return "2 Arguments needed";
            return CheckRegisteredType();
        }
        public override string Execute()
        {
            DataContext.Current.Delete(DataType, Id);
            return "OK";
        }
    }

    public class GetCommandHandler : TypeIdPropertyCommandHandler<GetCommandHandler>
    {
        public static GetCommandHandler Create(string[] arguments) => new GetCommandHandler().WithArguments(arguments);

        protected override string CheckArguments()
        {
            if (Arguments.Length != 3) return "3 Arguments needed";
            return base.CheckArguments();
        }

        public override string Execute()
        {
            var obj = DataContext.Current.Get(DataType, Id);

            var value = Property.GetValue(obj);
            return value.ToString();
        }
    }

    #endregion

    #region Helper Classes

    public interface IFetch<TKey, TValue>
    {
        public TValue this[TKey key] { get; }
        public bool TryGetValue(TKey key, out TValue value);
    }

    public class Fetch<TKey, TValue> : IFetch<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> BackingDictionary = new Dictionary<TKey, TValue>();

        public TValue this[TKey key]
        {
            get => BackingDictionary[key];
            set => BackingDictionary[key] = value;
        }

        public bool TryGetValue(TKey key, out TValue value) => BackingDictionary.TryGetValue(key, out value);
    }

    #endregion

    class Program
    {
        static string ExecuteCommand(string command) => CommandHandler.HandleCommand(command);

        static void Main(string[] args)
        {
            CommandHandler.RegisterType<Student>(nameof(Student.Name), nameof(Student.Birthday), nameof(Student.ShoeSize));
            CommandHandler.RegisterType<Room>(nameof(Room.Floor), nameof(Room.Subject));

            string studentID = ExecuteCommand("Create Student");
            string roomID = ExecuteCommand("Create Room");
            string nameResult = ExecuteCommand($"Set Student {studentID} Name 'Fred'");
            string deleteResult = ExecuteCommand($"Delete Room {roomID}");
            string name = ExecuteCommand($"Get Student {studentID} Name");
            Console.Write(name);
            Console.ReadLine();
        }
    }
}
