using System;
using System.Collections.Generic;
using System.Linq;
using ForestTaxator.Model;
using ForestTaxator.Utils;
using GeneticToolkit.Interfaces;
using GeneticToolkit.Phenotypes.Collective;

namespace ForestTaxator.Algorithms
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
        public Tree ApproximateTree(Tree tree, Terrain terrain, double? treeHeight = null, float sliceHeight = 0.1f)
        {
            var nodes = tree.GetAllNodesAsVector();
            var population = _geneticEllipseMatch.GeneticAlgorithm.Population;
            ApproximateGenetically(nodes, population);

            var terrainHeight = terrain.GetHeight(tree.Root.Center);
            
            tree.RegressMissingLevels(terrainHeight, treeHeight ?? tree.GetHighestNode().Center.Z, sliceHeight);
            //
            // nodes = tree.GetAllNodesAsVector();
            // foreach (var node in nodes)
            // {
            //     // node.Ellipse.SetFirstFocal(MathUtils.EDimension.Z, (int)(10 * (node.Ellipse.FirstFocal.Z + terrainHeight)) / 10.0f);
            //     // node.Ellipse.SetSecondFocal(MathUtils.EDimension.Z, (int)(10 * (node.Ellipse.SecondFocal.Z + terrainHeight)) / 10.0f);
            // }

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
                var ellipticParameters = ((CollectivePhenotype<EllipticParameters>) individual.Phenotype).GetValue();
                var ellipse = new Ellipsis(ellipticParameters, node.PointSet.Center.Z)
                {
                    Intensity = fitness
                };

                ellipse.FirstFocal.X += node.PointSet.Center.X;
                ellipse.SecondFocal.X += node.PointSet.Center.X;
                ellipse.FirstFocal.Y += node.PointSet.Center.Y;
                ellipse.SecondFocal.Y += node.PointSet.Center.Y;
                node.Ellipse = ellipse;

                if (ellipse.Eccentricity < _eccentricityThreshold && fitness < _fitnessThreshold)
                {
                    node.Ellipse = ellipse;
                }
                ProgressTracker.Progress(EProgressStage.TreeApproximation, "Approximating Tree segments", index++, count);
            }
        }
    }
}