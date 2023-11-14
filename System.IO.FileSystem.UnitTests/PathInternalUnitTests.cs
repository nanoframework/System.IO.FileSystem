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
    }
}
