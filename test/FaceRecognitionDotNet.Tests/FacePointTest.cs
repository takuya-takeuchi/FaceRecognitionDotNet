using System;
using System.Collections.Generic;
using Xunit;

namespace FaceRecognitionDotNet.Tests
{

    public class FacePointTest
    {

        [Fact]
        public void Equal()
        {
            var point1 = new FacePoint(new Point(10, 20), 0);
            var point2 = new FacePoint(new Point(10, 20), 0);
            Assert.Equal(point1, point2);
            Assert.True(point1 == point2);
            Assert.True(point1.Equals(point2));
            Assert.False(point1 != point2);
        }

        [Fact]
        public void NotEqual()
        {
            var point1 = new FacePoint(new Point(10, 20), 0);
            var point2 = new FacePoint(new Point(10, 20), 1);
            Assert.NotEqual(point1, point2);
            Assert.True(point1 != point2);
            Assert.True(!point1.Equals(point2));
            Assert.False(point1 == point2);
        }

        [Fact]
        public void NotEqual2()
        {
            var point1 = new FacePoint(new Point(10, 20), 0);
            var point2 = new FacePoint(new Point(10, 10), 0);
            Assert.NotEqual(point1, point2);
            Assert.True(point1 != point2);
            Assert.True(!point1.Equals(point2));
            Assert.False(point1 == point2);
        }

        [Fact]
        public void Hash()
        {
            var point1 = new FacePoint(new Point(10, 20), 0);
            var point2 = new FacePoint(new Point(10, 20), 1);

            var dictionary = new Dictionary<FacePoint, int>();
            dictionary.Add(point1, dictionary.Count);

            try
            {
                dictionary.Add(point2, dictionary.Count);
            }
            catch
            {
                Assert.True(false, $"{typeof(FacePoint)} must not throw exception.");
            }

            try
            {
                dictionary.Add(point2, dictionary.Count);
                Assert.True(false, $"{typeof(FacePoint)} must throw exception because key is duplicate.");
            }
            catch (ArgumentException)
            {
            }
        }

    }

}