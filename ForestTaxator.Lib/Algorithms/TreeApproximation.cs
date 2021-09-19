using System;
using System.Collections.Generic;
using System.Linq;
using ForestTaxator.Lib.Extensions;
using ForestTaxator.Lib.Model;
using ForestTaxator.Lib.Utils;
using GeneticToolkit.Interfaces;
using GeneticToolkit.Phenotypes.Collective;

namespace ForestTaxator.Lib.Algorithms
{
    public class TreeApproximation
    {
        private readonly GeneticEllipseMatch _geneticEllipseMatch;
        private readonly double _eccentricityThreshold;
        private readonly double _fitnessThreshold;

        public TreeApproximation(GeneticEllipseMatch geneticEllipseMatch, double eccentricityThreshold, double fitnessThreshold)
        {
            _geneticEllipseMatch = geneticEllipseMatch;
            _eccentricityThreshold = eccentricityThreshold;
            _fitnessThreshold = fitnessThreshold;
        }

        public Tree ApproximateTree(Tree tree, double treeHeight = 0, float sliceHeight = 0.1f, bool smooth = false)
        {
            if (tree is null)
            {
                return null;
            }

            var nodes = tree.GetAllNodesAsVector();
            var population = _geneticEllipseMatch.GeneticAlgorithm.Population;
            ApproximateGenetically(nodes, population);

            var ellipses = nodes.Select(n => n.Ellipse).ToArray();
            var correctEllipses = ellipses.Where(e => e != null && double.IsNaN(e.MajorRadius) == false).ToArray();
            if (correctEllipses.Length == 0 || (float)correctEllipses.Length / ellipses.Length < 0.25)
            {
                return null;
            }

            var height = treeHeight == 0 ? tree.GetHighestNode().Center.Z : treeHeight;
            foreach (var node in nodes)
            {
                node.Center.Z = (float)Math.Round(node.Center.Z, 1, MidpointRounding.ToEven);

                if (node.Ellipse is null)
                {
                    continue;
                }

                node.Ellipse.SetFirstFocal(MathUtils.EDimension.Z, (float)node.Center.Z);
                node.Ellipse.SetSecondFocal(MathUtils.EDimension.Z,  (float)node.Center.Z);
            }

            //terrain height was compensated during slicing step
            tree.RegressMissingLevels(0, height, sliceHeight, smooth);

            return tree;
        }

        private void ApproximateGenetically(IEnumerable<Tree.Node> nodes, IEvolutionaryPopulation population)
        {
            var index = 0;
            var nodesList = nodes.ToList();
            var count = nodesList.Count;
            foreach (var node in nodesList)
            {
                var individual = _geneticEllipseMatch.FindBestIndividual(node.PointSet, node.Parent?.Ellipse);
                var fitness = population.FitnessFunction.GetValue(individual);
                var ellipticParameters = ((CollectivePhenotype<EllipticParameters>)individual.Phenotype).GetValue();
                var ellipse = new Ellipsis(ellipticParameters, node.PointSet.Center.Z)
                {
                    Intensity = fitness
                };

                ellipse.SetFirstFocal(MathUtils.EDimension.X, ellipse.FirstFocal.X + node.PointSet.Center.X);
                ellipse.SetFirstFocal(MathUtils.EDimension.Y, ellipse.FirstFocal.Y + node.PointSet.Center.Y);
                
                ellipse.SetSecondFocal(MathUtils.EDimension.X, ellipse.SecondFocal.X + node.PointSet.Center.X);
                ellipse.SetSecondFocal(MathUtils.EDimension.Y, ellipse.SecondFocal.Y + node.PointSet.Center.Y);
                
                if (ellipse.Eccentricity < _eccentricityThreshold && fitness < _fitnessThreshold && double.IsNaN(fitness) == false)
                {
                    node.Ellipse = ellipse;
                }

                ProgressTracker.Progress(EProgressStage.TreeApproximation, "Approximating Tree segments", index++, count);
            }
        }
    }
}