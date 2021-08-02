using ForestTaxator.Lib.Model;
using NUnit.Framework;

namespace ForestTaxator.UnitTests.Utils.MathUtils
{
    public class DistanceUnitTests
    {

        [Test]
        [TestCase(-1,1,0,0,0,0, 2)]
        [TestCase(0,3,0,4,0,0, 5)]
        [TestCase(0,-3,0,4,0,0, 5)]
        [TestCase(0,0,0,3,0,4, 5)]
        public void EuclideanDistance_3D_ReturnsCorrectValues(double x1, double x2, double y1, double y2, double z1, double z2, double expectedDistance)
        {
            var p1 = new Point(x1, y1, z1);
            var p2 = new Point(x2, y2, z2);
            var distance = Lib.Utils.MathUtils.Distance(p1, p2, 
                Lib.Utils.MathUtils.EDistanceMetric.Euclidean,
                Lib.Utils.MathUtils.EDimension.X, 
                Lib.Utils.MathUtils.EDimension.Y,
                Lib.Utils.MathUtils.EDimension.Z);
            
            Assert.AreEqual(expectedDistance, distance, float.Epsilon);
        }
    }
}