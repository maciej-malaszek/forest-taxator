using System;
using System.Collections.Generic;
using System.Linq;
using ForestTaxator.Filters;
using ForestTaxator.Model;
using ForestTaxator.Utils;
using GeneticToolkit;
using GeneticToolkit.Phenotypes.Collective;
using GeneticToolkit.Utils.FitnessFunctions;

namespace ForestTaxator.Algorithms.Detection
{
    public partial class TreeDetectionAlgorithm
    {
        private Distribution[] _testedDistributions;
        public static int DistributionResolution => 32;
        
        public float GetFitness(GeneticAlgorithm geneticAlgorithm, PointSet group)
        {
            if (group == null || group.Center.Z < 0.1)
            {
                return float.MaxValue;
            }

            _testedDistributions = group
                .Normalized()
                .GetDistribution(DistributionResolution, MathUtils.EDimension.X, MathUtils.EDimension.Y)
                .Select(x => x.Normalized()).ToArray();

            geneticAlgorithm.Population.Initialize();
            geneticAlgorithm.Reset();
            geneticAlgorithm.Run();

            var best = geneticAlgorithm.Population.Generation > 0
                ? geneticAlgorithm.Population.HeavenPolicy.Memory[0]
                : geneticAlgorithm.Population.Best;
            var fitness = (float) geneticAlgorithm.Population.FitnessFunction.GetValue(best);

            return fitness;
        }

        public FitnessFunction GetFitnessFunction()
        {
            return new(p =>
                {
                    var phenotype = (CollectivePhenotype<ParabolicParameters>) p;

                    var x = phenotype.GetValue();
                    var distributions = GenerateDistributions(x, DistributionResolution).ToArray();
                    var differences = new Distribution[2];
                    for (var i = 0; i < differences.Length; i++)
                    {
                        differences[i] = distributions[i] - _testedDistributions[i];
                    }

                    var average = differences.Select(d => d.Average()).ToArray();
                    var standardDeviation = differences.Select((d, index) => d.StandardDeviation(average[index])).ToArray();

                    var fitness = new double[differences.Length];
                    for (var i = 0; i < differences.Length; i++)
                    {
                        fitness[i] = Math.Abs(average[i]) + Math.Abs(standardDeviation[i]);
                    }

                    return fitness.Max();
                }
            );
        }

        private static IEnumerable<Distribution> GenerateDistributions(ParabolicParameters parameters, int steps)
        {
            var dataX = new double[steps];
            var dataY = new double[steps];
            for (var i = 0; i < steps; i++)
            {
                float x = (i - steps / 2) / steps;
                dataX[i] = parameters.A1 * Math.Pow(x, 2.0) + parameters.B1 * x + parameters.C1;
                dataY[i] = parameters.A2 * Math.Pow(x, 2.0) + parameters.B2 * x + parameters.C2;
            }

            return new[] {new Distribution(dataX), new Distribution(dataY)};
        }
        
        public static Tree SelectHighestPotentialTree(IEnumerable<Tree> potentialTrees)
        {
            return potentialTrees.OrderByDescending(x => x.GetHighestNode().Center.Z - x.Root.Center.Z).First();
        }
        
        public static IEnumerable<Tree> RemoveFakeTrees(IEnumerable<Tree> potentialTrees)
        {
            ICollection<Tree> filteredTrees = new List<Tree>();
            foreach (var potentialTree in potentialTrees)
            {
                if (potentialTree.GetAllNodesAsVector().Length < 10)
                {
                    continue;
                }

                if (potentialTree.GetHighestNode().Center.Z - potentialTree.Root.Center.Z < 5)
                {
                    continue;
                }

                filteredTrees.Add(potentialTree);
            }

            return filteredTrees;
        }
        private static PointSet[][] GroupPointSets(IReadOnlyList<PointSlice> slices)
        {
            var groups = new PointSet[slices.Count][];
            for (var i = 0; i < slices.Count; i++)
            {
                var pointSlice = slices[i];
                if (pointSlice == null)
                {
                    continue;
                }

                groups[i] = pointSlice.GroupByDistance();
            }

            return groups;
        }
        private static void ApplyFilters(IEnumerable<PointSet[]> groups, IReadOnlyCollection<IPointSetFilter> filters)
        {
            foreach (var group in groups)
            foreach (var filter in filters)
            {
                filter.Filter(@group);
            }
        }

        public static PointSet[][] DivideIntoGroups(PointSet pointSet, Box box, IPointSetFilter[] filters, float sliceHeight = 0.1f)
        {
            var slices = pointSet.SplitByHeight(box, sliceHeight);
            var groups = GroupPointSets(slices);
            ApplyFilters(groups, filters);
            return groups;
        }
    }

    public struct MergingParameters
    {
        public double MinimumGroupingDistance { get; set; }
        public double MinimumRegressionGroupingDistance { get; set; }
        public double MaximumGroupingEmptyHeight { get; set; }
        public double MaximumRegressionEmptyHeight { get; set; }
    }
    
}