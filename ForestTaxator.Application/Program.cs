using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommandLine;
using ConsoleProgressBar;
using ForestTaxator.Application.Commands;
using ForestTaxator.Application.Commands.Analyze;
using ForestTaxator.Application.Commands.Extensions;
using ForestTaxator.Application.Flows;
using ForestTaxator.Lib.Model;
using ForestTaxator.Lib.Utils;
using ProgressHierarchy;
using Serilog;

namespace ForestTaxator.Application
{
    class Program
    {
        private static HierarchicalProgress _consoleProgressBar;
        private static ILogger _logger;

        private static void Report(string status, double step, double total)
        {
            _consoleProgressBar.Report(step / total, status);
        }

        private static void PrepareProgressTracker()
        {
            ProgressTracker.Actions[EProgressStage.Slicing] = new Action<string, double, double>[] { Report };
            ProgressTracker.Actions[EProgressStage.NoiseFiltering] = new Action<string, double, double>[] { Report };
            ProgressTracker.Actions[EProgressStage.TreeApproximation] = new Action<string, double, double>[] { Report };
            ProgressTracker.Actions[EProgressStage.FakeTreesFiltering] = new Action<string, double, double>[] { Report };
            ProgressTracker.Actions[EProgressStage.TreeBuilding] = new Action<string, double, double>[] { Report };
        }

        static void Main(string[] args)
        {
            _logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();
            using var progressBar = new ProgressBar
            {
                BarCharacter = '\u2588',
                Width = 0.8,
                StatusOnSeparateLine = false
            };
            _consoleProgressBar = progressBar.HierarchicalProgress.Fork(1);
            PrepareProgressTracker();

            var parsed = Parser.Default.ParseSetArguments<AnalyzeVerbSet, ConvertVerb>(args, OnVerbSetParsed);
            parsed
                .MapResult<ApproximateCommand, DetectTreesCommand, FilterCommand, SliceCommand, TerrainCommand, TreeHeightCommand, ConvertVerb,
                    Task>
                (
                    approximateCommand => Parser.Default.ExecuteMapping(approximateCommand, cmd => ApproximationFlow.Execute(cmd, _logger)),
                    detectTreesCommand => Parser.Default.ExecuteMapping(detectTreesCommand, cmd => DetectionFlow.Execute(cmd, _logger)),
                    filterCommand => Parser.Default.ExecuteMapping(filterCommand, cmd => FilteringFlow.Execute(cmd, _logger)),
                    sliceCommand => Parser.Default.ExecuteMapping(sliceCommand, cmd => SlicingFlow.Execute(cmd, _logger)),
                    terrainCommand => Parser.Default.ExecuteMapping(terrainCommand, cmd => TerrainFlow.Execute(cmd, _logger)),
                    treeHeightCommand => Parser.Default.ExecuteMapping(treeHeightCommand, cmd => TreeHeightFlow.Execute(cmd, _logger)),
                    convertCommand => Parser.Default.ExecuteMapping(convertCommand, cmd => ConversionFlow.Execute(cmd, _logger)),
                    _ => Task.CompletedTask
                );
            _consoleProgressBar.Dispose();
        }

        private static ParserResult<object> OnVerbSetParsed(Parser parser,
            Parsed<object> parsed, IEnumerable<string> argsToParse, bool containedHelpOrVersion)
        {
            return parsed.MapResult(
                (AnalyzeVerbSet _) =>
                    parser.ParseArguments<ApproximateCommand, DetectTreesCommand, FilterCommand, SliceCommand, TerrainCommand, TreeHeightCommand>(argsToParse),
                _ => parsed);
        }
    }
}