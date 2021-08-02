using System;
using System.Diagnostics.CodeAnalysis;
using ForestTaxator.Lib.Fitness;
using ForestTaxator.Lib.Model;
using GeneticToolkit;
using GeneticToolkit.Genotypes.Collective;
using GeneticToolkit.Interfaces;

namespace ForestTaxator.Lib.Algorithms
{
    public class GeneticEllipseMatch
    {
        public double EccentricityThreshold { get; set; }
        public double BufferWidth { get; set; }
        public GeneticAlgorithm GeneticAlgorithm { get; set; }
        
        public virtual IIndividual FindBestIndividual(PointSet group, Ellipsis initializer = null)
        {
            if (group == null || group.Center.Z < 0.1)
            {
                return null;
            }

            EllipsisMatchFitness.AnalyzedPointSet = group;

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
                const float RadiusDeviationInMeters = 0.25f;
                
                var random = new Random();
                var individuals = new IIndividual[GeneticAlgorithm.Population.Size];
                
                var offsetStepSize = OffsetDeviationInMeters / GeneticAlgorithm.Population.Size;
                var halfOffsetDeviationInMeters = OffsetDeviationInMeters / 2;

                var analyzedPointSet = EllipsisMatchFitness.AnalyzedPointSet;

                var defaultRadius = 0.5f * Math.Min(analyzedPointSet.BoundingBox.Width, analyzedPointSet.BoundingBox.Depth);
                
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
                            A = (float) (initializer?.MajorRadius ?? defaultRadius + random.NextDouble()*RadiusDeviationInMeters)
                        }
                    );
                    individuals[i] = GeneticAlgorithm.Population.IndividualFactory.CreateFromGenotype(genotype);
                }

                return individuals;
            });
        }
    }
}