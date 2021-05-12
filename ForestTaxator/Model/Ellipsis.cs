using System;
using static ForestTaxator.Utils.MathUtils;
using static ForestTaxator.Utils.MathUtils.EDistanceMetric;

namespace ForestTaxator.Model
{
    public class Ellipsis
    {
        private double? _focalLength;
        private double? _eccentricity;
        private double _majorRadius;
        private double? _minorRadius;

        private Point _center;
        private Point _firstFocal;
        private Point _secondFocal;

        public Ellipsis()
        {

        }

        public Ellipsis(EllipticParameters parameters, double z)
        {
            FirstFocal = new Point(parameters.X1, parameters.Y1, z);
            SecondFocal = new Point(parameters.X2, parameters.Y2, z);
            MajorRadius = parameters.A;
        }

        public Ellipsis(Point firstFocal, Point secondFocal, double majorRadius)
        {
            FirstFocal = firstFocal;
            SecondFocal = secondFocal;
            MajorRadius = majorRadius;
        }

        public Ellipsis Clone() =>
            new()
            {
                FirstFocal = FirstFocal.Clone(),
                SecondFocal = SecondFocal.Clone(),
                MajorRadius = MajorRadius,
                Error = Error,
            };

        public Point FirstFocal
        {
            get => _firstFocal;
            set {
                _firstFocal = value;
                _focalLength = null;
                _eccentricity = null;
                _minorRadius = null;
            }
        }

        public Point SecondFocal
        {
            get => _secondFocal;
            set {
                _secondFocal = value;
                _focalLength = null;
                _eccentricity = null;
                _minorRadius = null;
            }

        }

        public double MajorRadius
        {
            get => _majorRadius;
            set {
                _majorRadius = value;
                _eccentricity = null;
                _minorRadius = null;
            }
        }

        public double MinorRadius
        {
            get
            {
                _minorRadius ??= Math.Sqrt((1 - (Eccentricity * Eccentricity)) * MajorRadius * MajorRadius);

                return _minorRadius.Value;
            }
        }

        public Point Center =>
            _center ??= new Point
            {
                X = (FirstFocal.X + SecondFocal.X) / 2.0f,
                Y = (FirstFocal.Y + SecondFocal.Y) / 2.0f,
                Z = (FirstFocal.Z + SecondFocal.Z) / 2.0f,
            };

        public double Error { get; set; }

        public void SetFirstFocal(EDimension dimension, float newValue)
        {
            _firstFocal[(int)dimension] = newValue;
            _focalLength = null;
            _eccentricity = null;
            _minorRadius = null;
        }

        public void SetSecondFocal(EDimension dimension, float newValue)
        {
            _secondFocal[(int)dimension] = newValue;
            _focalLength = null;
            _eccentricity = null;
            _minorRadius = null;
        }

        public double Eccentricity => _eccentricity ??= FocalLength / (2 * MajorRadius);

        public double FocalLength => _focalLength ??= Distance(FirstFocal, SecondFocal,
            Euclidean, EDimension.X,
            EDimension.Y);

        

        public bool Contains(Point point)
        {
            return Distance(point, FirstFocal, Euclidean, EDimension.X, EDimension.Y) 
                + Distance(point, SecondFocal, Euclidean, EDimension.X, EDimension.Y) <  2 * MajorRadius;
        }

    }
}