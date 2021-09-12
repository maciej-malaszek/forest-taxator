using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ForestTaxator.Application.Commands.Analyze;
using ForestTaxator.Lib.Data;
using ForestTaxator.Lib.Data.GPD;
using ForestTaxator.Lib.Model;
using Serilog;

namespace ForestTaxator.Application.Flows
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
            
            if (File.Exists(command.TerrainPath) == false)
            {
                logger.Fatal("Terrain file does not exist!");
                Environment.Exit(0);
            }
            
            var cloud = new Cloud(streamReader);
            var terrain = Terrain.Import(command.TerrainPath);

            
            var slices = cloud.Slice(command.SliceHeight).Where(slice => slice != null).OrderBy(slice => slice.Height).ToList();
            foreach (var pointSlice in slices)
            {
                foreach (var pointSet in pointSlice.PointSets)
                {
                    foreach (var point in pointSet)
                    {
                        point.Z -= terrain.GetHeight(point);
                    }
                }
            }
            using var writer = new GpdWriter(command.Output, null, (float)command.SliceHeight);
            writer.WriteSlices(slices);
            
            return Task.CompletedTask;
        }
    }
}