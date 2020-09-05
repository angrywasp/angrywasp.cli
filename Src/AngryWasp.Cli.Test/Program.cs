using AngryWasp.Cli;
using AngryWasp.Cli.Args;
using AngryWasp.Cli.DefaultCommands;
using System;

namespace AngryWasp.Cli.Test
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] rawArgs)
        {
            Arguments args = Arguments.Parse(rawArgs);
            Log.Initialize();
            new Clear().Handle(null);

            /*string p1 = PasswordPrompt.Get("Test entering a random password");
            string p2 = PasswordPrompt.Get("Enter it again");

            if (p1 != p2)
            {
                Console.WriteLine("Passwords do not match");
                Environment.Exit(1);
            }*/

            Application.RegisterCommands();
            Application.Start();
        }
    }

    // Very basic CLI command to echo what you tell it to.

    [ApplicationCommand("echo", "Echo the command message")]
    public class EchoCliCommand : IApplicationCommand
    {
        public bool Handle(string command)
        {
            Log.WriteConsole($"You said: \"{command}\"");
            return true;
        }
    }

    [ApplicationCommand("pass", "Make the user enter a password before continuing")]
    public class Pass : IApplicationCommand
    {
        public bool Handle(string command)
        {
            Application.Clear();
            string p1 = PasswordPrompt.Get("Test entering a random password");
            Log.WriteConsole("Testing writing to the application log while a password prompt is displayed");
            Log.WriteConsole("All method calls to Log.WriteConsole should be buffered until the password is entered");
            string p2 = PasswordPrompt.Get("Enter it again");

            if (p1 != p2)
            {
                Console.WriteLine("Passwords do not match");
                Environment.Exit(1);
            }

            return true;
        }
    }
}