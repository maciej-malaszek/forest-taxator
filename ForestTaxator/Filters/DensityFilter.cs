using System;
using System.Linq;
using ForestTaxator.Model;

namespace ForestTaxator.Filters
{
    public class DensityFilter
    {
        // Maximal density per square meter
        public float Density { get; set; }
        public DensityFilter(float density = 20000)
        {
            Density = density;
        }
        public void Filter(PointSet[] groups)
        {
            if(groups == null)
            {
                return;
            }

            for (var i = 0; i < groups.Length; i++)
            {
                if (groups[i]?.BoundingBox == null)
                {
                    continue;
                }

                var maxSize = (int)(groups[i].BoundingBox.Depth * groups[i].BoundingBox.Width * Density);
                if (groups[i].Count > maxSize)
                {
                    groups[i] = ReduceGroup(groups[i]);
                }
            }
        }

        private PointSet ReduceGroup(PointSet pointSet)
        {
            const float MeshWidth = 0.01f;
            var maxPointPerMesh = (int)Math.Ceiling(Density * MeshWidth * MeshWidth);
            var rasterGrid = new RasterGrid(pointSet, MeshWidth);
            for (var x = 0; x < rasterGrid.MeshCount; x++)
            for (var y = 0; y < rasterGrid.MeshCount; y++)
            {
                var cloudPoints = rasterGrid[x, y]?.Take(maxPointPerMesh);
                if(cloudPoints != null)
                {
                    rasterGrid[x, y] = new PointSet(cloudPoints);
                }
            }
            return rasterGrid.Merge();

        }
    }
}