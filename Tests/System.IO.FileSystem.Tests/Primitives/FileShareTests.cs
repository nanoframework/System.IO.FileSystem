// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using nanoFramework.TestFramework;

namespace System.IO.FileSystem.Tests.Primitives
{
    [TestClass]
    public static class FileShareTests
    {
        [TestMethod]
        public static void ValueTest()
        {
            Assert.AreEqual(0, (int)FileShare.None);
            Assert.AreEqual(1, (int)FileShare.Read);
            Assert.AreEqual(2, (int)FileShare.Write);
            Assert.AreEqual(3, (int)FileShare.ReadWrite);
            Assert.AreEqual(4, (int)FileShare.Delete);
            Assert.AreEqual(0x10, (int)FileShare.Inheritable);
        }
    }
}
