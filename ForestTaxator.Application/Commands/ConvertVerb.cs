using CommandLine;
using ForestTaxator.Lib.Data;

namespace ForestTaxator.TestApp.Commands
{
    [Verb("convert")]
    public class ConvertVerb
    {
        [Value(0, HelpText = "Input file", Required = true)]
        public string InputFile { get; set; }
        
        [Value(1, HelpText = "Output file", Required = false, Default = null)]
        public string OutputFile { get; set; }
        
        [Option('i', "input-format", Required = true)]
        public ESupportedCloudFormat InputFormat { get; set; }
        
        [Option('o', "output-format", Required = true)]
        public ESupportedCloudFormat OutputFormat { get; set; }
    }
}