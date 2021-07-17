using System.Linq.Expressions;
using ForestTaxator.TestApp.Utils;

namespace ForestTaxator.TestApp.Models
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
    
    public class FiltersConfiguration
    {
        public LargeGroupsFilterConfiguration LargeGroupsFilter { get; set; }
        public SmallGroupsFilterConfiguration SmallGroupsFilter { get; set; }
        public AspectRatioFilterConfiguration AspectRatioFilter { get; set; }
        public EllipsisMatchFilterConfiguration EllipsisMatchFilterConfiguration { get; set; }
    }
}