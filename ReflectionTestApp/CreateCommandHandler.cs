using System;

namespace ReflectionTestApp
{
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
}
