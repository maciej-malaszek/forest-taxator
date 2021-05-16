using CommandLine;

namespace ForestTaxator.TestApp.Commands
{
    [Verb("detect")]
    public class DetectVerb
    {
        [Option('i', "input", Required = false, HelpText = "Input file to analyze.")]
        public string InputFile { get; set; }
    }
}