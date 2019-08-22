using System.Reflection;

namespace ReflectionTestApp
{
    public abstract class TypeIdPropertyCommandHandler<TThis> : TypeIDCommandHandler<TThis>
        where TThis : TypeIdPropertyCommandHandler<TThis>
    {
        protected PropertyInfo Property { get; private set; }

        private string CheckValidProperty() => ValidProperties.TryGetValue((DataType, Arguments[2]), out var pi)
            && (Property = pi) == pi ? null : $"invalid property {Arguments[2]}";

        protected override string CheckArguments() => base.CheckArguments()
                ?? CheckValidProperty();
    }
}
