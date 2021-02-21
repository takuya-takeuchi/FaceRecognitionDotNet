using System;
using System.Collections.Generic;
using Xunit;

namespace FaceRecognitionDotNet.Tests
{

    public class PointTest
    {

        [Fact]
        public void Equal()
        {
            var point1 = new Point(10, 20);
            var point2 = new Point(10, 20);
            Assert.Equal(point1, point2);
            Assert.True(point1 == point2);
            Assert.True(point1.Equals(point2));
            Assert.False(point1 != point2);
        }

        [Fact]
        public void NotEqual()
        {
            var point1 = new Point(10, 20);
            var point2 = new Point(40, 10);
            Assert.NotEqual(point1, point2);
            Assert.True(point1 != point2);
            Assert.True(!point1.Equals(point2));
            Assert.False(point1 == point2);
        }

        [Fact]
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
                Assert.True(false, $"{typeof(Point)} must not throw exception.");
            }

            try
            {
                dictionary.Add(point2, dictionary.Count);
                Assert.True(false, $"{typeof(Point)} must throw exception because key is duplicate.");
            }
            catch (ArgumentException)
            {
            }
        }

    }

}