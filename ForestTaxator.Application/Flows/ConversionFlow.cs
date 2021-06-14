using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ForestTaxator.Data;
using ForestTaxator.TestApp.Commands;
using Serilog;

namespace ForestTaxator.TestApp.Flows
{
    public static class ConversionFlow
    {
        public static Task Execute(ConvertVerb command, ILogger logger)
        {
            if (File.Exists(command.InputFile) == false)
            {
                logger.Fatal("Input file does not exist!");
                Environment.Exit(0);
            }

            var baseOutput = string.Join(".", command.InputFile.Split(".").SkipLast(1));
            var reader = FileFormatDetector.GetCloudStreamReader(command.InputFile, command.InputFormat);
            var writer = FileFormatDetector.GetCloudStreamWriter(
                command.OutputFile ?? $"{baseOutput}.{command.OutputFormat.ToString().ToLower()}",
                command.OutputFormat
            );

            writer.WritePointSet(reader.ReadPointSet());
            return Task.CompletedTask;
        }
    }
}