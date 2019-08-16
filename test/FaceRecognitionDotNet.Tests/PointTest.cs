using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FaceRecognitionDotNet.Tests
{

    [TestClass]
    public class PointTest
    {

        [TestMethod]
        public void Equal()
        {
            var point1 = new Point(10, 20);
            var point2 = new Point(10, 20);
            Assert.AreEqual(point1, point2);
            Assert.IsTrue(point1 == point2);
            Assert.IsFalse(point1 != point2);
        }

        [TestMethod]
        public void NotEqual()
        {
            var point1 = new Point(10, 20);
            var point2 = new Point(40, 10);
            Assert.AreNotEqual(point1, point2);
            Assert.IsTrue(point1 != point2);
            Assert.IsFalse(point1 == point2);
        }

        [TestMethod]
        public void Hash()
        {
            var point1 = new Point(10, 20);
            var point2 = new Point(40, 10);

            var dictionary = new Dictionary<Point, int>();
            dictionary.Add(point1, dictionary.Count);

            try
            {
                dictionary.Add(point2, dictionary.Count);
            }
            catch
            {
                Assert.Fail($"{typeof(Point)} must not throw exception.");
            }

            try
            {
                dictionary.Add(point2, dictionary.Count);
                Assert.Fail($"{typeof(Point)} must throw exception because key is duplicate.");
            }
            catch
            {
            }
        }

    }

}