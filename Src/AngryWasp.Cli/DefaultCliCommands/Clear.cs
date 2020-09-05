using System;
using System.Reflection;

namespace AngryWasp.Cli.DefaultCommands
{
    [ApplicationCommand("clear", "Clears the console")]
    public class Clear : IApplicationCommand
    {
        public bool Handle(string command)
        {
            Application.Clear();
            var an = Assembly.GetEntryAssembly().GetName();
            Log.WriteConsole($"{an.Name}: {an.Version}");
            return true;
        }
    }
}