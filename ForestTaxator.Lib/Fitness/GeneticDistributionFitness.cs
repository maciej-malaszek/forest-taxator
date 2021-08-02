using System;
using System.Collections.Generic;
using System.Linq;
using ForestTaxator.Lib.Model;
using ForestTaxator.Lib.Utils;
using GeneticToolkit.FitnessFunctions;
using GeneticToolkit.Interfaces;
using GeneticToolkit.Phenotypes.Collective;

namespace ForestTaxator.Lib.Fitness
{
    public class GeneticDistributionFitness : IFitnessFunctionFactory
    {
        public static Distribution[] TestedDistributions { get; set; }
        public static int DistributionResolution { get; set; }
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

        private static readonly IFitnessFunction _fitnessFunction = new FitnessFunction(Fitness);

        private static double Fitness(IPhenotype p)
        {
            var phenotype = (CollectivePhenotype<ParabolicParameters>) p;

            var x = phenotype.GetValue();
            var distributions = GenerateDistributions(x, DistributionResolution).ToArray();
            var differences = new Distribution[2];
            for (var i = 0; i < differences.Length; i++)
            {
                differences[i] = distributions[i] - TestedDistributions[i];
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
        public IFitnessFunction Make()
        {
            return _fitnessFunction;
        }
    }
}