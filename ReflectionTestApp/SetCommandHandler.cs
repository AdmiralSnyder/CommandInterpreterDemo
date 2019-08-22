using System;

namespace ReflectionTestApp
{
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
}
