using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ForestTaxator.Lib.Algorithms;
using ForestTaxator.Lib.Data.GPD;
using ForestTaxator.Lib.Filters;
using ForestTaxator.Lib.Fitness;
using ForestTaxator.Lib.Model;
using ForestTaxator.Lib.Utils;
using ForestTaxator.TestApp.Commands.Analyze;
using ForestTaxator.TestApp.Models;
using ForestTaxator.TestApp.Utils;
using GeneticToolkit;
using GeneticToolkit.Comparisons;
using GeneticToolkit.Factories;
using GeneticToolkit.Genotypes.Collective;
using GeneticToolkit.Interfaces;
using GeneticToolkit.Phenotypes.Collective;
using GeneticToolkit.Utils.Configuration;
using Newtonsoft.Json;
using Serilog;

namespace ForestTaxator.TestApp.Flows
{
    public class FilteringFlow
    {
        private static GeneticEllipseMatch PrepareGeneticEllipseMatchAlgorithm(EllipsisMatchFilterConfiguration configuration, ILogger logger)
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

        private static GeneticDistributionFilter PrepareGeneticDistributionFilter(GeneticDistributionFilterConfiguration configuration,
            ILogger logger)
        {
            var handler = FunctionBuilder<double>.CreateFunction(configuration.TrunkThreshold);
            var geneticDistributionFilterParams = new GeneticDistributionFilterParams
            {
                DistributionResolution = configuration.DistributionResolution,
                GetTrunkThreshold = height => handler(new object[] {height})
            };
            var geneticDistributionFilter = new GeneticDistributionFilter(geneticDistributionFilterParams);
            var compareCriteria = new SimpleComparison<GeneticDistributionFitness>(EOptimizationMode.Minimize);
            var factory = new CollectivePhenotypeFactory<ParabolicParameters>();
            var individualFactory =
                new IndividualFactory<CollectiveGenotype<ParabolicParameters>, CollectivePhenotype<ParabolicParameters>,
                        GeneticDistributionFitness>
                    (factory);

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
            geneticDistributionFilter.GeneticAlgorithm = geneticAlgorithm;

            return geneticDistributionFilter;
        }

        private static IEnumerable<IPointSetFilter> ParsePointSetFilters(FilterCommand command, ILogger logger)
        {
            var configurationFileContent = File.ReadAllText(command.FiltersConfigurationFile);
            var filtersConfiguration = JsonConvert.DeserializeObject<FiltersConfiguration>(configurationFileContent);
            if (filtersConfiguration == null)
            {
                logger.Fatal("Could not parse Filter Configuration file!");
                Environment.Exit(0);
            }

            var filters = new Dictionary<int, IPointSetFilter>();

            if (filtersConfiguration.AspectRatioFilter != null)
            {
                filters.Add(
                    filtersConfiguration.AspectRatioFilter.Order,
                    new AspectRatioFilter(
                        filtersConfiguration.AspectRatioFilter.MinimumAspectRatio,
                        filtersConfiguration.AspectRatioFilter.MaximumAspectRatio
                    ));
            }

            if (filtersConfiguration.LargeGroupsFilter != null)
            {
                var functor = FunctionBuilder<double>.CreateFunction(filtersConfiguration.LargeGroupsFilter.LargeGroupsMaxSize);
                filters.Add(
                    filtersConfiguration.LargeGroupsFilter.Order,
                    new LargeGroupsFilter(
                        height => functor(new object[] {height})
                    )
                );
            }

            if (filtersConfiguration.SmallGroupsFilter != null)
            {
                var functor = FunctionBuilder<double>.CreateFunction(filtersConfiguration.SmallGroupsFilter.SmallGroupsMinSize);
                filters.Add(
                    filtersConfiguration.SmallGroupsFilter.Order,
                    new SmallGroupsFilter(
                        height => functor(new object[] {height})
                    )
                );
            }

            if (filtersConfiguration.EllipsisMatchFilterConfiguration != null)
            {
                var algorithm = PrepareGeneticEllipseMatchAlgorithm(filtersConfiguration.EllipsisMatchFilterConfiguration, logger);
                var filter = new EllipsisMatchFilter()
                {
                    EccentricityThreshold = filtersConfiguration.EllipsisMatchFilterConfiguration.MatchEccentricityThreshold,
                    FitnessThreshold = filtersConfiguration.EllipsisMatchFilterConfiguration.FitnessThreshold,
                    GeneticEllipseMatch = algorithm
                };
                filters.Add(filtersConfiguration.EllipsisMatchFilterConfiguration.Order, filter);
            }

            if (filtersConfiguration.GeneticDistributionFilterConfiguration != null)
            {
                var filter = PrepareGeneticDistributionFilter(filtersConfiguration.GeneticDistributionFilterConfiguration, logger);
                filters.Add(filtersConfiguration.GeneticDistributionFilterConfiguration.Order, filter);
            }

            return filters.OrderBy(filter => filter.Key).Select(filter => filter.Value);
        }

        public static Task Execute(FilterCommand command, ILogger logger)
        {
            if (File.Exists(command.Input) == false)
            {
                logger.Fatal("Input file does not exist!");
                Environment.Exit(0);
            }

            if (File.Exists(command.FiltersConfigurationFile) == false)
            {
                logger.Fatal("Configuration file does not exist!");
                Environment.Exit(0);
            }

            var filters = ParsePointSetFilters(command, logger).ToArray();
            var detectionParameters = new DetectionParameters
            {
                PointSetFilters = filters,
            };

            using var reader = new GpdReader(command.Input);
            var slices = reader.ReadPointSlices();

            logger?.Information("Starting point grouping...");
            var groups = slices.Select(slice =>
                slice?.GroupByDistance(detectionParameters.MeshWidth, detectionParameters.MinimalPointsPerMesh)
            )
                .Where(slice => slice != null)
                .ToList();
            logger?.Information("Points grouped...");
            logger?.Information("Starting filtering...");
            var pointSetGroups = groups.Select((group, index) =>
            {
                ProgressTracker.Progress(EProgressStage.NoiseFiltering, "Filtering Noise", index, groups.Count);
                return @group.Filter(detectionParameters.PointSetFilters);
            }).Where(x => x != null).ToList();

            if (command.Merge)
            {
                using var writer = new GpdWriter(Path.Join(command.Output,"Filtered.gpd"), null, reader.Header.Slice);
                for(var i = 0; i < pointSetGroups.Count; i++)
                {
                    writer.WritePointSetGroup(pointSetGroups[i], i);
                }
            }
            else
            {
                for(var i = 0; i < pointSetGroups.Count; i++)
                {
                    using var writer = new GpdWriter(Path.Join(command.Output,$"Filtered.{i}.gpd"), null, reader.Header.Slice);
                    writer.WritePointSetGroup(pointSetGroups[i], i);
                }
            }

            return Task.CompletedTask;
        }
    }
}