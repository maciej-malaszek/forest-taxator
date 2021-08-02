using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ForestTaxator.Lib.Model
{ 
    public class Terrain
    {
        public double MeshSize { get; private set; }

        private readonly Dictionary<Tuple<int, int>, double> _heightMap;


        // FORESTTAXATORTERRAIN
        private static readonly byte[] _magicNumbers =
        {
            0x46, 0x4F, 0x52, 0x45, 0x53, 0x54, 0x54, 0x41, 0x58, 0x41, 0x54, 0x4F,
            0x52, 0x54, 0x45, 0x52, 0x52, 0x41, 0x49, 0x4E
        };

        private Terrain()
        {
            _heightMap = new Dictionary<Tuple<int, int>, double>();
        }


        public Terrain(PointSet cloud, double meshSize = 2.5, double maxTerrainHeight = 1)
        {
            MeshSize = meshSize;
            _heightMap = new Dictionary<Tuple<int, int>, double>();
            
            foreach (var cloudPoint in cloud)
            {
                var key = GetKey(cloudPoint);
                if (cloudPoint.Z > maxTerrainHeight)
                {
                    continue;
                }
                if (!_heightMap.ContainsKey(key) || _heightMap[key] > cloudPoint.Z)
                {
                    _heightMap[key] = cloudPoint.Z;
                }
            }
        }

        public double GetHeight(Point point)
        {
            var key = GetKey(point);
            return _heightMap[key];
        }

        private Tuple<int, int> GetKey(Point point)
        {
            var x = (int)(point.X / MeshSize);
            var y = (int)(point.Y / MeshSize);
            return new Tuple<int, int>(x, y);
        }

        public void Export(string file)
        {
            using var fileStream = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Write);
            using var binaryWriter = new BinaryWriter(fileStream);
            
            binaryWriter.Write(_magicNumbers);
            binaryWriter.Write(MeshSize);
            binaryWriter.Write(_heightMap.Keys.Count);
            foreach (var key in _heightMap.Keys)
            {
                binaryWriter.Write(key.Item1);
                binaryWriter.Write(key.Item2);
                binaryWriter.Write(_heightMap[key]);
            }
            binaryWriter.Flush();
        }

        public static Terrain Import(string file)
        {
            using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
            using var binaryReader = new BinaryReader(fileStream);

            var bytes = binaryReader.ReadBytes(_magicNumbers.Length);
            if (bytes.SequenceEqual(_magicNumbers) == false)
            {
                return null;
            }

            var terrain = new Terrain();
            terrain.MeshSize = binaryReader.ReadDouble();
            var entriesCount = binaryReader.ReadInt32();
            for (var i = 0; i < entriesCount; i++)
            {
                var key = new Tuple<int, int>(binaryReader.ReadInt32(), binaryReader.ReadInt32());
                terrain._heightMap[key] = binaryReader.ReadDouble();
            }

            return terrain;
        }
    }
}
