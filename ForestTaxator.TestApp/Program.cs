using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CommandLine;
using ConsoleProgressBar;
using ForestTaxator.Algorithms;
using ForestTaxator.Data;
using ForestTaxator.Data.PCD;
using ForestTaxator.Data.XYZ;
using ForestTaxator.Extensions;
using ForestTaxator.Filters;
using ForestTaxator.Model;
using ForestTaxator.TestApp.Commands;
using ForestTaxator.Utils;
using GeneticToolkit;
using GeneticToolkit.Comparisons;
using GeneticToolkit.Crossovers;
using GeneticToolkit.Genotypes.Collective;
using GeneticToolkit.Interfaces;
using GeneticToolkit.Mutations;
using GeneticToolkit.Phenotypes.Collective;
using GeneticToolkit.Policies.Heaven;
using GeneticToolkit.Policies.Incompatibility;
using GeneticToolkit.Policies.Mutation;
using GeneticToolkit.Policies.Resize;
using GeneticToolkit.Policies.Stop;
using GeneticToolkit.Populations;
using GeneticToolkit.Selections;
using GeneticToolkit.Utils;
using GeneticToolkit.Utils.Factories;
using ProgressHierarchy;

namespace ForestTaxator.TestApp
{
    class Program
    {
        private static HierarchicalProgress _consoleProgressBar;
        static void Main(string[] args)
        {
            using var progressBar = new ProgressBar
            {
                BarCharacter = '\u2588',
                Width = 0.8,
                StatusOnSeparateLine = false
            };
            _consoleProgressBar = progressBar.HierarchicalProgress.Fork(1);

            Parser.Default.ParseArguments<DetectVerb, ConvertVerb>(args)
                .WithParsed<DetectVerb>(DetectTree)
                .WithParsed<ConvertVerb>(ConvertFile);
            _consoleProgressBar.Dispose();
        }

        private static GeneticEllipseMatch PrepareGeneticEllipseMatchAlgorithm()
        {
            var geneticEllipseMatch = new GeneticEllipseMatch
            {
                BufferWidth = 0.005,
                EccentricityThreshold = 0.85,
            };
            var fitnessFunction = geneticEllipseMatch.GetFitnessFunction();
            var factory = new CollectivePhenotypeFactory<EllipticParameters>();
            var individualFactory =
                new IndividualFactory<CollectiveGenotype<EllipticParameters>, CollectivePhenotype<EllipticParameters>>(factory,
                    fitnessFunction);
            var compareCriteria = new SimpleComparison(fitnessFunction, EOptimizationMode.Minimize);
            var geneticAlgorithm = new GeneticAlgorithm
            {
                StopConditions = new IStopCondition[]
                {
                    new TimeSpanCondition(TimeSpan.FromSeconds(0.35f)),
                    new PopulationDegradation(0.9f),
                    new SufficientIndividual(fitnessFunction, 0.005f)
                },
                StopConditionMode = EStopConditionMode.Any,
                Population = new Population(fitnessFunction, 60)
                {
                    CompareCriteria = compareCriteria,
                    Crossover = new SinglePointCrossover(),
                    HeavenPolicy = new OneGod(),
                    Mutation = new ArithmeticMutation(new[]
                        {
                            new Range<float>(-10, 10),
                            new Range<float>(-10, 10),
                            new Range<float>(-10, 10),
                            new Range<float>(-10, 10),
                            new Range<float>(-10, 10)
                        },
                        new[]
                        {
                            ArithmeticMutation.EMode.Byte,
                            ArithmeticMutation.EMode.Byte,
                            ArithmeticMutation.EMode.Byte,
                            ArithmeticMutation.EMode.Byte,
                            ArithmeticMutation.EMode.Byte
                        }
                    ),
                    MutationPolicy = new HesserMannerMutation(1, 1, 0.1f),
                    ResizePolicy = new ConstantResizePolicy(),
                    IndividualFactory = individualFactory,
                    IncompatibilityPolicy = new AllowAll(),
                    SelectionMethod = new Tournament(compareCriteria, 0.01f),
                    StatisticUtilities = new Dictionary<string, IStatisticUtility>()
                }
            };
            geneticEllipseMatch.GeneticAlgorithm = geneticAlgorithm;
            
            return geneticEllipseMatch;
        }

