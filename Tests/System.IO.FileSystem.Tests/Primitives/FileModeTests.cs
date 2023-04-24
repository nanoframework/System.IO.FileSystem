// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using nanoFramework.TestFramework;

namespace System.IO.FileSystem.Tests.Primitives
{
    [TestClass]
    public static class FileModeTests
    {
        [TestMethod]
        public static void ValueTest()
        {
            Assert.AreEqual(1, (int)FileMode.CreateNew);
            Assert.AreEqual(2, (int)FileMode.Create);
            Assert.AreEqual(3, (int)FileMode.Open);
            Assert.AreEqual(4, (int)FileMode.OpenOrCreate);
            Assert.AreEqual(5, (int)FileMode.Truncate);
            Assert.AreEqual(6, (int)FileMode.Append);
        }
    }
}
