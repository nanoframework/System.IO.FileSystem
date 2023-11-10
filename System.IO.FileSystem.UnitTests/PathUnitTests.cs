using nanoFramework.TestFramework;

namespace System.IO.FileSystem.UnitTests
{
    [TestClass]
    internal class PathUnitTests
    {
        [TestMethod]
        public void ChangeExtension_adds_extension()
        {
            const string path = @"I:\file";
            const string expect = @"I:\file.new";

            Assert.AreEqual(expect, Path.ChangeExtension(path, "new"));
            Assert.AreEqual(expect, Path.ChangeExtension(path, ".new"));
        }

        [TestMethod]
        public void ChangeExtension_changes_extension()
        {
            const string path = @"I:\file.old";
            const string expect = @"I:\file.new";

            Assert.AreEqual(expect, Path.ChangeExtension(path, "new"));
            Assert.AreEqual(expect, Path.ChangeExtension(path, ".new"));
        }

        [TestMethod]
        public void ChangeExtension_removes_extension()
        {
            const string path = @"I:\file.old";
            const string expect = @"I:\file";

            Assert.AreEqual(expect, Path.ChangeExtension(path, null));
        }

        [TestMethod]
        public void ChangeExtension_returns_empty_string_if_path_is_empty_string()
        {
            Assert.AreEqual(string.Empty, Path.ChangeExtension(string.Empty, ".new"));
        }

        [TestMethod]
        public void ChangeExtension_returns_null_if_path_is_null()
        {
            Assert.IsNull(Path.ChangeExtension(null, ".new"));
        }

        [TestMethod]
        public void Combine_returns_path1_if_path2_is_empty_string()
        {
            var path1 = "path1";
            var path2 = string.Empty;

            var actual = Path.Combine(path1, path2);

            Assert.AreEqual(actual, path1);
        }

