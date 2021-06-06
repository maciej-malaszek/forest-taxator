using CommandLine;

namespace ForestTaxator.TestApp.Commands.Analyze
{
    [Verb("terrain")]
    public class TerrainCommand
    {
        public enum EMethod
        {
            Lowest,
            Average
        }
        [Value(0, HelpText = "Input file", Required = true)]
        public string Input { get; set; }
        
        [Option('o', "output", Required = true)]
        public string Output { get; set; }
        
        [Option('t', "terrain", Default = 2.5)]
        public float Resolution { get; set; }
        
        [Option('m', "method", Default = EMethod.Average, HelpText = "Terrain detection method (Lowest, Average)")]
        public EMethod Method { get; set; }
        
    }
}