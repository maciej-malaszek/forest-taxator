using System;
using System.Diagnostics.CodeAnalysis;
using ForestTaxator.Model;
using ForestTaxator.Utils;
using GeneticToolkit;
using GeneticToolkit.Genotypes.Collective;
using GeneticToolkit.Interfaces;
using GeneticToolkit.Phenotypes.Collective;
using GeneticToolkit.Utils.FitnessFunctions;

namespace ForestTaxator.Algorithms
{
    public class GeneticEllipseMatch
    {
        private PointSet _analyzedPointSet;
        public double EccentricityThreshold { get; set; }
        public double BufferWidth { get; set; }
        public GeneticAlgorithm GeneticAlgorithm { get; set; }
        
        public IFitnessFunction GetFitnessFunction() => new FitnessFunction(Fitness);
        
        public virtual double Fitness(IPhenotype p)
        {
            var phenotype = (CollectivePhenotype<EllipticParameters>)p;

            var x = phenotype.GetValue();

            var ellipsis = new Ellipsis(x, _analyzedPointSet.Center.Z);

            if (ellipsis.Eccentricity > EccentricityThreshold)
            {
                return double.MaxValue;
            }

            double fitness = 0;

            var offsetFoci1 = x.F1 + _analyzedPointSet.Center;
            var offsetFoci2 = x.F2 + _analyzedPointSet.Center;
            foreach (var point in _analyzedPointSet)
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
                    fitness += absoluteDistance / _analyzedPointSet.Count;
                }
            }
           
            return fitness;
        }
        
        

        public virtual IIndividual FindBestIndividual(PointSet @group, Ellipsis initializer = null)
        {
            if (group == null || group.Center.Z < 0.1)
            {
                return null;
            }

            _analyzedPointSet = group;

            ReinitializePopulation(initializer);
            GeneticAlgorithm.Reset();
            GeneticAlgorithm.Run();

            var best = GeneticAlgorithm.Population.Generation > 0
                ? GeneticAlgorithm.Population.HeavenPolicy.Memory[0]
                : GeneticAlgorithm.Population.Best;
            return best;
        }

        [SuppressMessage("ReSharper.DPA", "DPA0001: Memory allocation issues")]
        [SuppressMessage("ReSharper.DPA", "DPA0001: Memory allocation issues")]
        public virtual void ReinitializePopulation(Ellipsis initializer = null)
        {
            GeneticAlgorithm.Population.Initialize(() =>
            {
                const float OffsetDeviationInMeters = 0.05f;
                const float RadiusDeviationInMeters = 0.05f;
                
                var random = new Random();
                var individuals = new IIndividual[GeneticAlgorithm.Population.Size];
                
                var offsetStepSize = OffsetDeviationInMeters / GeneticAlgorithm.Population.Size;
                var halfOffsetDeviationInMeters = OffsetDeviationInMeters / 2;

                var radiusDeviationStep = RadiusDeviationInMeters / GeneticAlgorithm.Population.Size;
                var halfRadiusDeviation = RadiusDeviationInMeters / 2;

                var defaultRadius = (float) (0.5f * Math.Min(_analyzedPointSet.BoundingBox.Width, _analyzedPointSet.BoundingBox.Depth));
                
                for (var i = 0; i < GeneticAlgorithm.Population.Size; i++)
                {
                    var x1 = (float)((initializer?.FirstFocal.X ?? random.NextDouble()*0.5f) + i * offsetStepSize - halfOffsetDeviationInMeters);
                    var x2 = (float)((initializer?.SecondFocal.X ?? random.NextDouble()*0.5f) + i * offsetStepSize - halfOffsetDeviationInMeters);
                    var y1 = (float)((initializer?.FirstFocal.Y ?? random.NextDouble()*0.5f) + i * offsetStepSize - halfOffsetDeviationInMeters);
                    var y2 = (float)((initializer?.SecondFocal.Y ?? random.NextDouble()*0.5f) + i * offsetStepSize - halfOffsetDeviationInMeters);
                    var genotype = new CollectiveGenotype<EllipticParameters>(new EllipticParameters
                        {
                            X1 = x1,
                            X2 = x2,
                            Y1 = y1,
                            Y2 = y2,
                            A = (float) (initializer?.MajorRadius ?? defaultRadius + radiusDeviationStep * i - halfRadiusDeviation)
                        }
                    );
                    individuals[i] = GeneticAlgorithm.Population.IndividualFactory.CreateFromGenotype(genotype);
                }

                return individuals;
            });
        }
    }
}