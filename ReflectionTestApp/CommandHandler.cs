using System;

namespace ReflectionTestApp
{
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
}
