using System;
using System.Collections.Generic;

namespace ForestTaxator.Lib.Model
{
    public class Terrain : HeightMap
    {
        // FORESTTAXATORTERRAIN
        private static readonly byte[] _magicNumbers =
        {
            0x46, 0x4F, 0x52, 0x45, 0x53, 0x54, 0x54, 0x41, 0x58, 0x41, 0x54, 0x4F,
            0x52, 0x54, 0x45, 0x52, 0x52, 0x41, 0x49, 0x4E
        };

        private Terrain()
        {
        }

        public Terrain(PointSet cloud, double meshSize = 2.5, double maxTerrainHeight = 1)
        {
            MeshSize = meshSize;
            HeightMapDictionary = new Dictionary<Tuple<int, int>, double>();

            foreach (var cloudPoint in cloud)
            {
                var key = GetKey(cloudPoint);
                if (cloudPoint.Z > maxTerrainHeight)
                {
                    continue;
                }

                if (!HeightMapDictionary.ContainsKey(key) || HeightMapDictionary[key] > cloudPoint.Z)
                {
                    HeightMapDictionary[key] = cloudPoint.Z;
                }
            }
        }

        public void Export(string file)
        {
           Export(file, _magicNumbers);
        }

        public static Terrain Import(string file)
        {
            var terrain = new Terrain();
            return terrain.Import(file, _magicNumbers) ? terrain : null;
        }
    }
}