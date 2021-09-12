using CommandLine;

namespace ForestTaxator.Application.Commands.Analyze
{
    [Verb("terrain")]
    public class TerrainCommand
    {
        [Value(0, HelpText = "Input file", Required = true)]
        public string Input { get; set; }
        
        [Option('o', "output", Required = true)]
        public string Output { get; set; }
        
        [Option('r', "resolution", Default = 2.5)]
        public double Resolution { get; set; }
    }
}