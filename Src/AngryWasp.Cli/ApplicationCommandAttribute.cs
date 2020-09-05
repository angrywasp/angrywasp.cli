using System;

namespace AngryWasp.Cli
{
    public class ApplicationCommandAttribute : Attribute
    {
        private string helpText = string.Empty;
        private string key = string.Empty;

        public string HelpText => helpText;

        public string Key => key;

        public ApplicationCommandAttribute(string key, string helpText)
        {
            this.key = key;
            this.helpText = helpText;
        }
    }
}