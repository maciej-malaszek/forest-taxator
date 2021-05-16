using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ForestTaxator.Model;

namespace ForestTaxator.Filters
{
    public class DensityFilter : IPointSetFilter
    {
        // Maximal density per square meter
        public float Density { get; set; }

        public float MeshWidth { get; set; }
        public DensityFilter(float density = 20000, float meshWidth = 0.01f)
        {
            MeshWidth = meshWidth;
            Density = density;
        }

        public IList<PointSet> Filter(IList<PointSet> pointSets)
        {
            return pointSets?.Select(ReduceGroup).ToList();
        }
        
        private PointSet ReduceGroup(PointSet pointSet)
        {
            var maxPointsCount = (int)(pointSet.BoundingBox.Depth * pointSet.BoundingBox.Width * Density);
            if (pointSet.Count <= maxPointsCount)
            {
                return pointSet;
            }
            var maxPointPerMesh = (int)Math.Ceiling(Density * MeshWidth * MeshWidth);
            var rasterGrid = new RasterGrid(pointSet, MeshWidth);
            for (var x = 0; x < rasterGrid.MeshCount; x++)
            {
                for (var y = 0; y < rasterGrid.MeshCount; y++)
                {
                    var cloudPoints = rasterGrid[x, y]?.Take(maxPointPerMesh).ToList();
                    if (cloudPoints != null)
                    {
                        rasterGrid[x, y] = new PointSet(cloudPoints);
                    }
                }
            }

            return rasterGrid.Merge();
        }
    }
}