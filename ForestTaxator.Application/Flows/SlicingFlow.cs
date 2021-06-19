using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ForestTaxator.Data;
using ForestTaxator.Data.GPD;
using ForestTaxator.Model;
using ForestTaxator.TestApp.Commands.Analyze;
using ForestTaxator.Utils;
using Serilog;

namespace ForestTaxator.TestApp.Flows
{
    public class SlicingFlow
    {
        public static Task Execute(SliceCommand command, ILogger logger)
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
            
            var slices = cloud.Slice(command.SliceHeight).Where(slice => slice != null).OrderBy(slice => slice.Height).ToList();
            using var writer = new GpdWriter(command.Output, null, (float)command.SliceHeight);
            writer.WriteSlices(slices);
            
            return Task.CompletedTask;
            
        }
    }
}