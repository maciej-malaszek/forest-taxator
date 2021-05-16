using System;
// ReSharper disable PossibleLossOfFraction

namespace ForestTaxator.Model
{
    public class RasterGrid
    {
        private int Width { get; set; }
        private Point Center { get; set; }
        private float MeshWidth { get; set; }
        private PointSet[,] PointSets { get; set; }
        public int MeshCount { get; private set; }


        public RasterGrid(PointSet pointSet, float meshWidth)
        {
            Center = pointSet.Center;
            Width = (int) Math.Ceiling(Math.Max(pointSet.BoundingBox.Width, pointSet.BoundingBox.Depth));
            MeshWidth = meshWidth;
            MeshCount = (int) Math.Ceiling(Width / MeshWidth + 2);
            PointSets = new PointSet[MeshCount, MeshCount];
            for (var i = pointSet.Count - 1; i >= 0; i--)
            {
                AddPoint(pointSet[i]);
            }

            pointSet.Clear();
        }

        public bool AddPoint(CloudPoint point)
        {
            if (point == null)
            {
                return false;
            }

            var x = (int) ((point.X - Center.X) / MeshWidth + MeshCount / 2);
            var y = (int) ((point.Y - Center.Y) / MeshWidth + MeshCount / 2);
            if (x >= MeshCount || y >= MeshCount || x < 0 || y < 0)
            {
                return false;
            }

            if (PointSets[x, y] == null)
            {
                PointSets[x, y] = new PointSet();
            }

            PointSets[x, y].Add(point);

            return true;
        }

        public PointSet this[int x, int y]
        {
            get => PointSets[x, y];
            set => PointSets[x, y] = value;
        }

        public PointSet this[float x, float y]
        {
            get
            {
                var x1 = (int) ((x - Center.X) / MeshWidth + MeshCount / 2);
                var y1 = (int) ((y - Center.Y) / MeshWidth + MeshCount / 2);
                if (x1 >= MeshCount || y1 >= MeshCount || x1 < 0 || y1 < 0)
                {
                    return null;
                }

                return PointSets[x1, y1];
            }
        }

        public void FilterLowDensity(int size)
        {
            for (var x = 0; x < MeshCount; x++)
            {
                for (var y = 0; y < MeshCount; y++)
                {
                    if (PointSets[x, y] == null || PointSets[x, y].Count >= size)
                    {
                        continue;
                    }

                    PointSets[x, y].Clear();
                    PointSets[x, y] = null;
                }
            }
        }

        public PointSet Merge()
        {
            var pointSet = new PointSet();
            foreach (var set in PointSets)
            {
                set?.MoveTo(pointSet);
            }

            return pointSet;
        }

        public void MergeWithNeighbours(ref PointSet pointSet, int x, int y)
        {
            if (PointSets[x, y] == null)
            {
                return;
            }

            PointSets[x, y].MoveTo(pointSet);
            PointSets[x, y] = null;

            if (x > 0 && y > 0 
                      && PointSets[x - 1, y - 1] != null 
                      && PointSets[x - 1, y - 1].Count > 0)
            {
                MergeWithNeighbours(ref pointSet, x - 1, y - 1);
            }

            if (x > 0 && 
                PointSets[x - 1, y] != null && 
                PointSets[x - 1, y].Count > 0)
            {
                MergeWithNeighbours(ref pointSet, x - 1, y);
            }

            if (x > 0 && y < MeshCount - 1 && 
                PointSets[x - 1, y + 1] != null && 
                PointSets[x - 1, y + 1].Count > 0)
            {
                MergeWithNeighbours(ref pointSet,x - 1, y + 1);
            }

            if (y > 0 && 
                PointSets[x, y - 1] != null && 
                PointSets[x, y - 1].Count > 0)
            {
                MergeWithNeighbours(ref pointSet, x, y - 1);
            }

            if (y < MeshCount - 1 && 
                PointSets[x, y + 1] != null && 
                PointSets[x, y + 1].Count > 0)
            {
                MergeWithNeighbours(ref pointSet, x, y + 1);
            }

            if (x < MeshCount - 1 && y > 0 && 
                PointSets[x + 1, y - 1] != null && 
                PointSets[x + 1, y - 1].Count > 0)
            {
                MergeWithNeighbours(ref pointSet, x + 1, y - 1);
            }

            if (x < MeshCount - 1 && 
                PointSets[x + 1, y] != null && 
                PointSets[x + 1, y].Count > 0)
            {
                MergeWithNeighbours(ref pointSet, x + 1, y);
            }

            if (x < MeshCount - 1 && y < MeshCount - 1 &&
                PointSets[x + 1, y + 1] != null &&
                PointSets[x + 1, y + 1].Count > 0)
            {
                MergeWithNeighbours(ref pointSet, x + 1, y + 1);
            }
        }
    }
}