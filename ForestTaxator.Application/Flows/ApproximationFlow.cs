using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ForestTaxator.Application.Commands.Analyze;
using ForestTaxator.Application.Models;
using ForestTaxator.Lib.Algorithms;
using ForestTaxator.Lib.Data.GPD;
using ForestTaxator.Lib.Data.XYZ;
using ForestTaxator.Lib.Extensions;
using ForestTaxator.Lib.Fitness;
using ForestTaxator.Lib.Model;
using GeneticToolkit;
using GeneticToolkit.Comparisons;
using GeneticToolkit.Factories;
using GeneticToolkit.Genotypes.Collective;
using GeneticToolkit.Interfaces;
using GeneticToolkit.Phenotypes.Collective;
using GeneticToolkit.Utils.Configuration;
using Newtonsoft.Json;
using Serilog;

namespace ForestTaxator.Application.Flows
{
    public static class ApproximationFlow
    {
        private static GeneticEllipseMatch PrepareGeneticEllipseMatchAlgorithm(EllipsisMatchConfiguration configuration, ILogger logger)
        {
            var geneticEllipseMatch = new GeneticEllipseMatch
            {
                BufferWidth = configuration.BufferWidth,
                EccentricityThreshold = configuration.InvalidEccentricityThreshold,
            };
            var factory = new CollectivePhenotypeFactory<EllipticParameters>();
            var individualFactory =
                new IndividualFactory<CollectiveGenotype<EllipticParameters>, CollectivePhenotype<EllipticParameters>, EllipsisMatchFitness>(
                    factory);
            var compareCriteria = new SimpleComparison<EllipsisMatchFitness>(EOptimizationMode.Minimize);

            if (!File.Exists(configuration.GeneticAlgorithmConfigurationFile))
            {
                logger.Fatal("Genetic configuration file does not exist!");
                Environment.Exit(0);
            }

            var geneticAlgorithmConfigurationContent = File.ReadAllText(configuration.GeneticAlgorithmConfigurationFile);
            var geneticAlgorithmInfo = JsonConvert.DeserializeObject<DynamicObjectInfo>(geneticAlgorithmConfigurationContent);
            var geneticAlgorithm = DynamicObjectFactory<GeneticAlgorithm>.Build(geneticAlgorithmInfo);
            geneticAlgorithm.Population.CompareCriteria = compareCriteria;
            geneticAlgorithm.Population.IndividualFactory = individualFactory;
            geneticEllipseMatch.GeneticAlgorithm = geneticAlgorithm;

            return geneticEllipseMatch;
        }

        private static void BuildNodes(
            Dictionary<int, PointSet> nodeDictionary, Dictionary<int, int[]> parentChildrenDictionary, Tree.Node node, int nodeId)
        {
            if (parentChildrenDictionary.ContainsKey(nodeId) == false)
            {
                return;
            }

            foreach (var childId in parentChildrenDictionary[nodeId])
            {
                var childPointSet = nodeDictionary[childId];
                var child = node.AddChild(childPointSet);
                BuildNodes(nodeDictionary, parentChildrenDictionary, child, childId);
            }
        }

        private static Tree BuildTree(Dictionary<int, int> childParentDictionary, Dictionary<int, PointSet> nodeDictionary)
        {
            var parentChildrenDictionary = childParentDictionary
                .GroupBy(p => p.Value)
                .ToDictionary(p => p.Key, p => p.Select(t => t.Key).ToArray());

            var tree = new Tree();
            var rootPointSet = nodeDictionary[0];
            tree.CreateRoot(rootPointSet);

            BuildNodes(nodeDictionary, parentChildrenDictionary, tree.Root, 0);
            return tree;
        }

        public static Task Execute(ApproximateCommand command, ILogger logger)
        {
            if (Directory.Exists(command.Input) == false)
            {
                logger.Fatal("Input path does not exist!");
                Environment.Exit(0);
            }

            var inputFiles = Directory.GetFiles(command.Input).Where(p => p.EndsWith(".gpd")).ToList();
            if (inputFiles.Count == 0)
            {
                logger.Fatal("Specified directory does not contain GPD files");
                Environment.Exit(0);
            }

            var configurationFileContent = File.ReadAllText(command.ConfigurationFile);
            var configuration = JsonConvert.DeserializeObject<ApproximationConfiguration>(configurationFileContent);
            if (configuration == null)
            {
                logger.Fatal("Could not parse Configuration file!");
                Environment.Exit(0);
            }

            var geneticEllipseMatch = PrepareGeneticEllipseMatchAlgorithm(configuration.EllipsisMatchConfiguration, logger);
            var approximation = new TreeApproximation(geneticEllipseMatch, 0.8, 0.01f);
            var trees = ParseTrees(inputFiles);

            var treeId = 0;
            foreach (var detectedTree in trees)
            {
                var tree = approximation.ApproximateTree(detectedTree);
                if (command.ExportPreview)
                {
                    ExportPreview(command, tree, $"T{treeId}.e.xyz");
                }

                var nodes = tree.GetAllNodesAsVector();
                var treeInfo = new TreeInfo
                {
                    Height = tree.GetHighestNode().Center.Z - tree.Root.Center.Z
                };
                var nodeDictionary = nodes
                    .Select((node, id) => new { node, id })
                    .ToDictionary(value => value.node, value => value.id);
                foreach (var entry in nodeDictionary)
                {
                    var node = entry.Key;
                    treeInfo.SliceInfos.Add(new TreeSliceInfo
                    {
                        Center = node.Center,
                        EllipseEccentricity = node.Ellipse?.Eccentricity,
                        EllipseFocis = node.Ellipse is null ? null : new[] { node.Ellipse.FirstFocal, node.Ellipse.SecondFocal },
                        EllipseMajorSemiAxis = node.Ellipse?.MajorRadius,
                        Id = entry.Value.ToString(),
                        ParentId = node.Parent is null ? null : nodeDictionary[node.Parent].ToString()
                    });
                }

                var serialized = JsonConvert.SerializeObject(treeInfo);
                Directory.CreateDirectory(command.Output);
                File.WriteAllText(Path.Join(command.Output, $"T{treeId}.json"), serialized);
            }

            return Task.CompletedTask;
        }

        private static void ExportPreview(ApproximateCommand command, Tree tree, string filename)
        {
            using var writer = new XyzWriter(Path.Join(command.Output, filename));
            var ellipses = tree.GetAllNodesAsVector().Select(node => node.Ellipse).Where(ellipsis => ellipsis != null);
            foreach (var ellipsis in ellipses)
            {
                ellipsis.ExportToStream(writer);
            }
        }

        private static List<Tree> ParseTrees(List<string> inputFiles)
        {
            var trees = new List<Tree>();

            foreach (var inputFile in inputFiles)
            {
                var reader = new GpdReader(inputFile);
                PointSet pointSet;
                var nodeDictionary = new Dictionary<int, PointSet>();
                var childParentDictionary = new Dictionary<int, int>();

                do
                {
                    pointSet = reader.ReadPointSet(out var metaData);
                    if (pointSet is null)
                    {
                        break;
                    }

                    var ids = metaData.Comment.Split(";");
                    var groupId = int.Parse(ids[0].Split("+")[1]);
                    var parentId = int.Parse(ids[1].Split("+")[1]);

                    nodeDictionary.Add(groupId, pointSet);
                    childParentDictionary.Add(groupId, parentId);
                } while (pointSet != null);

                var tree = BuildTree(childParentDictionary, nodeDictionary);
                trees.Add(tree);
            }

            return trees;
        }
    }
}