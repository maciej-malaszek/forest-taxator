using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ForestTaxator.Extensions;
using ForestTaxator.Model;
using ForestTaxator.Utils;
using GeneticToolkit;
using GeneticToolkit.Interfaces;
using GeneticToolkit.Phenotypes.Collective;
using GeneticToolkit.Utils.FitnessFunctions;

namespace ForestTaxator.Filters
{
    /// <summary>
    /// This filter utilizes genetic algorithm to find parameters of parabolic function that will generate two-dimensional point distribution
    /// matching to actual point set.
    /// </summary>
    public class GeneticDistributionFilter : IPointSetFilter
    {
        public int DistributionResolution { get; set; }
        
        /// <summary>
        /// Maximum allowed fitness value depending on slice height in meters.
        /// </summary>
        public Func<double, double> GetTrunkThreshold { get; set; }
        public GeneticAlgorithm GeneticAlgorithm { get; set; }
        
        private Distribution[] _testedDistributions;

        public GeneticDistributionFilter(GeneticDistributionFilterParams parameters)
        {
            GeneticAlgorithm = parameters.GeneticAlgorithm;
            DistributionResolution = parameters.DistributionResolution;
            GetTrunkThreshold = parameters.GetTrunkThreshold;
        }
        public IList<PointSet> Filter(IList<PointSet> pointSets)
        {
            return pointSets.Where(PointSetIsTrunk).ToList();
        }

        public IFitnessFunction GetFitnessFunction() => new FitnessFunction(Fitness);

        private static IList<Distribution> GenerateDistributions(ParabolicParameters parameters, int steps)
        {
            var dataX = new double[steps];
            var dataY = new double[steps];
            for (var i = 0; i < steps; i++)
            {
                var x = (float)(i - steps / 2) / steps;
                dataX[i] = parameters.A1 * Math.Pow(x, 2.0) + parameters.B1 * x + parameters.C1;
                dataY[i] = parameters.A2 * Math.Pow(x, 2.0) + parameters.B2 * x + parameters.C2;
            }

            return new[] { new Distribution(dataX), new Distribution(dataY) };
        }
        
        private double Fitness(IPhenotype p)
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

        private IIndividual FindBestParameters(PointSet group)
        {
            if (group == null || group.Center.Z < 0.1)
            {
                return null;
            }

            _testedDistributions = group
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
            var fitness = Fitness(bestPossibleParameters.Phenotype);
            return fitness <= GetTrunkThreshold(pointSet.Center.Z);
        }
    }

    public class GeneticDistributionFilterParams
    {
        public GeneticAlgorithm GeneticAlgorithm { get; set; }
        public int DistributionResolution { get; set; } = 32;
        public Func<double, double> GetTrunkThreshold { get; set; } = _ => 0.22;
    }
}