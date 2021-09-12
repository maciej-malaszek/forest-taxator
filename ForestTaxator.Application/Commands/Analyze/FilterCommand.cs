using CommandLine;

namespace ForestTaxator.Application.Commands.Analyze
{
    [Verb("filter")]
    public class FilterCommand
    {
        [Value(0, HelpText = "Input file", Required = true)]
        public string Input { get; set; }
        
        [Option('f', "format", Default = "xyz")]
        public string Format { get; set; }

        [Option('c', "filters-configuration", Required = true)]
        public string FiltersConfigurationFile { get; set; }
        
        [Option('m', "merge", Default = false)]
        public bool Merge { get; set; }
        
        [Option('o', "output", HelpText = "Directory that will be used to store output files", Required = true)]
        public string Output { get; set; }
    }
}