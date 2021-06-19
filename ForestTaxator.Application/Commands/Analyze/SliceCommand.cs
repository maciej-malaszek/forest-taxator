using CommandLine;

namespace ForestTaxator.TestApp.Commands.Analyze
{
    [Verb("slice")]
    public class SliceCommand
    {
        [Value(0, HelpText = "Input file", Required = true)]
        public string Input { get; set; }
        
        [Option('h', "slice-height", Default = 0.1)]
        public double SliceHeight { get; set; }
        
        [Option('o', "output", Required = true)]
        public string Output { get; set; }
    }
}