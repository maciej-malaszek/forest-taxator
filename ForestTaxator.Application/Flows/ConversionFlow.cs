using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ForestTaxator.Application.Commands;
using ForestTaxator.Lib.Data;
using Serilog;

namespace ForestTaxator.Application.Flows
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
            using var writer = FileFormatDetector.GetCloudStreamWriter(
                command.OutputFile ?? $"{baseOutput}.{command.OutputFormat.ToString().ToLower()}",
                command.OutputFormat
            );

            var ps = reader.ReadPointSet();
            while (ps != null)
            {
                writer.WritePointSet(ps);
                ps = reader.ReadPointSet();
            }
            
            return Task.CompletedTask;
        }
    }
}