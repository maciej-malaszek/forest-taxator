using System.Collections.Generic;
using ForestTaxator.Data;

namespace ForestTaxator.Model
{
    public class Cloud : PointSet
    {
        public Cloud(ICloudStreamReader streamReader)
        {
            Points = (streamReader.ReadPointSet()).Points;
            RecalculateBoundingBox();
        }

        public Cloud(PointSet pointSet)
        {
            Points = pointSet.Points;
            RecalculateBoundingBox();
        }

        public IList<PointSlice> Slice(double height = 0.1)
        {
            return SplitByHeight(BoundingBox, height);
        }
        
        public Terrain DetectTerrain()
        {
            var terrain = new Terrain(this);
            return terrain;
        }
        
        public void NormalizeHeight(Terrain terrain = null)
        {
            terrain ??= DetectTerrain();
            
            foreach (var point in Points)
            {
                point.Z -= terrain.GetHeight(point);
            }
            RecalculateBoundingBox();
        }
    }
}