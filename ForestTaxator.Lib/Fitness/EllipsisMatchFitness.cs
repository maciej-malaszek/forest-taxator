using System;
using ForestTaxator.Lib.Model;
using ForestTaxator.Lib.Utils;
using GeneticToolkit.FitnessFunctions;
using GeneticToolkit.Interfaces;
using GeneticToolkit.Phenotypes.Collective;

namespace ForestTaxator.Lib.Fitness
{
    public class EllipsisMatchFitness : IFitnessFunctionFactory
    {
        private static readonly IFitnessFunction _fitnessFunction = new FitnessFunction(Fitness);
        public static PointSet AnalyzedPointSet;
        public static double EccentricityThreshold { get; set; }
        public static double BufferWidth { get; set; }
        public static double Fitness(IPhenotype p)
        {
            var phenotype = (CollectivePhenotype<EllipticParameters>)p;

            var x = phenotype.GetValue();

            var ellipsis = new Ellipsis(x, AnalyzedPointSet.Center.Z);

            if (ellipsis.Eccentricity > EccentricityThreshold)
            {
                return double.MaxValue;
            }

            double fitness = 0;
            var counter = 0;

            var offsetFoci1 = x.F1 + AnalyzedPointSet.Center;
            var offsetFoci2 = x.F2 + AnalyzedPointSet.Center;
            foreach (var point in AnalyzedPointSet)
            {
                // Distance of point from Foci 1
                var pf1 = MathUtils.Distance(point, offsetFoci1, MathUtils.EDistanceMetric.Euclidean, MathUtils.EDimension.X,
                    MathUtils.EDimension.Y);
                
                // Distance of point from Foci 2
                var pf2 = MathUtils.Distance(point, offsetFoci2, MathUtils.EDistanceMetric.Euclidean, MathUtils.EDimension.X,
                    MathUtils.EDimension.Y);
                
                // Given two fixed points F1 and F2 called the foci, and a distance 2a, which is greater than the distance between the foci,
                // the ellipse is the set of points P such that the sum of the distances PF1, PF2 is equal to 2a:
                var dist = pf1 + pf2 - 2 * x.A;

                var absoluteDistance = Math.Abs(dist); 
                if (absoluteDistance > BufferWidth)
                {
                    fitness += absoluteDistance;
                    counter++;
                }
            }
            fitness /= counter;
           
            return fitness;
        }
        public IFitnessFunction Make()
        {
            return _fitnessFunction;
        }
    }
}