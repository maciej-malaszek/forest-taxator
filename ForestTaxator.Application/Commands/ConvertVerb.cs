using CommandLine;

namespace ForestTaxator.TestApp.Commands
{
    [Verb("convert")]
    public class ConvertVerb
    {
        [Option('f', "input", Required = true, HelpText = "Input file to convert.")]
        public string InputFile { get; set; }
        
        [Option('i', "input-format", Required = true)]
        public string InputFormat { get; set; }
        
        [Option('o', "output-format", Required = true)]
        public string OutputFormat { get; set; }
    }
}