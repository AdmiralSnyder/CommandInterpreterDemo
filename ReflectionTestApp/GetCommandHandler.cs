namespace ReflectionTestApp
{
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
}
