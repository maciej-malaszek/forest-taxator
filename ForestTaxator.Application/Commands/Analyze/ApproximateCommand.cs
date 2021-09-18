using CommandLine;

namespace ForestTaxator.Application.Commands.Analyze
{
    [Verb("approximate-trees", HelpText = "Approximates tree trunk using ellipses")]
    public class ApproximateCommand
    {
        [Value(0, HelpText = "Input file", Required = true)]
        public string Input { get; set; }

        [Option('c', "configuration", Required = true)]
        public string ConfigurationFile { get; set; }

        [Option('o', "output", HelpText = "Directory that will be used to store output files", Required = true)]
        public string Output { get; set; }

        [Option("export-preview", HelpText = "export point clouds in XYZ for previewing purpose")]
        public bool ExportPreview { get; set; }

        [Option('h', "node-height", Default = 0.1)]
        public double NodeHeight { get; set; }
        
        [Option('t', "tree-heightmap", Required = true)]
        public string TreeHeightMapPath { get; set; }
        
        [Option("smooth", Default = false, HelpText = "Smooth tree using regression on all nodes. It replaces ellipses with circles")]
        public bool Smooth { get; set; }
    }
}