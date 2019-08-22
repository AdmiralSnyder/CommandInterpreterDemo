namespace ReflectionTestApp
{
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
}
