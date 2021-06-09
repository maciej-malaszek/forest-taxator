using CommandLine;
using ForestTaxator.TestApp.Commands.Attributes;

namespace ForestTaxator.TestApp.Commands.Analyze
{
    [Verb("detect-trees")]
    public class DetectTreesCommand
    {
        [Value(0)]
        public string InputPath { get; set; }

        [Option("raw", HelpText = "process files as raw cloud points")]
        public bool Raw { get; set; }

        [Option('c', "filters-configuration")]
        [OptionRequiredIfFlagged("Raw")]
        public string FiltersConfigurationPath { get; set; }

        [Option('h', "slice-height")]
        [OptionRequiredIfFlagged("Raw")]
        public double? SliceHeight { get; set; }

        [Option('f', "format")]
        [OptionRequiredIfFlagged("Raw")]
        public string Format { get; set; }

        [Option('t', "terrain", Required = true)]
        public string TerrainPath { get; set; }

        [Option('o', "output-directory", Required = true)]
        public string OutputDirectory { get; set; }

        [Option("output-format", Default = "gpd")]
        public string OutputFormat { get; set; }
    }
}