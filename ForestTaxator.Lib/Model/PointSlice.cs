using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ForestTaxator.Model
{
    public class PointSlice : IPointSetGroup
    {
        public IList<PointSet> PointSets { get; set; }
        
        public double Height { get; init; }

        private RasterGrid Rasterize(float meshWidth = 0.1f, int minimumCloudPoints = 10)
        {
            var rasterGrid = new RasterGrid(PointSets[0], meshWidth);
            rasterGrid.FilterLowDensity(minimumCloudPoints);
            return rasterGrid;
        }

        public PointSetGroup GroupByDistance(float meshWidth = 0.1f, int minimalPointsPerMesh = 10, int stackSize = 67108864)
        {
            var group = new PointSetGroup();
            var start = new ThreadStart(() => group.PointSets = ExtractGroups(Rasterize(meshWidth, minimalPointsPerMesh)));
            var t = new Thread(start, stackSize);
            t.Start();
            t.Join();
            return group;
        }

        private static IList<PointSet> ExtractGroups(RasterGrid rasterGrid)
        {
            var groups = new LinkedList<PointSet>();

            for (var x = 0; x < rasterGrid.MeshCount; x++)
            for (var y = 0; y < rasterGrid.MeshCount; y++)
            {
                if (rasterGrid[x, y] == null)
                {
                    continue;
                }

                var group = new PointSet();
                rasterGrid.MergeWithNeighbours(ref group, x, y);
                groups.AddLast(group);
            }

            return groups.ToArray();
        }
    }
}