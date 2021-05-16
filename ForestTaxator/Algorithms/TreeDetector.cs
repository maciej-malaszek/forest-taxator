using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ForestTaxator.Filters;
using ForestTaxator.Model;

namespace ForestTaxator.Algorithms
{
    public class TreeDetector
    {

        /// <summary>
        /// Extracts height-based collection of PointSetGroups that are considered as part of tree trunk.
        /// </summary>
        /// <param name="cloud"></param>
        /// <param name="detectionParameters"></param>
        /// <returns></returns>
        public IList<PointSetGroup> DetectTrunkPointSets(Cloud cloud, DetectionParameters detectionParameters)
        {
            Console.WriteLine("Starting cloud slicing...");
            var slices = cloud.Slice(detectionParameters.SliceHeight).ToList();
            Console.WriteLine("Cloud sliced");
            Console.WriteLine($"Slices count: {slices.Count}");
            Console.WriteLine("Starting point grouping...");
            var groups = slices.Select(slice => 
                slice?.GroupByDistance(detectionParameters.MeshWidth, detectionParameters.MinimalPointsPerMesh)
            ).Where(slice => slice != null);
            Console.WriteLine("Points grouped...");
            Console.WriteLine("Starting filtering...");
            return groups.Select(group => group.Filter(detectionParameters.PointSetFilters)).ToList();
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
            return treeFilters.Aggregate(trees, (current, pointSetFilter) => pointSetFilter.Filter(current));
        }
    }

    public class DetectionParameters
    {
        public float SliceHeight { get; set; } = 0.1f;
        public ITreeFilter[] TreeFilters { get; set; }
        public IPointSetFilter[] PointSetFilters { get; set; }
        public float MeshWidth { get; set; } = 0.1f;
        public int MinimalPointsPerMesh { get; set; } = 10;
    }
    public class MergingParameters
    {
        public int MinimumRegressionNodes { get; set; } = 5;
        public double RegressionDistance { get; set; } = 1.0;
        public double MinimumGroupingDistance { get; set; }
        public double MinimumRegressionGroupingDistance { get; set; }
        public double MaximumGroupingEmptyHeight { get; set; }
    }
}