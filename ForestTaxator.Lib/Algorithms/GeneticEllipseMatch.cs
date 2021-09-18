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
        private static readonly Random _random = new();
        
        public virtual IIndividual FindBestIndividual(PointSet group, Ellipsis initializer = null)
        {
            if (group == null || group.Center.Z < 0.1)
            {
                return null;
            }

            EllipsisMatchFitness.BufferWidth = BufferWidth;
            EllipsisMatchFitness.EccentricityThreshold = EccentricityThreshold;
            EllipsisMatchFitness.AnalyzedPointSet = group;

            ReinitializePopulation(initializer);
            GeneticAlgorithm.Reset();
            GeneticAlgorithm.Run();

            var best = GeneticAlgorithm.Population.Generation > 0
                ? GeneticAlgorithm.Population.HeavenPolicy.Memory[0]
                : GeneticAlgorithm.Population.Best;
            return best;
        }

        private static float Coalesce(double? val) => (float)(val ?? _random.NextDouble() * 0.5f - 0.25f);

        [SuppressMessage("ReSharper.DPA", "DPA0001: Memory allocation issues")]
        [SuppressMessage("ReSharper.DPA", "DPA0001: Memory allocation issues")]
        public virtual void ReinitializePopulation(Ellipsis initializer = null)
        {
            GeneticAlgorithm.Population.Initialize(() =>
            {
                const float OffsetDeviationInMeters = 0.2f;
                const float RadiusDeviationInMeters = 0.1f;
                const float RadiusBaseOffset = RadiusDeviationInMeters / 2;
                const int RadiusSteps = 4;
                
                var individuals = new IIndividual[GeneticAlgorithm.Population.Size];

                var radiusStepSize = RadiusDeviationInMeters / RadiusSteps;
                var offsetStepSize = OffsetDeviationInMeters / GeneticAlgorithm.Population.Size * RadiusSteps;
                var halfOffsetDeviationInMeters = OffsetDeviationInMeters / 2;

                var analyzedPointSet = EllipsisMatchFitness.AnalyzedPointSet;

                var defaultRadius = 0.5f * Math.Min(analyzedPointSet.BoundingBox.Width, analyzedPointSet.BoundingBox.Depth);
                
                for (var i = 0; i < GeneticAlgorithm.Population.Size / RadiusSteps; i++)
                {
                    // [-halfOffsetDeviationInMeters; halfOffsetDeviationInMeters]
                    var offset = i * offsetStepSize - halfOffsetDeviationInMeters;
                    var x1 = Coalesce(initializer?.FirstFocal.X) + offset;
                    var x2 = Coalesce(initializer?.SecondFocal.X) + offset;
                    var y1 = Coalesce(initializer?.FirstFocal.Y) + offset;
                    var y2 = Coalesce(initializer?.SecondFocal.Y) + offset;

                    for (var j = 0; j < RadiusSteps; j++)
                    {
                        var radiusBase = initializer?.MajorRadius ?? defaultRadius;
                        var radiusOffset = j * radiusStepSize - RadiusBaseOffset;
                        var radius = radiusBase + radiusOffset;
                    
                        var genotype = new CollectiveGenotype<EllipticParameters>(new EllipticParameters
                            {
                                X1 = x1,
                                X2 = x2,
                                Y1 = y1,
                                Y2 = y2,
                                A = (float) radius
                            }
                        );
                        var index = i * RadiusSteps + j;
                        if (index < individuals.Length)
                        {
                            individuals[index] = GeneticAlgorithm.Population.IndividualFactory.CreateFromGenotype(genotype);
                        }
                    }
                    
                    
                }

                return individuals;
            });
        }
    }
}