using CommandLine;

namespace ForestTaxator.TestApp.Commands.Analyze
{
    [Verb("approximate-trees", HelpText = "Approximates tree trunk using ellipses")]
    public class ApproximateCommand
    {
        [Value(0, HelpText = "Input file", Required = true)]
        public string Input { get; set; }

        [Option('c', "configuration", Required = true)]
        public string ConfigurationFile { get; set; }

        [Option('o', "output", HelpText = "Directory that will be used to store output files", Required = true)]
        public string Output { get; set; }
    }
}