namespace AngryWasp.Cli.Args
{
    public class Argument
    {
        private string flag;
        private string value;

        public string Flag => flag;

        public string Value => value;

        public Argument(string flag, string value)
        {
            this.flag = flag;
            this.value = value;
        }
    }
}