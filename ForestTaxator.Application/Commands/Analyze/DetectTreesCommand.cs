using CommandLine;
using ForestTaxator.Application.Commands.Attributes;

namespace ForestTaxator.Application.Commands.Analyze
{
    [Verb("detect-trees")]
    public class DetectTreesCommand
    {
        [Value(0)]
        public string Input { get; set; }

        [Option("raw", HelpText = "process files as raw cloud points")]
        public bool Raw { get; set; }

        [Option('c', "filters-configuration", HelpText ="Required if --raw switch is used")]
        [OptionRequiredIfFlagged("Raw")]
        public string FiltersConfigurationFile { get; set; }

        [Option('h', "slice-height", HelpText ="Required if --raw switch is used")]
        [OptionRequiredIfFlagged("Raw")]
        public double? SliceHeight { get; set; }

        [Option('r', "resolution", Default = 2.5, HelpText ="Required if --raw switch is used")]
        [OptionRequiredIfFlagged("Raw")]
        public double Resolution { get; set; }

        [Option('o', "output-directory", Required = true)]
        public string OutputDirectory { get; set; }
        
        [Option("export-preview", HelpText = "export point clouds in XYZ for previewing purpose")]
        public bool ExportPreview { get; set; }
    }
}