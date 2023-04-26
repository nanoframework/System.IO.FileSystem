// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using nanoFramework.TestFramework;

namespace System.IO.FileSystem.Tests.Primitives
{
    [TestClass]
    public static class FileAttributesTests
    {
        [TestMethod]
        public static void ValueTest()
        {
            Assert.AreEqual(0x0001, (int)FileAttributes.ReadOnly);
            Assert.AreEqual(0x0002, (int)FileAttributes.Hidden);
            Assert.AreEqual(0x0004, (int)FileAttributes.System);
            Assert.AreEqual(0x0010, (int)FileAttributes.Directory);
            Assert.AreEqual(0x0020, (int)FileAttributes.Archive);
        }

    }
}
