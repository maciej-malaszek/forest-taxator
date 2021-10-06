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
        
        [Option('r', "resolution", Default = 2.5, HelpText = "Size of rectangle in heightmap grid in meters")]
        public double Resolution { get; set; }
        
        [Option('h', "max-height", Default = 1, HelpText = "Maximum height in meters that will be still valid as terrain height")]
        public double MaxHeight { get; set; }
    }
}