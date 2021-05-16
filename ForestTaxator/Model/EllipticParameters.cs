using GeneticToolkit.Interfaces;
using GeneticToolkit.Utils;

using System;
using System.Collections.Generic;

namespace ForestTaxator.Model
{
    public class EllipticParameters : IGeneticallySerializable
    {
        private static readonly Range<float>[] _range = {
                new(0.01f, 0.4f),
                new(-0.1f, 0.1f),
          };

        private static float Scale0 => _range[0].High - _range[0].Low;
        private static float Scale1 => _range[1].High - _range[1].Low;
        private static float Step0 => Scale0 / byte.MaxValue;
        private static float Step1 => Scale1 / byte.MaxValue;

        private readonly float[] _parameters = new float[5];

        public float X1 { get => _parameters[0]; set => _parameters[0] = value; }
        public float Y1 { get => _parameters[1]; set => _parameters[1] = value; }
        public float X2 { get => _parameters[2]; set => _parameters[2] = value; }
        public float Y2 { get => _parameters[3]; set => _parameters[3] = value; }
        public float A { get => _parameters[4]; set => _parameters[4] = value; }

        public byte[] Serialize()
        {
            var bytes = new List<byte>();
            for (var i = 0; i < _parameters.Length; i++)
            {
                if (i == 4)
                {
                    var rawValue = (byte)((_parameters[i] - _range[0].Low) / Step0);
                    bytes.Add(rawValue);
                }
                else
                {
                    var rawValue = (byte)((_parameters[i] - _range[1].Low) / Step1);
                    bytes.Add(rawValue);
                }
            }

            return bytes.ToArray();
        }

        public IGeneticallySerializable Deserialize(byte[] data)
        {
            var offset = 0;
            for (var i = 0; i < _parameters.Length; i++)
            {
                if (i == 4)
                {
                    var rawValue = data[offset];
                    offset += sizeof(byte);
                    _parameters[i] = rawValue * Step0 + _range[0].Low;
                }
                else
                {

                    var rawValue = data[offset];
                    offset += sizeof(byte);
                    _parameters[i] = rawValue * Step1 + _range[1].Low;
                }
            }

            return this;
        }
    }
}
