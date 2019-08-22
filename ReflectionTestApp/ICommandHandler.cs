namespace ReflectionTestApp
{
    public interface ICommandHandler
    {
        bool CommandOK { get; }
        string CommandError { get; }
        string Execute();
    }
}
