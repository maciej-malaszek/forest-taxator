using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ForestTaxator.Application.Commands.Analyze;
using ForestTaxator.Lib.Algorithms;
using ForestTaxator.Lib.Data;
using ForestTaxator.Lib.Data.GPD;
using ForestTaxator.Lib.Data.XYZ;
using ForestTaxator.Lib.Filters;
using ForestTaxator.Lib.Model;
using ForestTaxator.Lib.Utils;
using Serilog;

namespace ForestTaxator.Application.Flows
{
    public class DetectionFlow
    {
        private static IList<Tree> FilterTrees(IList<Tree> trees, IList<ITreeFilter> treeFilters)
        {
            var result = trees;
            var index = 0;
            foreach (var filter in treeFilters)
            {
                ProgressTracker.Progress(EProgressStage.FakeTreesFiltering, "Filtering Fake Trees", index++, treeFilters.Count);
                result = filter.Filter(result);
            }
            return result;
        }

        private static IList<Tree> Detect(PointSetGroup pointSetGroup)
        {
            var mergingParameters = new MergingParameters
            {
                MinimumRegressionGroupingDistance = 0.09,
                MaximumGroupingEmptyHeight = 20,
                MinimumGroupingDistance = 0.35,
            };
            var treeFilters = new List<ITreeFilter>
            {
                new TreeHeightFilter
                {
                    MinimalTreeHeight = 4
                }
            };
            var potentialTrees = pointSetGroup.BuildTrees(mergingParameters);
            return FilterTrees(potentialTrees, treeFilters);
        }
        
        private static Task<IList<Tree>> ExecuteFromRaw(DetectTreesCommand command, ILogger logger)
        {
            using var streamReader = FileFormatDetector.GetCloudStreamReader(command.Input);
            if (streamReader == null)
            {
                logger.Fatal("Not supported data format.");
                Environment.Exit(0);
            }
            
            var cloud = new Cloud(streamReader);
            var terrain = new Terrain(cloud, command.Resolution);
            foreach (var point in cloud)
            {
                point.Z -= terrain.GetHeight(point);
            }
            var slices = cloud
                .Slice(command.SliceHeight.GetValueOrDefault(0.1))
                .Where(slice => slice != null).OrderBy(slice => slice.Height)
                .ToList();
            var pointSetGroups = FilteringFlow.GetFilteredPointSetGroups(command.FiltersConfigurationFile, logger, slices);
            var pointSetGroup = new PointSetGroup(pointSetGroups.SelectMany(p=>p.PointSets).ToList());
            var trees = Detect(pointSetGroup);
            return Task.FromResult(trees);

        }

        private static Task<IList<Tree>> ExecuteFromProcessed(DetectTreesCommand command, ILogger logger)
        {
            using var reader = new GpdReader(command.Input);
            var cloud = reader.ReadPointSlices().SelectMany(slice => slice.PointSets).ToList();
            var pointSetGroup = new PointSetGroup(cloud);
            var trees = Detect(pointSetGroup);
            return Task.FromResult(trees);
        }

        public static void ExportTreePreview(DetectTreesCommand command, IList<Tree> trees)
        {
            var x = 0;
            foreach (var tree in trees)
            {
                using var writer = new XyzWriter(Path.Join(command.OutputDirectory, $"T{x++}.xyz"));
                foreach (var node in tree.GetAllNodesAsVector())
                {
                    writer.WritePointSet(node.PointSet);
                }
            }
        }
        
        public static void ExportTrees(DetectTreesCommand command, IList<Tree> trees)
        {
            var x = 0;
            foreach (var tree in trees)
            {
                using var writer = new GpdWriter(
                    Path.Join(command.OutputDirectory, $"T{x++}.gpd"), null, command.SliceHeight.GetValueOrDefault(0.1)
                );
                var nodes = tree.GetAllNodesAsVector();
                var nodeDictionary = nodes
                    .Select((n, i) => new Tuple<Tree.Node, int>(n, i))
                    .ToDictionary(t => t.Item1, t => t.Item2);
                foreach (var node in nodeDictionary.Keys)
                {
                    var parentId = node.Parent is null ? -1 : nodeDictionary[node.Parent];
                    var id = nodeDictionary[node];
                    writer.WritePointSet(node.PointSet, 0, $"Id+{id};Parent+{parentId};");
                }
            }
        }
        
        public static async Task Execute(DetectTreesCommand command, ILogger logger)
        {
            if (File.Exists(command.Input) == false)
            {
                logger.Fatal("Input file does not exist!");
                Environment.Exit(0);
            }

            var trees = command.Raw ? await ExecuteFromRaw(command, logger) : await ExecuteFromProcessed(command, logger);
            if (command.ExportPreview)
            {
                ExportTreePreview(command, trees);
            }
            ExportTrees(command, trees);
        }
    }
}