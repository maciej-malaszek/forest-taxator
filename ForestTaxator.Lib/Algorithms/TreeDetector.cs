using System;
using System.Collections.Generic;
using System.Linq;
using ForestTaxator.Lib.Filters;
using ForestTaxator.Lib.Model;
using ForestTaxator.Lib.Utils;
using Microsoft.Extensions.Logging;

namespace ForestTaxator.Lib.Algorithms
{
    public class TreeDetector
    {
        private readonly ILogger<TreeDetector> _logger;
        public TreeDetector(ILogger<TreeDetector> logger = null)
        {
            _logger = logger;
        }
        /// <summary>
        /// Extracts height-based collection of PointSetGroups that are considered as part of tree trunk.
        /// </summary>
        /// <param name="cloud"></param>
        /// <param name="detectionParameters"></param>
        /// <returns></returns>
        public IList<PointSetGroup> DetectTrunkPointSets(Cloud cloud, DetectionParameters detectionParameters)
        {
            _logger?.LogInformation("Starting cloud slicing...");
            var slices = cloud.Slice(detectionParameters.SliceHeight).ToList();
            _logger?.LogInformation("Cloud sliced");
            _logger?.LogTrace($"Slices count: {slices.Count}");
            _logger?.LogInformation("Starting point grouping...");
            var groups = slices.Select(slice => 
                slice?.GroupByDistance(detectionParameters.MeshWidth, detectionParameters.MinimalPointsPerMesh)
            ).Where(slice => slice != null).ToList();
            _logger?.LogInformation("Points grouped...");
            _logger?.LogInformation("Starting filtering...");
            return groups.Select((group, index) =>
            {
                ProgressTracker.Progress(EProgressStage.NoiseFiltering, "Filtering Noise", index, groups.Count);
                return @group.Filter(detectionParameters.PointSetFilters);
            }).Where(x=>x != null).ToList();
        }
        
        /// <summary>
        /// </summary>
        /// <param name="cloud"></param>
        /// <param name="detectionParameters"></param>
        /// <param name="mergingParameters"></param>
        /// <returns></returns>
        public IList<Tree> DetectPotentialTrees(Cloud cloud, DetectionParameters detectionParameters, MergingParameters mergingParameters)
        {
            var pointSets = DetectTrunkPointSets(cloud, detectionParameters).SelectMany(group => group.PointSets).ToList();
            var pointSetGroup = new PointSetGroup(pointSets);
            var potentialTrees = pointSetGroup.BuildTrees(mergingParameters);
            return FilterTrees(potentialTrees, detectionParameters.TreeFilters);
        }
        
        public static IList<Tree> FilterTrees(IList<Tree> trees, IList<ITreeFilter> treeFilters)
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
    }

    public class DetectionParameters
    {
        public float SliceHeight { get; set; } = 0.1f;
        public ITreeFilter[] TreeFilters { get; set; } = Array.Empty<ITreeFilter>();
        public IPointSetFilter[] PointSetFilters { get; set; } = Array.Empty<IPointSetFilter>();
        public float MeshWidth { get; set; } = 0.1f;
        public int MinimalPointsPerMesh { get; set; } = 2;
    }
    public class MergingParameters
    {
        public double SliceHeight { get; set; } = 0.1f;
        public int MinimumRegressionNodes { get; set; } = 5;
        public double RegressionDistance { get; set; } = 1.0;
        public double MinimumGroupingDistance { get; set; }
        public double MinimumRegressionGroupingDistance { get; set; }
        public double MaximumGroupingEmptyHeight { get; set; }
    }
}