        private static EllipsisMatchFilter PrepareEllipsisMatchFilter()
        {
            var filter = new EllipsisMatchFilter
            {
                FitnessThreshold = 0.01,
                EccentricityThreshold = 0.80,
                GeneticEllipseMatch = new GeneticEllipseMatch
                {
                    BufferWidth = 0.01,
                    EccentricityThreshold = 0.85,
                }
            };
            var fitnessFunction = filter.GeneticEllipseMatch.GetFitnessFunction();

            var factory = new CollectivePhenotypeFactory<EllipticParameters>();
            var individualFactory =
                new IndividualFactory<CollectiveGenotype<EllipticParameters>, CollectivePhenotype<EllipticParameters>>(factory,
                    fitnessFunction);
            var compareCriteria = new SimpleComparison(fitnessFunction, EOptimizationMode.Minimize);
            var geneticAlgorithm = new GeneticAlgorithm
            {
                StopConditions = new IStopCondition[]
                {
                    new TimeSpanCondition(TimeSpan.FromSeconds(0.3f)),
                    new PopulationDegradation(0.9f),
                    new SufficientIndividual(fitnessFunction, filter.FitnessThreshold - 0.001f)
                },
                StopConditionMode = EStopConditionMode.Any,
                Population = new Population(fitnessFunction, 60)
                {
                    CompareCriteria = compareCriteria,
                    Crossover = new SinglePointCrossover(),
                    HeavenPolicy = new OneGod(),
                    Mutation = new ArithmeticMutation(new[]
                        {
                            new Range<float>(-10, 10),
                            new Range<float>(-10, 10),
                            new Range<float>(-10, 10),
                            new Range<float>(-10, 10),
                            new Range<float>(-10, 10)
                        },
                        new[]
                        {
                            ArithmeticMutation.EMode.Byte,
                            ArithmeticMutation.EMode.Byte,
                            ArithmeticMutation.EMode.Byte,
                            ArithmeticMutation.EMode.Byte,
                            ArithmeticMutation.EMode.Byte
                        }
                    ),
                    MutationPolicy = new HesserMannerMutation(1, 1, 0.1f),
                    ResizePolicy = new ConstantResizePolicy(),
                    IndividualFactory = individualFactory,
                    IncompatibilityPolicy = new AllowAll(),
                    SelectionMethod = new Tournament(compareCriteria, 0.01f),
                    StatisticUtilities = new Dictionary<string, IStatisticUtility>()
                }
            };

            filter.GeneticEllipseMatch.GeneticAlgorithm = geneticAlgorithm;
            return filter;
        }

        private static GeneticDistributionFilter PrepareGeneticDistributionFilter(float threshold = 0.22f)
        {
            var geneticDistributionFilter = new GeneticDistributionFilter(new GeneticDistributionFilterParams
            {
                GetTrunkThreshold = _ => threshold
            });
            var fitnessFunction = geneticDistributionFilter.GetFitnessFunction();

            var factory = new CollectivePhenotypeFactory<ParabolicParameters>();
            var individualFactory =
                new IndividualFactory<CollectiveGenotype<ParabolicParameters>, CollectivePhenotype<ParabolicParameters>>(factory,
                    fitnessFunction);
            var compareCriteria = new SimpleComparison(fitnessFunction, EOptimizationMode.Minimize);
            var geneticAlgorithm = new GeneticAlgorithm
            {
                StopConditions = new IStopCondition[]
                {
                    new TimeSpanCondition(TimeSpan.FromSeconds(0.3f)),
                    new PopulationDegradation(0.9f),
                    new SufficientIndividual(fitnessFunction, threshold - 0.001f)
                },
                StopConditionMode = EStopConditionMode.Any,
                Population = new Population(fitnessFunction, 60)
                {
                    CompareCriteria = compareCriteria,
                    Crossover = new SinglePointCrossover(),
                    HeavenPolicy = new OneGod(),
                    Mutation = new ArithmeticMutation(new[]
                        {
                            new Range<float>(-10, 10),
                            new Range<float>(-10, 10),
                            new Range<float>(-10, 10),
                            new Range<float>(-10, 10),
                            new Range<float>(-10, 10)
                        },
                        new[]
                        {
                            ArithmeticMutation.EMode.Byte,
                            ArithmeticMutation.EMode.Byte,
                            ArithmeticMutation.EMode.Byte,
                            ArithmeticMutation.EMode.Byte,
                            ArithmeticMutation.EMode.Byte
                        }
                    ),
                    MutationPolicy = new HesserMannerMutation(1, 1, 0.1f),
                    ResizePolicy = new ConstantResizePolicy(),
                    IndividualFactory = individualFactory,
                    IncompatibilityPolicy = new AllowAll(),
                    SelectionMethod = new Tournament(compareCriteria, 0.01f),
                    StatisticUtilities = new Dictionary<string, IStatisticUtility>()
                }
            };
            geneticDistributionFilter.GeneticAlgorithm = geneticAlgorithm;
            return geneticDistributionFilter;
        }

