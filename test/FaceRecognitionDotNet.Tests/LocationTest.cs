using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FaceRecognitionDotNet.Tests
{

    [TestClass]
    public class LocationTest
    {

        [TestMethod]
        public void Equal()
        {
            var location1 = new Location(10, 20, 30, 40);
            var location2 = new Location(10, 20, 30, 40);
            Assert.AreEqual(location1, location2);
        }

        [TestMethod]
        public void NotEqual()
        {
            var location1 = new Location(10, 20, 30, 40);
            var location2 = new Location(40, 10, 20, 30);
            Assert.AreNotEqual(location1, location2);
        }

        [TestMethod]
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
                Assert.Fail($"{typeof(Location)} must not throw exception.");
            }

            try
            {
                dictionary.Add(location2, dictionary.Count);
                Assert.Fail($"{typeof(Location)} must throw exception because key is duplicate.");
            }
            catch
            {
            }
        }

    }

}