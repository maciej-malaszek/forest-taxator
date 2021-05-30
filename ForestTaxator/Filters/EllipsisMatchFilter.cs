using System;
using System.Collections.Generic;
using System.Linq;
using ForestTaxator.Algorithms;
using ForestTaxator.Model;
using ForestTaxator.Utils;
using GeneticToolkit.Phenotypes.Collective;


namespace ForestTaxator.Filters
{
    public class EllipsisMatchFilter : IPointSetFilter
    {
        public double EccentricityThreshold { get; set; }
        public double FitnessThreshold { get; set; } = 0.2f;
        public GeneticEllipseMatch GeneticEllipseMatch { get; set; }
        
        public IList<PointSet> Filter(IList<PointSet> pointSets)
        {
            return pointSets.Where((x, i) => ConditionMatched(x,i,pointSets.Count)).ToList();
        }
        public bool ConditionMatched(PointSet pointSet, int index, int total)
        {
            var bestPossibleIndividual = GeneticEllipseMatch.FindBestIndividual(pointSet);
            var fitness = GeneticEllipseMatch.Fitness(bestPossibleIndividual.Phenotype);
            var ellipseParameters = ((CollectivePhenotype<EllipticParameters>)bestPossibleIndividual.Phenotype).GetValue();
            var ellipse = new Ellipsis(ellipseParameters, 0);
            foreach (var t in pointSet)
            {
                t.Intensity = (float) fitness;
            }

            return ellipse.Eccentricity < EccentricityThreshold && fitness < FitnessThreshold;
        }
    }
}