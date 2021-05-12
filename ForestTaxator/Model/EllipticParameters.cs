using GeneticToolkit.Interfaces;
using GeneticToolkit.Utils;

using System;
using System.Collections.Generic;

namespace ForestTaxator.Model
{
    public class EllipticParameters : IGeneticallySerializable
    {
        private static readonly Range<float>[] Range = {
                new Range<float>(0.01f, 0.4f),
                new Range<float>(-0.1f, 0.1f),
          };

        private static float Scale0 => Range[0].High - Range[0].Low;
        private static float Scale1 => Range[1].High - Range[1].Low;
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
                    var rawValue = (byte)((_parameters[i] - Range[0].Low) / Step0);
                    bytes.Add(rawValue);
                }
                else
                {
                    var rawValue = (byte)((_parameters[i] - Range[1].Low) / Step1);
                    bytes.Add(rawValue);

                    //ushort rawValue = (ushort)((_parameters[i] - Range[1].Low) / Step1);
                    //bytes.AddRange(BitConverter.GetBytes(rawValue));
                    //bytes.AddRange(BitConverter.GetBytes(_parameters[i]));
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
                    _parameters[i] = rawValue * Step0 + Range[0].Low;
                }
                else
                {

                    var rawValue = data[offset];
                    offset += sizeof(byte);
                    _parameters[i] = rawValue * Step1 + Range[1].Low;

                    //ushort rawValue = BitConverter.ToUInt16(data, offset);
                    //offset += sizeof(ushort);
                    //_parameters[i] = rawValue * Step1 + Range[1].Low;

                    //_parameters[i] = BitConverter.ToSingle(data, sizeof(float) * i);
                }
            }

            return this;
        }
    }
}
