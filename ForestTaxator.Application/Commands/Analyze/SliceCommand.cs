using CommandLine;

namespace ForestTaxator.Application.Commands.Analyze
{
    [Verb("slice")]
    public class SliceCommand
    {
        [Value(0, HelpText = "Input file", Required = true)]
        public string Input { get; set; }
        
        [Option('h', "slice-height", Default = 0.1)]
        public double SliceHeight { get; set; }
        
        [Option('t', "terrain", Required = true, HelpText = "Path to terrain heightmap file")]
        public string TerrainPath { get; set; }
        
        [Option('o', "output", Required = true)]
        public string Output { get; set; }
    }
}