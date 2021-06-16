using System;
using System.IO;
using System.Threading.Tasks;
using ForestTaxator.Data;
using ForestTaxator.Model;
using ForestTaxator.TestApp.Commands.Analyze;
using Serilog;

namespace ForestTaxator.TestApp.Flows
{
    public static class TerrainFlow
    {
        public static Task Execute(TerrainCommand command, ILogger logger)
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
            Console.WriteLine(cloud[9] == null);
            var terrain = new Terrain(cloud);

            return Task.CompletedTask;
            
        }
    }
}