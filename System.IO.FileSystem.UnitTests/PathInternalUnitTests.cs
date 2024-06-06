using nanoFramework.TestFramework;

namespace System.IO.FileSystem.UnitTests
{
    [TestClass]
    public class PathInternalUnitTests
    {
        [TestMethod]
        public void IsValidDriveChar_returns_true()
        {
            var tests = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

            foreach (var test in tests)
            {
                Assert.IsTrue(PathInternal.IsValidDriveChar(test), $"Case: {test}");
            }
        }

        [TestMethod]
        [DataRow("folder1/folder2/folder3", "folder1\\folder2\\folder3", "Case: Forward slash")]
        [DataRow("folder1\\folder2\\folder3", "folder1\\folder2\\folder3", "Case: Back slash")]
        [DataRow("folder1/folder2\\folder3", "folder1\\folder2\\folder3", "Case: Mixed slashes")]
        [DataRow("folder1\\..\\folder2\\folder3", "folder2\\folder3", "Case: Navigation commands")]
        [DataRow("D:\\FileUnitTests-Destination.test", "D:\\FileUnitTests-Destination.test", "Case: Navigation commands in filename")]
        [DataRow("folder1/./folder2/folder3", "folder1\\folder2\\folder3", "Case: Current directory command")]
        [DataRow("folder1/../../folder2/folder3", "folder2\\folder3", "Case: Multiple navigation commands")]
        [DataRow("folder1/folder2/folder3/..", "folder1\\folder2", "Case: Navigation command at end")]
        [DataRow("folder1/folder2/..folder3", "folder1\\folder2\\..folder3", "Case: Navigation command in filename")]
        [DataRow("folder1/folder2/.../folder3", "folder1\\folder2\\...\\folder3", "Case: Triple dot in path")]
        [DataRow("D:\\", "D:\\", "Case: Handle root paths")]
        public void NormalizeDirectorySeparators_Returns_Correct_Path(string path, string expected, string caseName)
        {
            var actual = PathInternal.NormalizeDirectorySeparators(path);
            Assert.AreEqual(expected, actual, caseName);
        }
    }
}
