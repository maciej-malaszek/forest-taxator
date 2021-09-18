using System;
using System.IO;
using System.Threading.Tasks;
using ForestTaxator.Application.Commands.Analyze;
using ForestTaxator.Lib.Data;
using ForestTaxator.Lib.Model;
using Serilog;

namespace ForestTaxator.Application.Flows
{
    public static class TreeHeightFlow
    {
        public static Task Execute(TreeHeightCommand command, ILogger logger)
        {
            if (File.Exists(command.Input) == false)
            {
                logger.Fatal("Provided cloud point data file does not exist!");
                Environment.Exit(0);
            }

            using var streamReader = FileFormatDetector.GetCloudStreamReader(command.Input);
            if (streamReader == null)
            {
                logger.Fatal("Not supported data format.");
                Environment.Exit(0);
            }
            
            var cloud = new Cloud(streamReader);
            var treeHeightMap = new TreeHeightMap(cloud, command.Resolution, command.MaxHeight);
            treeHeightMap.Export(command.Output);

            return Task.CompletedTask;
            
        }
    }
}