// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using nanoFramework.TestFramework;

namespace System.IO.FileSystem.Tests
{
    [TestClass]
    public static class FileModeTests
    {
        [TestMethod]
        public static void ValueTest()
        {
            Assert.Equal(1, (int)FileMode.CreateNew);
            Assert.Equal(2, (int)FileMode.Create);
            Assert.Equal(3, (int)FileMode.Open);
            Assert.Equal(4, (int)FileMode.OpenOrCreate);
            Assert.Equal(5, (int)FileMode.Truncate);
            Assert.Equal(6, (int)FileMode.Append);
        }
    }
}
