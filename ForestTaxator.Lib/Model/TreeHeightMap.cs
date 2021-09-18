using System;
using System.Collections.Generic;

namespace ForestTaxator.Lib.Model
{
    public class TreeHeightMap : HeightMap
    {
        // FORESTTAXATORHEIGHT
        private static readonly byte[] _magicNumbers =
        {
            //   F    O     R     E     S     T     T     A     X     A     T     O     R
            0x46, 0x4F, 0x52, 0x45, 0x53, 0x54, 0x54, 0x41, 0x58, 0x41, 0x54, 0x4F, 0x52,
            //  H     E     I     G     H     T
            0x48, 0x45, 0x49, 0x47, 0x48, 0x54
        };
        private TreeHeightMap()
        {
        }
        
        public TreeHeightMap(PointSet cloud, double meshSize = 2.5, double maxHeight = 50)
        {
            MeshSize = meshSize;
            HeightMapDictionary = new Dictionary<Tuple<int, int>, double>();

            foreach (var cloudPoint in cloud)
            {
                var key = GetKey(cloudPoint);
                if (cloudPoint.Z > maxHeight)
                {
                    continue;
                }

                if (!HeightMapDictionary.ContainsKey(key) || HeightMapDictionary[key] < cloudPoint.Z)
                {
                    HeightMapDictionary[key] = cloudPoint.Z;
                }
            }
        }
        
        public void Export(string file)
        {
            Export(file, _magicNumbers);
        }

        public static TreeHeightMap Import(string file)
        {
            var terrain = new TreeHeightMap();
            return terrain.Import(file, _magicNumbers) ? terrain : null;
        }
    }
}