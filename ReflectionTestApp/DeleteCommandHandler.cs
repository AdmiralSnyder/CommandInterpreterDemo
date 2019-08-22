namespace ReflectionTestApp
{
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
}
