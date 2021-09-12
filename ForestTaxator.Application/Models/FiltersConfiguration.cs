using ForestTaxator.Application.Utils;

namespace ForestTaxator.Application.Models
{
    public class LargeGroupsFilterConfiguration
    {
        public ExpressionDefinition LargeGroupsMaxSize { get; set; }
        public int Order { get; set; }
    }

    public class SmallGroupsFilterConfiguration
    {
        public ExpressionDefinition SmallGroupsMinSize { get; set; }
        public int Order { get; set; }
    }

    public class AspectRatioFilterConfiguration
    {
        public float MinimumAspectRatio { get; set; }
        public float MaximumAspectRatio { get; set; }
        public int Order { get; set; }
    }

    public class EllipsisMatchFilterConfiguration
    {
        public double FitnessThreshold { get; set; }
        public double MatchEccentricityThreshold { get; set; }
        public double BufferWidth { get; set; }
        public double InvalidEccentricityThreshold { get; set; }
        public string GeneticAlgorithmConfigurationFile { get; set; }
        public int Order { get; set; }
    }

    public class GeneticDistributionFilterConfiguration
    {
        public string GeneticAlgorithmConfigurationFile { get; set; }
        public int DistributionResolution { get; set; }
        public ExpressionDefinition TrunkThreshold { get; set; }
        public int Order { get; set; }
    }

    public class FiltersConfiguration
    {
        public LargeGroupsFilterConfiguration LargeGroupsFilter { get; set; }
        public SmallGroupsFilterConfiguration SmallGroupsFilter { get; set; }
        public AspectRatioFilterConfiguration AspectRatioFilter { get; set; }
        public EllipsisMatchFilterConfiguration EllipsisMatchFilterConfiguration { get; set; }
        public GeneticDistributionFilterConfiguration GeneticDistributionFilterConfiguration { get; set; }
    }
}