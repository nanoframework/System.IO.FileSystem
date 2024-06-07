//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using nanoFramework.TestFramework;

namespace System.IO.FileSystem.UnitTests
{
    [TestClass]
    public class FileSystemUnitTestsBase
    {
        internal const int _numberOfDrives = 4;
        internal const string Root = @"D:\";

        // set to true to wait for removable drive(s) to be mounted
        internal const bool _waitForRemovableDrive = true;
    }
}