        private static void ConvertFile(ConvertVerb convertVerb)
        {
            ICloudStreamReader reader = convertVerb.InputFormat.ToLowerInvariant() switch
            {
                "pcd" => new PcdReader(convertVerb.InputFile),
                "xyz" => new XyzReader(convertVerb.InputFile, Encoding.ASCII),
                _ => throw new ArgumentOutOfRangeException()
            };

            var baseOutput = string.Join(".", convertVerb.InputFile.Split(".").SkipLast(1));
            ICloudStreamWriter writer = convertVerb.OutputFormat.ToLowerInvariant() switch
            {
                "xyz" => new XyzWriter($"{baseOutput}.xyz"),
                _ => throw new ArgumentOutOfRangeException()
            };

            writer.WritePointSet(reader.ReadPointSet());
        }

        
        private static void Report(string status, double step, double total)
        {
            _consoleProgressBar.Report(step/total, status);
        }

        private static void DetectTree(DetectVerb detectVerb)
        {
            ProgressTracker.Actions[EProgressStage.Slicing] = new Action<string,double,double>[] {Report};
            ProgressTracker.Actions[EProgressStage.NoiseFiltering] = new Action<string,double,double>[] {Report};
            ProgressTracker.Actions[EProgressStage.TreeApproximation] = new Action<string,double,double>[] {Report};
            ProgressTracker.Actions[EProgressStage.FakeTreesFiltering] = new Action<string,double,double>[] {Report};
            ProgressTracker.Actions[EProgressStage.TreeBuilding] = new Action<string,double,double>[] {Report};
            using var reader = new XyzReader(detectVerb.InputFile, Encoding.ASCII);
            var cloud = new Cloud(reader);
            cloud.NormalizeHeight();

            var terrain = new Terrain(cloud);
            var treeDetector = new TreeDetector();

            IPointSetFilter[] pointSetFilters =
            {
                new LargeGroupsFilter(p => Math.Max(0.1f, 0.75f - 0.01f * p)),
                new AspectRatioFilter(0.85f, 1.2f),
                new SmallGroupsFilter(p => Math.Max(0.05f, 0.3f - 0.02f * p)),
                PrepareGeneticDistributionFilter(0.3f),
                PrepareEllipsisMatchFilter()
            };

            var detectionParameters = new DetectionParameters
            {
                PointSetFilters = pointSetFilters
            };
            var mergingParameters = new MergingParameters
            {
                MinimumRegressionGroupingDistance = 0.09,
                MaximumGroupingEmptyHeight = 20,
                MinimumGroupingDistance = 0.35,
            };
            var x = 0;
            
            Directory.CreateDirectory(detectVerb.OutputDirectory);

            var trees = treeDetector.DetectPotentialTrees(cloud, detectionParameters, mergingParameters).ToList();
            
            
            var approximation = new TreeApproximation(PrepareGeneticEllipseMatchAlgorithm(), 0.8, 0.09);
            foreach (var detectedTree in trees)
            {
                var tree = approximation.ApproximateTree(detectedTree, terrain);
                using var writer1 = new XyzWriter($"{detectVerb.OutputDirectory}/T{x++}.e.xyz");
                using var writer2 = new XyzWriter($"{detectVerb.OutputDirectory}/T{x++}.xyz");
                var ellipses = tree.GetAllNodesAsVector().Select(node => node.Ellipse);
                foreach (var ellipse in ellipses)
                {
                    ellipse.ExportToStream(writer1);
                }
                foreach (var node in tree.GetAllNodesAsVector())
                {
                    writer2.WritePointSet(node.PointSet);
                }
            }

           
            
            // var pointSetGroups = treeDetector.DetectTrunkPointSets(cloud, detectionParameters).ToList();
            // using var writer = new XyzWriter($"{detectVerb.OutputDirectory}/trunk.xyz");
            //
            // foreach (var pointSetGroup in pointSetGroups)
            // {
            //     var pointSetSize = pointSetGroup.PointSets.Select(pointSet => pointSet.Count).Sum();
            //     if (pointSetSize <= 0)
            //     {
            //         continue;
            //     }
            //
            //     foreach (var t in pointSetGroup.PointSets)
            //     {
            //         writer.WritePointSet(t);
            //     }
            // }
        }
    }
}