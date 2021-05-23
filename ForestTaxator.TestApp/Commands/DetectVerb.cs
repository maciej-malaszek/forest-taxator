using CommandLine;

namespace ForestTaxator.TestApp.Commands
{
    [Verb("detect")]
    public class DetectVerb
    {
        [Option('i', "input", Required = true, HelpText = "Input file to analyze.")]
        public string InputFile { get; set; }
        
        [Option('o', "output-directory", Required = true, HelpText = "Output directory where results will be saved")]
        public string OutputDirectory { get; set; }
    }
}