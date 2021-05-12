using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ForestTaxator.Utils;
using static ForestTaxator.Utils.MathUtils;

namespace ForestTaxator.Model
{
    public class PointSet : IEnumerable<CloudPoint>
    {
        private Point _centerPoint;
        private bool _boundingBoxDeprecated;
        private Box _boundingBox;

        public Point Center
        {
            get
            {
                return _centerPoint ??= new Point()
                {
                    X = Points.Sum(x => x.X) / Points.Count,
                    Y = Points.Sum(x => x.Y) / Points.Count,
                    Z = Points.Sum(x => x.Z) / Points.Count,
                };
            }
            private set => _centerPoint = value;
        }

        public Box BoundingBox
        {
            get
            {
                if (_boundingBoxDeprecated)
                {
                    return BoundingBox = Box.Find(this);
                }

                return _boundingBox;
            }
            private set
            {
                _boundingBox = value;
                _boundingBoxDeprecated = false;
            }
        }

        public Point P1 { get; private set; }
        public Point P2 { get; private set; }


        private List<CloudPoint> Points { get; set; } = new List<CloudPoint>();

        public bool Empty => Points.Count == 0;

        public long Count => Points.Count;

        public PointSet()
        {
            _boundingBoxDeprecated = true;
            _centerPoint = new Point();
        }

        public PointSet(IEnumerable<CloudPoint> points)
        {
            Points.AddRange(points);
            _boundingBoxDeprecated = true;
            _centerPoint = new Point();
        }

        public void RecalculateBoundingBox()
        {
            _boundingBox = Box.Find(this);
            _boundingBoxDeprecated = false;
            P1 = BoundingBox.P1;
            P2 = BoundingBox.P2;
        }

        public CloudPoint this[long indexer] => indexer < Count ? Points[(int) indexer] : null;

        public void SetBoundingBox(Point center)
        {
            if (center == null)
            {
                return;
            }

            BoundingBox.P1 = center.Clone();
            BoundingBox.P2 = center.Clone();

            _boundingBoxDeprecated = false;
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
            _centerPoint = null;
        }
        
        public void Clear() => Points.Clear();

        public void Remove(CloudPoint point)
        {
            _centerPoint = null;
            if (point.OwningSet != this)
            {
                return;
            }

            Points.Remove(point);
            _boundingBoxDeprecated = true;
        }

        public void MoveTo(PointSet otherSet)
        {
            for (var i = Count - 1; i >= 0; i--)
            {
                var p = this[i];
                p.DetachFromSet();
                otherSet.Add(p);
            }

            _boundingBoxDeprecated = true;
            _centerPoint = null;
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

        public PointSlice[] SplitByHeight(Box analyzedBox, float sliceHeight)
        {
            var box = analyzedBox.Intersect(BoundingBox);
            var minZ = box.P1.Z;
            var maxZ = box.P2.Z;
            var slices = new PointSlice[(int)Math.Ceiling((maxZ - minZ) / sliceHeight)];

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
            }

            return slices;
        }
        
        public Terrain FindTerrain(Box analyzedBox)
        {
            var terrain = new Terrain();
            var box = analyzedBox.Intersect(BoundingBox);
            foreach (var cloudPoint in Points)
            {
                if (box.Contains(cloudPoint) == false)
                {
                    continue;
                }

                // ReSharper disable PossibleLossOfFraction
                var x = (int)(cloudPoint.X / Terrain.MeshSize + Terrain.MeshesCount / 2);
                var y = (int)(cloudPoint.Y / Terrain.MeshSize + Terrain.MeshesCount / 2);
                // ReSharper restore PossibleLossOfFraction
                if (x is < 0 or >= Terrain.MeshesCount || y is < 0 or >= Terrain.MeshesCount)
                {
                    continue;
                }

                if (terrain.Mesh[x, y] > cloudPoint.Z)
                {
                    terrain.Mesh[x, y] = cloudPoint.Z;
                }
            }

            return terrain;
        }

        public Distribution GetDistribution(EDimension dimension, int steps)
        {
            var distribution = new int[steps];
            var setSize = P2[(int) dimension] - P1[(int) dimension];
            var stepSize = setSize / (steps - 1);

            for (var i = 0; i < Count; i++)
            {
                var index = (int) ((this[i][(int) dimension] + setSize / 2) / stepSize);
                distribution[index]++;
            }

            return new Distribution(distribution);
        }

        public Distribution[] GetDistribution(int steps, params EDimension[] dimensions)
        {
            var distributions = new Distribution[dimensions.Length];
            for (var i = 0; i < dimensions.Length; i++)
            {
                distributions[i] = GetDistribution(dimensions[i], steps);
            }

            return distributions;
        }

        public IEnumerator<CloudPoint> GetEnumerator() => Points.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Points.GetEnumerator();
    }
}