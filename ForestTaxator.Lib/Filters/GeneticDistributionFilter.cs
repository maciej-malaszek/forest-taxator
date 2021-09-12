using System;
using System.Collections.Generic;
using System.Linq;
using ForestTaxator.Lib.Extensions;
using ForestTaxator.Lib.Fitness;
using ForestTaxator.Lib.Model;
using ForestTaxator.Lib.Utils;
using GeneticToolkit;
using GeneticToolkit.Interfaces;

namespace ForestTaxator.Lib.Filters
{
    /// <summary>
    /// This filter utilizes genetic algorithm to find parameters of parabolic function that will generate two-dimensional point distribution
    /// matching to actual point set.
    /// </summary>
    public class GeneticDistributionFilter : IPointSetFilter
    {
        /// <summary>
        /// Maximum allowed fitness value depending on slice height in meters.
        /// </summary>
        public Func<double, double> GetTrunkThreshold { get; set; }

        public static int DistributionResolution { get; set; }
        public GeneticAlgorithm GeneticAlgorithm { get; set; }
        private readonly IFitnessFunction _fitnessFunction;

        public GeneticDistributionFilter(GeneticDistributionFilterParams parameters)
        {
            GeneticAlgorithm = parameters.GeneticAlgorithm;
            DistributionResolution = parameters.DistributionResolution;
            GeneticDistributionFitness.DistributionResolution = parameters.DistributionResolution;
            GetTrunkThreshold = parameters.GetTrunkThreshold;
            _fitnessFunction = new GeneticDistributionFitness().Make();
        }

        public IList<PointSet> Filter(IList<PointSet> pointSets)
        {
            return pointSets.Where(PointSetIsTrunk).ToList();
        }


        protected virtual IIndividual FindBestParameters(PointSet group)
        {
            if (group == null || group.Center.Z < 0.1)
            {
                return null;
            }

            GeneticDistributionFitness.TestedDistributions = group
                .Normalized()
                .GetDistribution(DistributionResolution, MathUtils.EDimension.X, MathUtils.EDimension.Y)
                .Select(x => x.Normalized()).ToArray();

            GeneticAlgorithm.Population.Initialize();
            GeneticAlgorithm.Reset();
            GeneticAlgorithm.Run();

            var best = GeneticAlgorithm.Population.Generation > 0
                ? GeneticAlgorithm.Population.HeavenPolicy.Memory[0]
                : GeneticAlgorithm.Population.Best;
            return best;
        }

        public bool PointSetIsTrunk(PointSet pointSet)
        {
            if (pointSet == null)
            {
                return false;
            }

            var bestPossibleParameters = FindBestParameters(pointSet);
            if (bestPossibleParameters is null)
            {
                return false;
            }
            var fitness = _fitnessFunction.GetValue(bestPossibleParameters.Phenotype);

            var isTrunk = fitness <= GetTrunkThreshold(pointSet.Center.Z);
            if (!isTrunk)
            {
                return false;
            }

            foreach (var t in pointSet)
            {
                t.Intensity = (float) fitness;
            }

            return true;
        }
    }

    public class GeneticDistributionFilterParams
    {
        public GeneticAlgorithm GeneticAlgorithm { get; set; }
        public int DistributionResolution { get; set; } = 32;
        public Func<double, double> GetTrunkThreshold { get; set; } = _ => 0.22;
    }
}