﻿using System;

namespace ForestTaxator.Model
{
    public class Point
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public float Intensity { get; set; }

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

        public virtual byte[] Serialize()
        {
            var data = new byte[32];
            Array.Copy(BitConverter.GetBytes(X),data,8);
            Array.Copy(BitConverter.GetBytes(Y),0, data,8, 8);
            Array.Copy(BitConverter.GetBytes(Z),0, data,16, 8);
            Array.Copy(BitConverter.GetBytes(Intensity),0, data,24, 8);
            return data;
        }
    }
}
