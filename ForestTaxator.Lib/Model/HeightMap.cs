using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ForestTaxator.Lib.Model
{
    public abstract class HeightMap
    {
        public double MeshSize { get; protected set; }

        protected Dictionary<Tuple<int, int>, double> HeightMapDictionary;

        protected HeightMap()
        {
            HeightMapDictionary = new Dictionary<Tuple<int, int>, double>();
        }

        public double GetHeight(Point point)
        {
            var key = GetKey(point);
            return HeightMapDictionary.ContainsKey(key) ? HeightMapDictionary[key] : 0;
        }

        protected Tuple<int, int> GetKey(Point point)
        {
            var x = (int)(point.X / MeshSize);
            var y = (int)(point.Y / MeshSize);
            return new Tuple<int, int>(x, y);
        }

        protected bool Import(string file, byte[] magicNumbers)
        {
            using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
            using var binaryReader = new BinaryReader(fileStream);

            var bytes = binaryReader.ReadBytes(magicNumbers.Length);
            if (bytes.SequenceEqual(magicNumbers) == false)
            {
                return false;
            }

            MeshSize = binaryReader.ReadDouble();

            var entriesCount = binaryReader.ReadInt32();
            for (var i = 0; i < entriesCount; i++)
            {
                var key = new Tuple<int, int>(binaryReader.ReadInt32(), binaryReader.ReadInt32());
                HeightMapDictionary[key] = binaryReader.ReadDouble();
            }

            return true;
        }

        protected virtual void Export(string file, byte[] magicNumbers)
        {
            using var fileStream = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Write);
            using var binaryWriter = new BinaryWriter(fileStream);

            binaryWriter.Write(magicNumbers);
            binaryWriter.Write(MeshSize);
            binaryWriter.Write(HeightMapDictionary.Keys.Count);
            foreach (var key in HeightMapDictionary.Keys)
            {
                binaryWriter.Write(key.Item1);
                binaryWriter.Write(key.Item2);
                binaryWriter.Write(HeightMapDictionary[key]);
            }

            binaryWriter.Flush();
        }
    }
}