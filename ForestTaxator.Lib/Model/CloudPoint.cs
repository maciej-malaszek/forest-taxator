using System;

namespace ForestTaxator.Lib.Model
{
    [Serializable]
    public class CloudPoint : Point
    {
        public double Intensity { get; set; }
        public PointSet OwningSet { get; private set; }

        public CloudPoint() {  }

        public CloudPoint(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override byte[] BinarySerialized()
        {
            var data = new byte[32];
            Array.Copy(BitConverter.GetBytes(X),data,sizeof(double));
            Array.Copy(BitConverter.GetBytes(Y),0, data,sizeof(double), sizeof(double));
            Array.Copy(BitConverter.GetBytes(Z),0, data,2*sizeof(double), sizeof(double));
            Array.Copy(BitConverter.GetBytes(Intensity),0, data,3*sizeof(double), sizeof(double));
            return data;
        }

        public override string StringSerialized()
        {
            return $"{X:0.########} {Y:0.########} {Z:0.########} {Intensity:0.########}";
        }

        public void DetachFromSet()
        {
            if(OwningSet == null)
            {
                return;
            }

            OwningSet.Remove(this);
            OwningSet = null;
        }

        public override Point Clone()
        {
            return new CloudPoint(X, Y, Z)
            {
                Intensity = Intensity,
                OwningSet = OwningSet
            };
        }

        public void AttachToSet(PointSet pointSet)
        {
            if(pointSet == null)
            {
                return;
            }

            OwningSet = pointSet;
            OwningSet.Add(this);
        }

    }
}
