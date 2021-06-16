using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ForestTaxator.Algorithms;
using ForestTaxator.Utils;

namespace ForestTaxator.Model
{
    public class PointSet : IEnumerable<CloudPoint>
    {
        protected Point CenterPoint;
        protected bool BoundingBoxDeprecated;
        private Box _boundingBox;

        public Point Center
        {
            get
            {
                return CenterPoint ??= new Point()
                {
                    X = Points.Sum(x => x.X) / Points.Count,
                    Y = Points.Sum(x => x.Y) / Points.Count,
                    Z = Points.Sum(x => x.Z) / Points.Count,
                };
            }
            private set => CenterPoint = value;
        }

        public Box BoundingBox
        {
            get
            {
                if (BoundingBoxDeprecated)
                {
                    return BoundingBox = Box.Find(this);
                }

                return _boundingBox;
            }
            private set
            {
                _boundingBox = value;
                BoundingBoxDeprecated = false;
            }
        }

        public Point P1 { get; private set; }
        public Point P2 { get; private set; }


        protected internal List<CloudPoint> Points { get; set; } = new();

        public bool Empty => Points.Count == 0;

        public long Count => Points.Count;

        public PointSet()
        {
            BoundingBoxDeprecated = true;
            CenterPoint = new Point();
        }
        
        public PointSet(int size)
        {
            BoundingBoxDeprecated = true;
            CenterPoint = new Point();
            Points = new List<CloudPoint>(size);
        }

        public PointSet(IList<CloudPoint> points)
        {
            Points.AddRange(points);
            BoundingBoxDeprecated = true;
            CenterPoint = new Point();
        }

        public void RecalculateBoundingBox()
        {
            _boundingBox = Box.Find(this);
            BoundingBoxDeprecated = false;
            P1 = BoundingBox.P1;
            P2 = BoundingBox.P2;
        }

        public CloudPoint this[long indexer]
        {
            get => indexer < Count ? Points[(int) indexer] : null;
            set
            {
                if (indexer < Points.Count)
                {
                    Points[(int) indexer] = value;
                }
            }
        }

        public void SetBoundingBox(Point center)
        {
            if (center == null)
            {
                return;
            }

            BoundingBox.P1 = center.Clone();
            BoundingBox.P2 = center.Clone();

            BoundingBoxDeprecated = false;
        }

        public void Add(CloudPoint point)
        {
            Points.Add(point);
            BoundingBox.Broaden(point);
            P1 ??= point.Clone();

            P2 ??= point.Clone();

            P1.X = Math.Min(P1.X, point.X);
            P1.Y = Math.Min(P1.Y, point.Y);
            P1.Z = Math.Min(P1.Z, point.Z);

            P2.X = Math.Max(P2.X, point.X);
            P2.Y = Math.Max(P2.Y, point.Y);
            P2.Z = Math.Max(P2.Z, point.Z);
            CenterPoint = null;
        }
        
        public void Clear() => Points.Clear();

        public void Remove(CloudPoint point)
        {
            CenterPoint = null;
            if (point.OwningSet != this)
            {
                return;
            }

            Points.Remove(point);
            BoundingBoxDeprecated = true;
        }

        public void MoveTo(PointSet otherSet)
        {
            for (var i = Count - 1; i >= 0; i--)
            {
                var p = this[i];
                p.DetachFromSet();
                otherSet.Add(p);
            }

            BoundingBoxDeprecated = true;
            CenterPoint = null;
        }

        public PointSet Clone()
        {
            var pointSet = new PointSet();
            foreach (var cloudPoint in Points)
            {
                pointSet.Add((CloudPoint) cloudPoint.Clone());
            }

            return pointSet;
        }

        private Point GetNormalizationOffset()
        {
            return new()
            {
                X = P1.X + (P2.X - P1.X) / 2,
                Y = P1.Y + (P2.Y - P1.Y) / 2,
                Z = P1.Z + (P2.Z - P1.Z) / 2
            };
        }

        public void Normalize()
        {
            var offset = GetNormalizationOffset();
            for (var i = 0; i < Count; i++)
            {
                this[i].X -= offset.X;
                this[i].Y -= offset.Y;
                this[i].Z -= offset.Z;
            }

            RecalculateBoundingBox();
        }

        public PointSet Normalized()
        {
            var pointSet = new PointSet();
            var offset = GetNormalizationOffset();
            for (var i = 0; i < Count; i++)
            {
                var p = Points[i].Clone() as CloudPoint;
                if(p == null)
                {
                    continue;
                }

                p.X -= offset.X;
                p.Y -= offset.Y;
                p.Z -= offset.Z;
                pointSet.Add(p);
            }

            return pointSet;
        }

        public IList<PointSlice> SplitByHeight(Box analyzedBox, float sliceHeight)
        {
            var box = analyzedBox.Intersect(BoundingBox);
            var minZ = box.P1.Z;
            var maxZ = box.P2.Z;
            var slices = new PointSlice[(int)Math.Ceiling((maxZ - minZ) / sliceHeight)];
            var step = 0;

            foreach (var cloudPoint in Points)
            {
                if (box.Contains(cloudPoint) == false)
                {
                    continue;
                }
                var z = (int)((cloudPoint.Z - minZ) / sliceHeight);
                if (slices[z] == null)
                {
                    slices[z] = new PointSlice
                    {
                        PointSets = new List<PointSet>(),
                        Height = Math.Round(cloudPoint.Z,1)
                    };
                    slices[z].PointSets.Add(new PointSet());
                }
                slices[z].PointSets[0].Add(cloudPoint);
                ProgressTracker.Progress(EProgressStage.Slicing, "Slicing Cloud of Points", step++, Points.Count);
            }

            return slices;
        }
        public Tree.Node FindBestNode(IList<Tree> potentialTrees, MergingParameters parameters)
        {
            double bestDistance = int.MaxValue;
            Tree.Node bestTreeNode = null;

            foreach (var tree in potentialTrees)
            {
                var z = Center.Z;
                var nodes = tree.GetNodeByHeightWithLeaves(z);

                foreach (var node in nodes)
                {
                    if (node.PointsOutTo(Center, parameters))
                    {
                        return node;
                    }
                }

                var treeNode = tree.GetNearestNode(Center, out var distance, parameters.SliceHeight);

                if (treeNode == null)
                {
                    continue;
                }

                if (distance >= parameters.MinimumGroupingDistance)
                {
                    continue;
                }

                if (distance >= bestDistance)
                {
                    continue;
                }

                if (Center.Z - treeNode.Center.Z >= parameters.MaximumGroupingEmptyHeight)
                {
                    continue;
                }

                if (Center.Z - treeNode.Center.Z <= 0)
                {
                    continue;
                }

                bestDistance = distance;
                bestTreeNode = treeNode;
            }

            return bestTreeNode;
        }

        public IEnumerator<CloudPoint> GetEnumerator() => Points.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Points.GetEnumerator();
    }
}