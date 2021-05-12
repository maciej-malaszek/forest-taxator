using System;
using System.Collections.Generic;
using System.Linq;
using ForestTaxator.Model;
using GeneticToolkit;

namespace ForestTaxator.Algorithms.Detection
{
    public partial class TreeDetectionAlgorithm
    {
        public IEnumerable<PointSet> FilterNoiseOnLevel(GeneticAlgorithm geneticAlgorithm, IEnumerable<PointSet> groups,
            double treeHeightPercent, Func<float, float> getThreshold)
        {
            var trunkPointSets = new List<PointSet>();

            foreach (var set in groups)
            {
                if (set == null)
                {
                    continue;
                }

                var trunkFitness = GetFitness(geneticAlgorithm, set);
                if (trunkFitness > getThreshold((float) treeHeightPercent))
                {
                    continue;
                }

                trunkPointSets.Add(set);
            }

            return trunkPointSets;
        }

        public ICollection<PointSet> FilterNoise(GeneticAlgorithm geneticAlgorithm, PointSet[][] groups, double treeHeight,
            Func<float, float> getThreshold)
        {
            var trunkPointSets = new List<PointSet>();

            for (var level = 0; level < groups.Length; level++)
            {
                Console.WriteLine($"\nLevel {level + 1} of {groups.Length}: ");
                if (groups[level] == null || groups[level].Any(x => x != null) == false)
                {
                    continue;
                }

                var heightPercent = groups[level].First(x => x != null).Center.Z / treeHeight;
                trunkPointSets.AddRange(FilterNoiseOnLevel(geneticAlgorithm, groups[level], heightPercent, getThreshold));
            }

            return trunkPointSets;
        }
    }
}