using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ForestTaxator.Algorithms;
using ForestTaxator.Data;
using ForestTaxator.Filters;
using ForestTaxator.Model;
using ForestTaxator.TestApp.Commands.Analyze;
using ForestTaxator.TestApp.Models;
using ForestTaxator.TestApp.Utils;
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
using GeneticToolkit.Utils.Configuration;
using GeneticToolkit.Utils.Factories;
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
            var fitnessFunction = geneticEllipseMatch.GetFitnessFunction();
            var factory = new CollectivePhenotypeFactory<EllipticParameters>();
            var individualFactory =
                new IndividualFactory<CollectiveGenotype<EllipticParameters>, CollectivePhenotype<EllipticParameters>>(factory,
                    fitnessFunction);
            var compareCriteria = new SimpleComparison(fitnessFunction, EOptimizationMode.Minimize);

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

            return Task.CompletedTask;
        }
    }
}