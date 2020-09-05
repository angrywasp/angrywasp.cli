using System.Collections.Generic;

namespace AngryWasp.Cli.Args
{
    public class Arguments
    {
        private List<Argument> options = new List<Argument>();

        public Argument this[int index]
        {
            get
            {
                if (index < 0 || index >= options.Count)
                    return null;

                return options[index];
            }
        }

        public Argument this[string flag]
        {
            get
            {
                string f1 = string.IsNullOrEmpty(flag) ? null : flag.ToLower().Trim(new char[] { '-' });

                for (int i = 0; i < options.Count; i++)
                {
                    string f2 = string.IsNullOrEmpty(options[i].Flag) ? null : options[i].Flag.ToLower().Trim(new char[] { '-' });
                    if (f2 == f1)
                        return options[i];
                }

                return null;
            }
        }

        public static Arguments Parse(string[] args)
        {
            Arguments cmd = new Arguments();

            if (args == null)
                return cmd;

            for (int i = 0; i < args.Length; i++)
            {
                bool isFlag = args[i].StartsWith("-");
                bool hasParam = (i + 1) < args.Length ? !args[i + 1].StartsWith("-") : false;

                string parameter = hasParam ? args[i + 1] : null;
                string arg = args[i].TrimStart(new char[] { '-' });

                if (isFlag)
                {
                    char[] argChars = arg.ToCharArray();

                    if (args[i].StartsWith("--"))
                        cmd.Push(arg, parameter);
                    else
                    {
                        if (arg.Length == 1)
                            cmd.Push(arg, parameter);
                        else
                            for (int c = 0; c < argChars.Length; c++)
                                cmd.Push(argChars[c].ToString(), null);
                    }

                    if (argChars.Length == 1 && hasParam)
                        i++;
                }
                else
                    cmd.Push(null, arg);
            }

            return cmd;
        }

        public Argument Pop()
        {
            Argument o = options[0];
            options.RemoveAt(0);
            return o;
        }

        public static Arguments New(params Argument[] arguments) => new Arguments();

        public void Push(Argument o) => options.Add(o);

        public void Push(string flag, string value) => options.Add(new Argument(flag, value));

        public int Count => options.Count;
    }
}