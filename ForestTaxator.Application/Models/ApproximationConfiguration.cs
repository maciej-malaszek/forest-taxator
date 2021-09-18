namespace ForestTaxator.Application.Models
{
    public class EllipsisMatchConfiguration
    {
        public double BufferWidth { get; set; }
        public double InvalidEccentricityThreshold { get; set; }
        public string GeneticAlgorithmConfigurationFile { get; set; }
    }
    
    public class ApproximationConfiguration
    {
        public EllipsisMatchConfiguration EllipsisMatchConfiguration { get; set; }
        public double EccentricityThreshold { get; set; }
        public double FitnessThreshold { get; set; }
    }
}