using GeneticToolkit.Interfaces;
using GeneticToolkit.Utils;
using System;
using System.Collections.Generic;

namespace ForestTaxator.Model
{
    public class ParabolicParameters : IGeneticallySerializable
    {
        private static readonly Range<float>[] _range =
        {
            new(0.01f, 1.5f),
            new(0.0f, 0.5f)
        };

        private static float Scale0 => _range[0].High - _range[0].Low;
        private static float Scale1 => _range[1].High - _range[1].Low;
        private static float Step0 => Scale0 / ushort.MaxValue;
        private static float Step1 => Scale1 / ushort.MaxValue;

        private readonly double[] _parameters = new double[6];

        public double A1
        {
            get => _parameters[0];
            set => _parameters[0] = value;
        }

        public double B1
        {
            get => _parameters[1];
            set => _parameters[1] = value;
        }

        public double C1
        {
            get => _parameters[2];
            set => _parameters[2] = value;
        }

        public double A2
        {
            get => _parameters[3];
            set => _parameters[3] = value;
        }

        public double B2
        {
            get => _parameters[4];
            set => _parameters[4] = value;
        }

        public double C2
        {
            get => _parameters[5];
            set => _parameters[5] = value;
        }

        public byte[] Serialize()
        {
            var bytes = new List<byte>();
            for (var i = 0; i < _parameters.Length; i++)
            {
                var parameter = _parameters[i];
                ushort rawValue;

                if (i is 0 or 3)
                {
                    rawValue = (ushort) ((parameter - _range[0].Low) / Step0);
                }
                else
                {
                    rawValue = (ushort) ((parameter - _range[1].Low) / Step1);
                }

                bytes.AddRange(BitConverter.GetBytes(rawValue));
            }

            return bytes.ToArray();
        }

        public IGeneticallySerializable Deserialize(byte[] data)
        {
            for (var i = 0; i < _parameters.Length; i++)
            {
                var rawValue = BitConverter.ToUInt16(data, sizeof(ushort) * i);
                if (i is 0 or 3)
                {
                    _parameters[i] = rawValue * Step0 + _range[0].Low;
                }
                else
                {
                    _parameters[i] = rawValue * Step1 + _range[1].Low;
                }
            }

            return this;
        }
    }
}