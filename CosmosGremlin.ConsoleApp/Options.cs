using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace CosmosGremlin.ConsoleApp
{
    public class Options
    {
        [Option('u', "Url", HelpText ="URI to endpoint", Required =true)]
        public string Url { get; set; }

        [Option('k', "Account_Key", HelpText ="", Required =true)]
        public string AccountKey { get; set; }

        [Option('h', "Help", HelpText = "Help", DefaultValue = false)]
        public bool DoHelp { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
              (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
