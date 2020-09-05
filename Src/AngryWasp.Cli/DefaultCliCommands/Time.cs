using System;

namespace AngryWasp.Cli.DefaultCommands
{
    [ApplicationCommand("time", "Display the current time")]
    public class Time : IApplicationCommand
    {
        public bool Handle(string command)
        {
            Log.WriteConsole($"Current time: {DateTime.Now}");
            return true;
        }
    }
}