
using System;

namespace ForestTaxator.Model
{
    public class Box
    {
        private Point _p1;
        private Point _p2;
        
        // Most -X, -Y, -Z
        public Point P1 { 
            get => _p1;
            set { 
                _p1 = value;
                _center = null;
            }
        }
        
        // Most +X, +Y, +Z
        public Point P2 { 
            get => _p2;
            set { 
                _p2 = value;
                _center = null;
            }
        }

        public double Width => P2.X-P1.X;
        public double Height => P2.Z-P1.Z;
        public double Depth => P2.Y-P1.Y;

        public bool Empty => P1.X == 0 && P2.X == 0 && P1.Y == 0 && P2.Y == 0 && P1.Z == 0 && P2.Z == 0;

        public Box()
        {
            P1 = new Point();
            P2 = new Point();
        }

        public Box(Point x1, Point x2)
        {
            P1 = x1;
            P2 = x2;
        }

        private Point _center;

        public Point Center => _center ??= new Point((P1.X + P2.X) / 2, (P1.Y + P2.Y) / 2, (P1.Z + P2.Z) / 2);

        /// <summary>
        /// Checks if point is physically in this Box
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool Contains(CloudPoint point)
        {
            return  point.X <= P2.X && point.X >= P1.X &&
                    point.Y <= P2.Y && point.Y >= P1.Y &&
                    point.Z <= P2.Z && point.Z >= P1.Z;
        }

        public bool Includes(Box box)
        {
            return  box.P2.X <= P2.X && box.P1.X >= P1.X &&
                    box.P2.Y <= P2.Y && box.P1.Y >= P1.Y &&
                    box.P2.Z <= P2.Z && box.P1.Z >= P1.Z;
        }

        public bool IncludesPlanar(Box box)
        {
            return box.P2.X <= P2.X && P1.X >= box.P2.X &&
                    box.P2.Y <= P2.Y && P1.Y >= box.P2.Y;
        }

        public Box Intersect(Box box)
        {
            return new Box(new CloudPoint
            {
                X = Math.Max(P1.X, box.P1.X),
                Y = Math.Max(P1.Y, box.P1.Y),
                Z = Math.Max(P1.Z, box.P1.Z),
            },
            new CloudPoint
            {
                X = Math.Min(P2.X, box.P2.X),
                Y = Math.Min(P2.Y, box.P2.Y),
                Z = Math.Min(P2.Z, box.P2.Z),
            });
        }

        public void Broaden(Box box)
        {
            if (box.Empty)
            {
                return;
            }

            if (box.P1.X < P1.X)
            {
                P1.X = box.P1.X;
            }

            if (box.P2.X > P2.X)
            {
                P2.X = box.P2.X;
            }

            if (box.P1.Y < P1.Y)
            {
                P1.Y = box.P1.Y;
            }

            if (box.P2.Y > P2.Y)
            {
                P2.Y = box.P2.Y;
            }

            if (box.P1.Z < P1.Z)
            {
                P1.Z = box.P1.Z;
            }

            if (box.P2.Z > P2.Z)
            {
                P2.Z = box.P2.Z;
            }
        }

        public void Broaden(CloudPoint p)
        {
            P1.X = Math.Min(p.X, P1.X);
            P2.X = Math.Max(p.X, P2.X);
            P1.Y = Math.Min(p.Y, P1.Y);
            P2.Y = Math.Max(p.Y, P2.Y);
            P1.Z = Math.Min(p.Z, P1.Z);
            P2.Z = Math.Max(p.Z, P2.Z);
        }

        public bool BoundsWithPlanar(Box other)
        {
            if (IncludesPlanar(other))
            {
                return true;
            }

            if (other.IncludesPlanar(this))
            {
                return true;
            }

            if (other.P1.X <= P2.X == false || other.P2.X >= P1.X == false)
            {
                return false;
            }

            return other.P1.Y <= P2.Y && other.P2.Y >= P1.Y;
        }

        public bool BoundsWith(Box other)
        {
            if (Includes(other))
            {
                return true;
            }

            if (other.Includes(this))
            {
                return true;
            }

            if (!(other.P1.X <= P2.X) || !(other.P2.X >= P1.X))
            {
                return false;
            }

            if (!(other.P1.Y <= P2.Y) || !(other.P2.Y >= P1.Y))
            {
                return false;
            }

            return other.P1.Z <= P2.Z && other.P2.Z >= P1.Z;
        }


        public static Box Find(PointSet pointSet)
        {
            if (pointSet.Count < 1)
            {
                return new Box();
            }

            var p = pointSet[0];
            var box = new Box(p.Clone(), p.Clone());

            for (long i = 1; i < pointSet.Count; i++)
            {
                p = pointSet[i];
                if (p.X < box.P1.X)
                {
                    box.P1.X = p.X;
                }

                if (p.X > box.P2.X)
                {
                    box.P2.X = p.X;
                }

                if (p.Y < box.P1.Y)
                {
                    box.P1.Y = p.Y;
                }

                if (p.Y > box.P2.Y)
                {
                    box.P2.Y = p.Y;
                }

                if (p.Z < box.P1.Z)
                {
                    box.P1.Z = p.Z;
                }

                if (p.Z > box.P2.Z)
                {
                    box.P2.Z = p.Z;
                }
            }

            //foreach (PointSet s in pointSet.Subsets)
            //    box.Broaden(s.BoundingBox);

            return box;

        }

        public double Rdiff(Box other)
        {
            double res = 0;
            if( other.P1.X < P1.X )
            {
                res += P1.X - other.P1.X;
            }

            if( other.P2.X > P2.X )
            {
                res += other.P2.X - P2.X;
            }

            if( other.P1.Y < P1.Y )
            {
                res += P1.Y - other.P1.Y;
            }

            if( other.P2.Y > P2.Y )
            {
                res += other.P2.Y - P2.Y;
            }

            if( other.P1.Z < P1.Z )
            {
                res += P1.Z - other.P1.Z;
            }

            if( other.P2.Z > P2.Z )
            {
                res += other.P2.Z - P2.Z;
            }

            return res;
        }

        public double RdiffPlanar(Box other)
        {
            double res = 0;
            if (other.P1.X < P1.X)
            {
                res += P1.X - other.P1.X;
            }

            if (other.P2.X > P2.X)
            {
                res += other.P2.X - P2.X;
            }

            if (other.P1.Y < P1.Y)
            {
                res += P1.Y - other.P1.Y;
            }

            if (other.P2.Y > P2.Y)
            {
                res += other.P2.Y - P2.Y;
            }

            return res;
        }

        public void Expand(float size)
        {
            P1.X -= size;
            P1.Y -= size;
            P1.Z -= size;
            P2.X += size;
            P2.Y += size;
            P2.Z += size;
        }
    }
}