        [TestMethod]
        public void Combine_combines_paths()
        {
            var expect = @"I:\Path1\Path2\File.ext";

            Assert.AreEqual(expect, Path.Combine(@"I:\Path1", @"Path2\File.ext"));
            Assert.AreEqual(expect, Path.Combine(@"I:\Path1\", @"Path2\File.ext"));
        }

        [TestMethod]
        public void Combine_returns_path2_if_it_is_an_absolute_path()
        {
            var path1 = @"I:\Directory";
            var path2 = @"I:\Absolute\Path";

            var actual = Path.Combine(path1, path2);

            Assert.AreEqual(actual, path2);
        }

        [TestMethod]
        public void Combine_returns_path2_if_path1_is_empty_string()
        {
            var path1 = string.Empty;
            var path2 = "path2";

            var actual = Path.Combine(path1, path2);

            Assert.AreEqual(actual, path2);
        }

        [TestMethod]
        public void Combine_throws_if_path1_is_null()
        {
            Assert.ThrowsException(typeof(ArgumentNullException), () => { Path.Combine(null, "File.ext"); });
        }

        [TestMethod]
        public void Combine_throws_if_path2_is_null()
        {
            Assert.ThrowsException(typeof(ArgumentNullException), () => { Path.Combine(@"I:\Directory", null); });
        }

        [TestMethod]
        public void GetFilename_returns_empty_string()
        {
            Assert.AreEqual(string.Empty, Path.GetFileName("I:"));
            Assert.AreEqual(string.Empty, Path.GetFileName(@"I:\"));
        }

        [TestMethod]
        public void GetFilename_returns_filename_without_extension()
        {
            Assert.AreEqual("file", Path.GetFileName(@"\\server\share\directory\file"));
            Assert.AreEqual("file.ext", Path.GetFileName(@"\\server\share\directory\file.ext"));
            Assert.AreEqual("file", Path.GetFileName(@"I:\directory\file"));
            Assert.AreEqual("file.ext", Path.GetFileName(@"I:\directory\file.ext"));
            Assert.AreEqual("file", Path.GetFileName(@"I:\file"));
            Assert.AreEqual("file.ext", Path.GetFileName(@"I:\file.ext"));
        }

        [TestMethod]
        public void GetFilename_returns_null()
        {
            Assert.IsNull(Path.GetFileName(null));
        }

        [TestMethod]
        public void GetFilenameWithoutExtension_returns_empty_string()
        {
            Assert.AreEqual(string.Empty, Path.GetFileNameWithoutExtension("I:"));
            Assert.AreEqual(string.Empty, Path.GetFileNameWithoutExtension(@"I:\"));
        }

        [TestMethod]
        public void GetFilenameWithoutExtension_returns_filename_without_extension()
        {
            Assert.AreEqual("file", Path.GetFileNameWithoutExtension(@"\\server\share\directory\file"));
            Assert.AreEqual("file", Path.GetFileNameWithoutExtension(@"\\server\share\directory\file.ext"));
            Assert.AreEqual("file", Path.GetFileNameWithoutExtension(@"I:\directory\file"));
            Assert.AreEqual("file", Path.GetFileNameWithoutExtension(@"I:\directory\file.ext"));
            Assert.AreEqual("file", Path.GetFileNameWithoutExtension(@"I:\file"));
            Assert.AreEqual("file", Path.GetFileNameWithoutExtension(@"I:\file.ext"));
        }

        [TestMethod]
        public void GetFilenameWithoutExtension_returns_null()
        {
            Assert.IsNull(Path.GetFileNameWithoutExtension(null));
        }

        [TestMethod]
        public void GetPathRoot_returns_empty_string()
        {
            Assert.AreEqual(string.Empty, Path.GetPathRoot(@"directory\file"));
            Assert.AreEqual(string.Empty, Path.GetPathRoot(@"directory\file.ext"));
            Assert.AreEqual(string.Empty, Path.GetPathRoot("file"));
            Assert.AreEqual(string.Empty, Path.GetPathRoot("file.ext"));
        }

        [TestMethod]
        public void GetPathRoot_returns_null()
        {
            Assert.IsNull(Path.GetPathRoot(null));
            Assert.IsNull(Path.GetPathRoot(" "));
        }

        [TestMethod]
        public void GetPathRoot_returns_root()
        {
            Assert.AreEqual(@"\\server\share", Path.GetPathRoot(@"\\server\share\directory\file"));
            Assert.AreEqual(@"\\server\share", Path.GetPathRoot(@"\\server\share\directory\file.ext"));
            Assert.AreEqual("I:", Path.GetPathRoot("I:"));
            Assert.AreEqual(@"I:\", Path.GetPathRoot(@"I:\directory\file"));
            Assert.AreEqual(@"I:\", Path.GetPathRoot(@"I:\directory\file.ext"));
            Assert.AreEqual(@"I:\", Path.GetPathRoot(@"I:\file"));
            Assert.AreEqual(@"I:\", Path.GetPathRoot(@"I:\file.ext"));
        }

        [TestMethod]
        public void HasExtension_returns_false()
        {
            Assert.IsFalse(Path.HasExtension("file"), "file");
            Assert.IsFalse(Path.HasExtension("file."), "file.");
            Assert.IsFalse(Path.HasExtension(@"\"), @"\");
            Assert.IsFalse(Path.HasExtension("/"), "/");
            Assert.IsFalse(Path.HasExtension("I:"), "I:");
            Assert.IsFalse(Path.HasExtension(@"I:\"), @"I:\");
        }

        [TestMethod]
        public void HasExtension_returns_true()
        {
            Assert.IsTrue(Path.HasExtension("file.ext"), "file.ext");
            Assert.IsTrue(Path.HasExtension(@"\file.ext"), @"\file.ext");
            Assert.IsTrue(Path.HasExtension("/file.ext"), "/file.ext");
            Assert.IsTrue(Path.HasExtension("I:file.ext"), "I:file.ext");
            Assert.IsTrue(Path.HasExtension(@"I:\file.ext"), @"I:\file.ext");
        }

        [TestMethod]
        public void IsPathRooted_returns_true()
        {
            Assert.IsTrue(Path.IsPathRooted(@"\"));
            Assert.IsTrue(Path.IsPathRooted("/"));
            Assert.IsTrue(Path.IsPathRooted("I:"));
            Assert.IsTrue(Path.IsPathRooted(@"I:\"));
            Assert.IsTrue(Path.IsPathRooted(@"I:\file.ext"));
        }
    }
}
