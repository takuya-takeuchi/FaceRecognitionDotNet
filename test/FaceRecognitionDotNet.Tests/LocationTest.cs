using System;
using System.Collections.Generic;
using Xunit;

namespace FaceRecognitionDotNet.Tests
{

    public class LocationTest
    {

        [Fact]
        public void Equal()
        {
            var location1 = new Location(10, 20, 30, 40);
            var location2 = new Location(10, 20, 30, 40);
            Assert.Equal(location1, location2);
            Assert.True(location1.Equals(location2));
        }

        [Fact]
        public void NotEqual()
        {
            var location1 = new Location(10, 20, 30, 40);
            var location2 = new Location(40, 10, 20, 30);
            Assert.NotEqual(location1, location2);
            Assert.True(!location1.Equals(location2));
        }

        [Fact]
        public void Hash()
        {
            var location1 = new Location(10, 20, 30, 40);
            var location2 = new Location(40, 10, 20, 30);

            var dictionary = new Dictionary<Location, int>();
            dictionary.Add(location1, dictionary.Count);

            try
            {
                dictionary.Add(location2, dictionary.Count);
            }
            catch
            {
                Assert.True(false, $"{typeof(Location)} must not throw exception.");
            }

            try
            {
                dictionary.Add(location2, dictionary.Count);
                Assert.True(false, $"{typeof(Location)} must throw exception because key is duplicate.");
            }
            catch (ArgumentException)
            {
            }
        }

    }

}