using System;
using System.Collections.Generic;
using ForestTaxator.Lib.Model;

namespace ForestTaxator.Lib.Utils
{
    public static class MathUtils
    {
        public static void Regression(IList<double> x, IList<double> y, out double af, out double bf)
        {
            af = 0;
            bf = 0;

            if (x.Count != y.Count)
            {
                return;
            }

            double sumX = 0, sumY = 0, sumXY = 0, sumOfSquares = 0;


            for (var i = 0; i < x.Count; ++i)
            {
                var xI = x[i];
                var yI = y[i];

                sumOfSquares += xI * xI;
                sumX += xI;
                sumY += yI;

                sumXY += yI * xI;
            }

            af = (x.Count * sumXY - sumX * sumY) / (x.Count * sumOfSquares - sumX * sumX);
            bf = (sumY - af * sumX) / x.Count;
        }

        public static LinearParameters Regression(IList<double> x, IList<double> y)
        {
            Regression(x, y, out var a, out var b);
            return new LinearParameters {A = a, B = b};
        }

        public struct LinearParameters
        {
            public double A { get; set; }
            public double B { get; set; }
        }


        public enum EDimension
        {
            X = 0,
            Y = 1,
            Z = 2
        }

        public enum EDistanceMetric
        {
            Euclidean,
            Manhattan,
            EuclideanSquare
        }

        private static double EuclideanDistance2D(double x1, double y1, double x2, double y2) =>
            Math.Sqrt(Math.Pow(x2 - x1, 2.0) + Math.Pow(y2 - y1, 2.0));

        private static double ManhattanDistance2D(double x1, double y1, double x2, double y2) =>
            Math.Abs(x2 - x1) + Math.Abs(y2 - y1);

        private static double EuclideanDistance3D(double x1, double y1, double z1, double x2, double y2, double z2) =>
            Math.Sqrt(Math.Pow(x2 - x1, 2.0) + Math.Pow(y2 - y1, 2.0) + Math.Pow(z2 - z1, 2.0));

        private static double ManhattanDistance3D(double x1, double y1, double z1, double x2, double y2, double z2) =>
            Math.Abs(x2 - x1) + Math.Abs(y2 - y1) + Math.Abs(z2 - z1);


        private static double EuclideanSquareDistance2D(double x1, double y1, double x2, double y2) =>
            Math.Pow(x2 - x1, 2.0) + Math.Pow(y2 - y1, 2.0);

        private static double EuclideanSquareDistance3D(double x1, double y1, double z1, double x2, double y2, double z2) =>
            Math.Pow(x2 - x1, 2.0) + Math.Pow(y2 - y1, 2.0) + Math.Pow(z2 - z1, 2.0);


        public static double Distance(Point p1, Point p2, EDistanceMetric metricType, params EDimension[] dimension)
        {
            return dimension.Length switch
            {
                1 => Math.Abs(p1[(int) dimension[0]] - p2[(int) dimension[0]]),
                2 => metricType switch
                {
                    EDistanceMetric.Euclidean => EuclideanDistance2D(p1[(int) dimension[0]], p1[(int) dimension[1]], p2[(int) dimension[0]],
                        p2[(int) dimension[1]]),
                    EDistanceMetric.Manhattan => ManhattanDistance2D(p1[(int) dimension[0]], p1[(int) dimension[1]], p2[(int) dimension[0]],
                        p2[(int) dimension[1]]),
                    EDistanceMetric.EuclideanSquare => EuclideanSquareDistance2D(p1[(int) dimension[0]], p1[(int) dimension[1]],
                        p2[(int) dimension[0]], p2[(int) dimension[1]]),
                    _ => EuclideanDistance2D(p1[(int) dimension[0]], p1[(int) dimension[1]], p2[(int) dimension[0]], p2[(int) dimension[1]])
                },
                3 => metricType switch
                {
                    EDistanceMetric.Euclidean => EuclideanDistance3D(p1.X, p1.Y, p1.Z, p2.X, p2.Y, p2.Z),
                    EDistanceMetric.Manhattan => ManhattanDistance3D(p1.X, p1.Y, p1.Z, p2.X, p2.Y, p2.Z),
                    EDistanceMetric.EuclideanSquare => EuclideanSquareDistance3D(p1.X, p1.Y, p1.Z, p2.X, p2.Y, p2.Z),
                    _ => EuclideanDistance3D(p1.X, p1.Y, p1.Z, p2.X, p2.Y, p2.Z)
                },
                _ => 0.0f
            };
        }
    }
}