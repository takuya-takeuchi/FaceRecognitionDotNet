using System.Collections.Generic;
using Xunit;

namespace FaceRecognitionDotNet.Tests
{

    public class HeadPoseTest
    {

        [Fact]
        public void Equal()
        {
            var point1 = new HeadPose(10, 20, 9);
            var point2 = new HeadPose(10, 20, 9);
            Assert.Equal(point1, point2);
            Assert.True(point1 == point2);
            Assert.False(point1 != point2);
        }

        [Fact]
        public void NotEqual1()
        {
            var point1 = new HeadPose(10, 20, 9);
            var point2 = new HeadPose(40, 20, 9);
            Assert.NotEqual(point1, point2);
            Assert.True(point1 != point2);
            Assert.False(point1 == point2);
        }

        [Fact]
        public void NotEqual2()
        {
            var point1 = new HeadPose(40, 10, 9);
            var point2 = new HeadPose(40, 20, 9);
            Assert.NotEqual(point1, point2);
            Assert.True(point1 != point2);
            Assert.False(point1 == point2);
        }

        [Fact]
        public void NotEqual3()
        {
            var point1 = new HeadPose(40, 20, 9);
            var point2 = new HeadPose(40, 20, 0);
            Assert.NotEqual(point1, point2);
            Assert.True(point1 != point2);
            Assert.False(point1 == point2);
        }

        [Fact]
        public void Hash()
        {
            var point1 = new HeadPose(40, 20, 9);
            var point2 = new HeadPose(40, 20, 0);

            var dictionary = new Dictionary<HeadPose, int>();
            dictionary.Add(point1, dictionary.Count);

            try
            {
                dictionary.Add(point2, dictionary.Count);
            }
            catch
            {
                Assert.True(false, $"{typeof(HeadPose)} must not throw exception.");
            }

            try
            {
                dictionary.Add(point2, dictionary.Count);
                Assert.True(false, $"{typeof(HeadPose)} must throw exception because key is duplicate.");
            }
            catch
            {
            }
        }

    }

}