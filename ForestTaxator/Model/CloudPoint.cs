using System;

namespace ForestTaxator.Model
{
    [Serializable]
    public class CloudPoint : Point
    {
        public float Intensity { get; set; }
        public PointSet OwningSet { get; private set; }

        public CloudPoint() {  }

        public CloudPoint(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override byte[] Serialize()
        {
            var data = new byte[32];
            Array.Copy(BitConverter.GetBytes(X),data,8);
            Array.Copy(BitConverter.GetBytes(Y),0, data,8, 8);
            Array.Copy(BitConverter.GetBytes(Z),0, data,16, 8);
            Array.Copy(BitConverter.GetBytes(Intensity),0, data,24, 8);
            return data;
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
