using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using LambdaModel.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LambdaModel.Tests.Utilities
{
    [TestClass]
    public class SpiralGridEnumeratorTests
    {
        [TestMethod]
        public void TinyTest()
        {
            var values = SpiralGridEnumerator.Enumerate(1).ToArray();
            Assert.AreEqual(9, values.Length);

            var correct = new (int x, int y)[]
            {
                (-1, -1),
                (0, -1),
                (1, -1),
                (1, 0),
                (1, 1),
                (0, 1),
                (-1, 1),
                (-1, 0),
                (0, 0)
            };

            for (var i = 0; i < correct.Length; i++)
            {
                Assert.AreEqual(correct[i].x, values[i].x);
                Assert.AreEqual(correct[i].y, values[i].y);
            }
        }

        [TestMethod]
        public void LargerTest()
        {
            var values = SpiralGridEnumerator.Enumerate(10).ToArray();

            // Correct array size
            Assert.AreEqual((10 * 2 + 1) * (10 * 2 + 1), values.Length);

            // All values within valid range
            foreach (var v in values)
            {
                Assert.IsTrue(v.x >= -10);
                Assert.IsTrue(v.x <= 10);
                Assert.IsTrue(v.y >= -10);
                Assert.IsTrue(v.y <= 10);
            }

            // No duplicates
            Assert.IsTrue(values.GroupBy(p => p.x + "_" + p.y).All(p => p.Count() == 1));
        }
    }
}
