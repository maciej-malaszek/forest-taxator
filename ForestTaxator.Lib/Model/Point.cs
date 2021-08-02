using System;

namespace ForestTaxator.Lib.Model
{
    public class Point
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public Point()
        {

        }

        public Point(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double this[int index]
        {
            get
            {
                return index switch
                {
                    0 => X,
                    1 => Y,
                    2 => Z,
                    _ => -1
                };
            }
            set
            {
                switch (index)
                {
                    case 0: X = value; break;
                    case 1: Y = value; break;
                    case 2: Z = value; break;
                }
            }
        }
        public virtual Point Clone()
        {
            return new()
            {
                X = X,
                Y = Y,
                Z = Z,
            };
        }

        public virtual byte[] BinarySerialized()
        {
            var data = new byte[32];
            Array.Copy(BitConverter.GetBytes(X),data,8);
            Array.Copy(BitConverter.GetBytes(Y),0, data,8, 8);
            Array.Copy(BitConverter.GetBytes(Z),0, data,16, 8);
            return data;
        }

        public virtual string StringSerialized()
        {
            return $"{X:0.########} {Y:0.########} {Z:0.########}";
        }

        public override string ToString()
        {
            return StringSerialized();
        }

        public static Point operator -(Point p1, Point p2)
        {
            return new(p1.X - p2.X, p1.Y - p2.Y, p1.Z - p2.Z);
        }
        
        public static Point operator +(Point p1, Point p2)
        {
            return new(p1.X + p2.X, p1.Y + p2.Y, p1.Z + p2.Z);
        }
    }
}
