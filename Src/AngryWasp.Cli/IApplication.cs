namespace AngryWasp.Cli
{
    public interface IApplicationCommand
    {
        bool Handle(string command);
    }
}