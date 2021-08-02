using System.Collections.Generic;
using System.Linq;

namespace ForestTaxator.Lib.Utils
{
    public class Distribution
    {
        private double[] _data;
        public long Size => _data.Length;
        
        public Distribution(int size)
        {
            _data = new double[size];
        }

        public Distribution(IList<int> data)
        {
            _data = data?.Select(x => (double) x).ToArray();
        }

        public Distribution(double[] data)
        {
            _data = data;
        }

        public double this[int index]
        {
            get => _data[index];
            set => _data[index] = value;
        }

        public static Distribution operator -(Distribution x1, Distribution x2)
        {
            if (x1.Size != x2.Size)
            {
                return null;
            }

            var data = new double[x1.Size];
            for (var i = 0; i < x1.Size; i++)
            {
                data[i] = x1[i] - x2[i];
            }

            return new Distribution(data);
        }
        private double[] GetNormalizedData()
        {
            var min = double.MaxValue;
            var max = -double.MaxValue;
            var data = new double[Size];

            foreach (var t in _data)
            {
                if (t < min)
                {
                    min = t;
                }

                if (t > max)
                {
                    max = t;
                }
            }

            var range = max - min;
            if (range == 0.0)
            {
                return data;
            }

            for (var i = 0; i < Size; i++)
            {
                data[i] = (_data[i] - min) / range;
            }

            return data;
        }

        public void Normalize()
        {
            _data = GetNormalizedData();
        }

        public Distribution Normalized() => new(GetNormalizedData());

        public double Average() => Statistics.Average(_data.Skip(2).SkipLast(2).ToList());

        public double StandardDeviation(double average) => Statistics.StandardDeviation(_data.Skip(2).SkipLast(2).ToList(), average);

        public double StandardDeviation() => Statistics.StandardDeviation(_data.Skip(2).SkipLast(2).ToList());
    }
}