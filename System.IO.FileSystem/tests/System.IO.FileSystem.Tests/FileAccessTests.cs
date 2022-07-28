// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using nanoFramework.TestFramework;
using System.IO;
using System.IO.FileSystem;

namespace System.IO.FileSystem.Tests
{
    [TestClass]
    public static class FileAccessTests
    {
        [TestMethod]
        public static void ValueTest()
        {
            Assert.Equal(1, (int)FileAccess.Read);
            Assert.Equal(2, (int)FileAccess.Write);
            Assert.Equal(3, (int)FileAccess.ReadWrite);
        }
    }
}
