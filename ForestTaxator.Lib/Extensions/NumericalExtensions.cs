namespace ForestTaxator.Lib.Extensions
{
    public static class NumericalExtensions
    {
        public static float RoundDownToFirstDigit(this double p) => (int)(10 * p) / 10.0f;
        public static float RoundDownToFirstDigit(this float p) => (int)(10 * p) / 10.0f;
        
    }
}