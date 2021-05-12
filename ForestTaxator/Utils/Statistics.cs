using System;
using System.Collections.Generic;
using System.Linq;

namespace ForestTaxator.Utils
{
    public static class Statistics
    {
        public static double Average(IEnumerable<double> data) => data != null && data.Any() ?  data.Average() : -1;
        public static double Average(IEnumerable<int> data) => data != null && data.Any() ? data.Average() : -1;

        public static double StandardDeviation(IEnumerable<double> data)
        {
            var enumerable = data as double[] ?? data.ToArray();
            var avg = Average(enumerable);
            var sum = enumerable.Sum(x => Math.Pow(x - avg, 2));

            return Math.Sqrt(sum / enumerable.Length);
        }
        
        public static double StandardDeviation(IEnumerable<double> data, double average)
        {
            var enumerable = data as double[] ?? data.ToArray();
            var sum = enumerable.Sum(x => Math.Pow(x - average, 2));
            return Math.Sqrt(sum / enumerable.Length);
        }

    }
}