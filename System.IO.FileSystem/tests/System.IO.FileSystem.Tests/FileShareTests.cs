// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using nanoFramework.TestFramework;

namespace System.IO.FileSystem.Tests
{
    [TestClass]
    public static class FileShareTests
    {
        [TestMethod]
        public static void ValueTest()
        {
            Assert.Equal(0, (int)FileShare.None);
            Assert.Equal(1, (int)FileShare.Read);
            Assert.Equal(2, (int)FileShare.Write);
            Assert.Equal(3, (int)FileShare.ReadWrite);
            Assert.Equal(4, (int)FileShare.Delete);
            Assert.Equal(0x10, (int)FileShare.Inheritable);
        }
    }
}
