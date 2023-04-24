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
            Assert.AreEqual(0x0040, (int)FileAttributes.Device);
            Assert.AreEqual(0x0080, (int)FileAttributes.Normal);
            Assert.AreEqual(0x0100, (int)FileAttributes.Temporary);
            Assert.AreEqual(0x0200, (int)FileAttributes.SparseFile);
            Assert.AreEqual(0x0400, (int)FileAttributes.ReparsePoint);
            Assert.AreEqual(0x0800, (int)FileAttributes.Compressed);
            Assert.AreEqual(0x1000, (int)FileAttributes.Offline);
            Assert.AreEqual(0x2000, (int)FileAttributes.NotContentIndexed);
            Assert.AreEqual(0x4000, (int)FileAttributes.Encrypted);
            Assert.AreEqual(0x8000, (int)FileAttributes.IntegrityStream);
            Assert.AreEqual(0x20000, (int)FileAttributes.NoScrubData);
        }

    }
}
