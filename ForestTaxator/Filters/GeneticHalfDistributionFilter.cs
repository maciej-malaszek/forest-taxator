using System;
using System.Linq;
using ForestTaxator.Extensions;
using ForestTaxator.Model;
using ForestTaxator.Utils;
using GeneticToolkit.Interfaces;
using GeneticToolkit.Phenotypes.Collective;

namespace ForestTaxator.Filters
{
    public class GeneticHalfDistributionFilter : GeneticDistributionFilter
    {
        private readonly Distribution _testedDistribution;

        public GeneticHalfDistributionFilter(GeneticDistributionFilterParams parameters) : base(parameters)
        {
            _testedDistribution = new Distribution(DistributionResolution);
        }

        protected override double Fitness(IPhenotype p)
        {
            var phenotype = (CollectivePhenotype<ParabolicParameters>) p;

            var x = phenotype.GetValue();
            var distributions = GenerateDistributions(x, DistributionResolution).ToArray();

            var summedDistribution = new Distribution((int) (distributions.Max(x => x.Size)));

            for (var i = 0; i < distributions[1].Size; i++)
            {
                var offsetIndex = (int) ((i + distributions[1].Size - 1) % distributions[1].Size);
                summedDistribution[i] = distributions[0][i] + distributions[1][offsetIndex];
            }

            var difference = summedDistribution - _testedDistribution;

            var average = difference.Average();
            var standardDeviation = difference.StandardDeviation(average);

            var fitness = Math.Abs(average) + Math.Abs(standardDeviation);

            return fitness;
        }

        protected override IIndividual FindBestParameters(PointSet @group)
        {
            if (group == null || group.Center.Z < 0.1)
            {
                return null;
            }

            var distributions = group
                .Normalized()
                .GetDistribution(DistributionResolution, MathUtils.EDimension.X, MathUtils.EDimension.Y)
                .Select(x => x.Normalized()).ToArray();
            
            for (var i = 0; i < distributions[1].Size; i++)
            {
                var offsetIndex = (int) ((i + distributions[1].Size - 1) % distributions[1].Size);
                _testedDistribution[i] = distributions[0][i] + distributions[1][offsetIndex];
            }

            GeneticAlgorithm.Population.Initialize();
            GeneticAlgorithm.Reset();
            GeneticAlgorithm.Run();

            var best = GeneticAlgorithm.Population.Generation > 0
                ? GeneticAlgorithm.Population.HeavenPolicy.Memory[0]
                : GeneticAlgorithm.Population.Best;
            return best;
        }
    }
}