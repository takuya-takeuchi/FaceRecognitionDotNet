using System;
using System.Collections.Generic;
using Xunit;

namespace FaceRecognitionDotNet.Tests
{

    public class HeadPoseTest
    {

        [Fact]
        public void Equal()
        {
            var pose1 = new HeadPose(10, 20, 9);
            var pose2 = new HeadPose(10, 20, 9);
            Assert.Equal(pose1, pose2);
            Assert.True(pose1 == pose2);
            Assert.True(pose1.Equals(pose2));
            Assert.False(pose1 != pose2);
        }

        [Fact]
        public void NotEqual1()
        {
            var pose1 = new HeadPose(10, 20, 9);
            var pose2 = new HeadPose(40, 20, 9);
            Assert.NotEqual(pose1, pose2);
            Assert.True(pose1 != pose2);
            Assert.True(!pose1.Equals(pose2));
            Assert.False(pose1 == pose2);
        }

        [Fact]
        public void NotEqual2()
        {
            var pose1 = new HeadPose(40, 10, 9);
            var pose2 = new HeadPose(40, 20, 9);
            Assert.NotEqual(pose1, pose2);
            Assert.True(pose1 != pose2);
            Assert.True(!pose1.Equals(pose2));
            Assert.False(pose1 == pose2);
        }

        [Fact]
        public void NotEqual3()
        {
            var pose1 = new HeadPose(40, 20, 9);
            var pose2 = new HeadPose(40, 20, 0);
            Assert.NotEqual(pose1, pose2);
            Assert.True(pose1 != pose2);
            Assert.True(!pose1.Equals(pose2));
            Assert.False(pose1 == pose2);
        }

        [Fact]
        public void Hash()
        {
            var pose1 = new HeadPose(40, 20, 9);
            var pose2 = new HeadPose(40, 20, 0);

            var dictionary = new Dictionary<HeadPose, int>();
            dictionary.Add(pose1, dictionary.Count);

            try
            {
                dictionary.Add(pose2, dictionary.Count);
            }
            catch
            {
                Assert.True(false, $"{typeof(HeadPose)} must not throw exception.");
            }

            try
            {
                dictionary.Add(pose2, dictionary.Count);
                Assert.True(false, $"{typeof(HeadPose)} must throw exception because key is duplicate.");
            }
            catch (ArgumentException)
            {
            }
        }

    }

}