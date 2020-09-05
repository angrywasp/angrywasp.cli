using AngryWasp.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace AngryWasp.Cli
{
    public static class Application
    {
        private static bool exitTriggered = false;
        private static string exitMessage = null;
        public static void TriggerExit(string message)
        {
            exitTriggered = true;
            exitMessage = message;
        }

        private static bool logBufferPaused = false;

        public static void PauseBufferedLog(bool pause) => logBufferPaused = pause;

        public delegate bool CliFunc(string arg);

        private static Dictionary<string, Tuple<string, CliFunc>> commands = new Dictionary<string, Tuple<string, CliFunc>>();

        public static Dictionary<string, Tuple<string, CliFunc>> Commands => commands;

        public static void RegisterCommands()
        {
            RegisterCommands(Assembly.GetEntryAssembly());
            RegisterCommands(Assembly.GetExecutingAssembly());
        }

        public static void RegisterCommands(Assembly assembly)
        {
            var types = ReflectionHelper.Instance.GetTypesInheritingOrImplementing(assembly, typeof(IApplicationCommand))
                .Where(m => m.GetCustomAttributes(typeof(ApplicationCommandAttribute), false).Length > 0)
                .ToArray();

            foreach (var type in types)
            {
                IApplicationCommand ia = (IApplicationCommand)Activator.CreateInstance(type);
                ApplicationCommandAttribute a = ia.GetType().GetCustomAttributes(true).OfType<ApplicationCommandAttribute>().FirstOrDefault();
                RegisterCommand(a.Key, a.HelpText, ia.Handle);
            }
        }

        public static void RegisterCommand(string key, string helpText, CliFunc handler)
        {
            if (!commands.ContainsKey(key))
                commands.Add(key, new Tuple<string, CliFunc>(helpText, handler));
        }

        public static void Clear()
        {
            Console.Clear();
            if (Environment.GetEnvironmentVariable("TERM").StartsWith("xterm")) 
                Console.WriteLine("\x1b[3J");
            Console.CursorTop = 0;
        }

        private static string PopWord(ref string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;
                
            int index = input.IndexOf(' ');
            if (index == -1)
            {
                string ret = input;
                input = string.Empty;
                return ret;
            }
            else
            {
                string ret = input.Substring(0, index);
                input = input.Remove(0, ret.Length).TrimStart();
                return ret;
            }
        }

        public static void Start(bool allowUserInput = true)
        {
            bool noPrompt = true;
            List<char> enteredText = new List<char>();

            Thread t0 = new Thread(new ThreadStart( () =>
            {
                while(true)
                {
                    if (exitTriggered)
                    {
                        if (Console.CursorLeft != 0)
                        {
                            Console.Write("\r");
                            Console.Write(new string(' ', Console.BufferWidth));
                            Console.Write("\r");
                        }

                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(exitMessage);
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                    }

                    if (Log.Buffer.Count == 0 || logBufferPaused)
                    {
                        Thread.Sleep(250);
                        continue;
                    }

                    //convert to array to make a copy before clearing the original list
                    var logMessages = Log.Buffer.ToArray();
                    Log.Buffer.Clear();

                    if (Console.CursorLeft != 0)
                    {
                        Console.Write("\r");
                        Console.Write(new string(' ', Console.BufferWidth));
                        Console.Write("\r");
                    }
                    
                    foreach (var m in logMessages)
                    {
                        Console.ForegroundColor = m.Item1;
                        Console.WriteLine(m.Item2);
                    }

                    if (allowUserInput)
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("> ");
                        Console.Write(new string(enteredText.ToArray()));
                    }
                }
            }));
            
            Thread t1 = new Thread(new ThreadStart( () =>
            {
                List<string> lines = new List<string>();
                int lineIndex = 0, lastLineIndex = 0;
                
                while (true)
                {
                    if (exitTriggered)
                        break;

                    if (!noPrompt)
                        Console.Write("> ");

                    if (!Console.KeyAvailable)
                    {
                        noPrompt = true;
                        Thread.Sleep(100);
                        continue;
                    }

                    var key = Console.ReadKey();
                    noPrompt = false;
                    if (key.Key == ConsoleKey.UpArrow)
                    {
                        --lineIndex;
                        MathHelper.Clamp(ref lineIndex, 0, lines.Count - 1);

                        if (lineIndex == lastLineIndex)
                        {
                            noPrompt = true;
                            continue;
                        }

                        lastLineIndex = lineIndex;
                        Console.Write("\r");
                        Console.Write(new string(' ', Console.BufferWidth));
                        Console.Write("\r");
                        Console.Write("> ");

                        if (lineIndex < lines.Count)
                        {
                            Console.Write(lines[lineIndex]);
                            noPrompt = true;
                        }

                        enteredText.Clear();
                        enteredText.AddRange(lines[lineIndex]);
                        
                        continue;
                    }
                    else if (key.Key == ConsoleKey.DownArrow)
                    {
                        ++lineIndex;
                        MathHelper.Clamp(ref lineIndex, 0, lines.Count - 1);

                        if (lineIndex == lastLineIndex)
                        {
                            noPrompt = true;
                            continue;
                        }

                        lastLineIndex = lineIndex;

                        Console.Write("\r");
                        Console.Write(new string(' ', Console.BufferWidth));
                        Console.Write("\r");
                        Console.Write("> ");

                        if (lineIndex < lines.Count)
                        {
                            Console.Write(lines[lineIndex]);
                            noPrompt = true;
                        }

                        enteredText.Clear();
                        enteredText.AddRange(lines[lineIndex]);
                        
                        continue;
                    }
                    else if (key.Key == ConsoleKey.Backspace)
                    {
                        if (enteredText.Count > 0)
                        {
                            if (Console.CursorLeft == 0)
                            {
                                Console.CursorTop -= 1;
                                Console.CursorLeft = Console.BufferWidth - 1;
                                enteredText.RemoveAt(enteredText.Count - 1);
                                Console.Write(" ");
                                Console.CursorLeft = Console.BufferWidth - 1;
                                Console.CursorTop -= 1;
                            }
                            else
                            {
                                enteredText.RemoveAt(enteredText.Count - 1);
                                Console.Write("\b \b");
                            }

                        }

                        noPrompt = true;
                    }
                    else if (key.Key == ConsoleKey.Enter)
                    {
                        string line = new string(enteredText.ToArray());
                        string commandString = new string(enteredText.ToArray());
                        enteredText.Clear();
                        Console.WriteLine();
                        
                        string cmd = PopWord(ref commandString);

                        Application.PauseBufferedLog(true);

                        if (commands.ContainsKey(cmd))
                        {
                            if (!commands[cmd].Item2.Invoke(commandString))
                                Log.WriteError("Command failed");
                        }
                        else
                            Log.WriteError("Unknown command");

                        Application.PauseBufferedLog(false);
                        
                        if (!string.IsNullOrEmpty(line))
                            lines.Add(line);

                        lineIndex = lines.Count;
                        lastLineIndex = lineIndex;
                    }
                    else if (key.KeyChar != '\u0000')
                    {
                        enteredText.Add(key.KeyChar);
                        noPrompt = true;
                    }
                    // Ignore the following keys
                    // TODO: Implement handling of these keys
                    else if (key.Key == ConsoleKey.LeftArrow ||
                             key.Key == ConsoleKey.RightArrow)
                        noPrompt = true;

                    if (exitTriggered)
                        break;
                }
            }));

            t0.Start();

            if (allowUserInput)
                t1.Start();

            t0.Join();

            if (allowUserInput)
                t1.Join();
        }
    }
}