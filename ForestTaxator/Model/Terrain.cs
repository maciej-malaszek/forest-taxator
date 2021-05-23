using System;
using System.Collections.Generic;

namespace ForestTaxator.Model
{
    public class Terrain
    {
        public double MeshSize { get; }

        private readonly Dictionary<Tuple<int, int>, double> _heightMap;

        public Terrain(PointSet cloud, double meshSize = 2.5)
        {
            MeshSize = meshSize;
            _heightMap = new Dictionary<Tuple<int, int>, double>();
            
            foreach (var cloudPoint in cloud)
            {
                var key = GetKey(cloudPoint);
                
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
    }
}
