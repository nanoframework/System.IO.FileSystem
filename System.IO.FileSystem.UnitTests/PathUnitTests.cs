//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.

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

            Assert.AreEqual(path1, actual);
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

            Assert.AreEqual(path2, actual);
        }

        [TestMethod]
        public void Combine_returns_path2_if_path1_is_empty_string()
        {
            var path1 = string.Empty;
            var path2 = "path2";

            var actual = Path.Combine(path1, path2);

            Assert.AreEqual(path2, actual);
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
        public void GetDirectoryName_returns_directory()
        {
            var tests = new[] { @"I:\directory", @"I:\directory\", @"I:\directory\file.ext" };
            var answers = new[] { @"I:\", @"I:\directory", @"I:\directory" };

            for (var i = 0; i < tests.Length; i++)
            {
                var test = tests[i];
                var expected = answers[i];

                Assert.AreEqual(expected, Path.GetDirectoryName(test), $"Case: {test}");
            }
        }

        [TestMethod]
        public void GetDirectoryName_returns_directory_UNC_paths()
        {
            var tests = new[] { @"\\server\share\", @"\\server\share\file.ext" };
            var answers = new[] { @"\\server\share", @"\\server\share" };

            for (var i = 0; i < tests.Length; i++)
            {
                var test = tests[i];
                var expected = answers[i];

                Assert.AreEqual(expected, Path.GetDirectoryName(test), $"Case: {test}");
            }
        }

        [TestMethod]
        public void GetDirectoryName_returns_null()
        {
            Assert.IsNull(Path.GetDirectoryName(null), $"Case: 'null'");

            // TODO: Would like to add '(string) null' to these cases but for some reason this crashes. Investigate further and open defect 
            var tests = new[] { string.Empty, " ", @"\", "C:", @"C:\" };
            foreach (var test in tests)
            {
                var actual = Path.GetDirectoryName(test);
                var message = $"Actual: '{actual}'. Case: '{test}'";

                Assert.IsNull(Path.GetDirectoryName(test), message);
            }
        }

        [TestMethod]
        public void GetDirectoryName_returns_null_UNC_paths()
        {
            Assert.SkipTest("UNC paths are not supported in the default build");

            var tests = new[] { @"\\server\share" };
            foreach (var test in tests)
            {
                var actual = Path.GetDirectoryName(test);
                var message = $"Actual: '{actual}'. Case: '{test}'";

                Assert.IsNull(Path.GetDirectoryName(test), message);
            }
        }
        [TestMethod]
        public void GetExtension_returns_empty_string()
        {
            Assert.AreEqual(string.Empty, Path.GetExtension(string.Empty));
            Assert.AreEqual(string.Empty, Path.GetExtension("file"));
            Assert.AreEqual(string.Empty, Path.GetExtension("file."));
        }

        [TestMethod]
        public void GetExtension_returns_extension()
        {
            var file = "file.ext";
            var expect = ".ext";

            Assert.AreEqual(expect, Path.GetExtension(file));
            Assert.AreEqual(expect, Path.GetExtension($"I:{file}"));
            Assert.AreEqual(expect, Path.GetExtension(@$"I:\{file}"));
            Assert.AreEqual(expect, Path.GetExtension(@$"I:\directory\{file}"));
            Assert.AreEqual(expect, Path.GetExtension(@$"\{file}"));
        }

        [TestMethod]
        public void GetExtension_returns_extension_UNC_paths()
        {
            var file = "file.ext";
            var expect = ".ext";

            Assert.AreEqual(expect, Path.GetExtension(@$"\\server\share\{file}"));
            Assert.AreEqual(expect, Path.GetExtension(@$"\\server\share\directory\{file}"));
        }

        [TestMethod]
        public void GetExtension_returns_null()
        {
            Assert.IsNull(Path.GetExtension(null));
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
            Assert.AreEqual("file", Path.GetFileName(@"I:\directory\file"));
            Assert.AreEqual("file.ext", Path.GetFileName(@"I:\directory\file.ext"));
            Assert.AreEqual("file", Path.GetFileName(@"I:\file"));
            Assert.AreEqual("file.ext", Path.GetFileName(@"I:\file.ext"));
        }

        [TestMethod]
        public void GetFilename_returns_filename_without_extension_UNC_paths()
        {
            Assert.AreEqual("file", Path.GetFileName(@"\\server\share\directory\file"));
            Assert.AreEqual("file.ext", Path.GetFileName(@"\\server\share\directory\file.ext"));
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
            Assert.AreEqual("file", Path.GetFileNameWithoutExtension(@"I:\directory\file"));
            Assert.AreEqual("file", Path.GetFileNameWithoutExtension(@"I:\directory\file.ext"));
            Assert.AreEqual("file", Path.GetFileNameWithoutExtension(@"I:\file"));
            Assert.AreEqual("file", Path.GetFileNameWithoutExtension(@"I:\file.ext"));
        }

        [TestMethod]
        public void GetFilenameWithoutExtension_returns_filename_without_extension_UNC_paths()
        {
            Assert.AreEqual("file", Path.GetFileNameWithoutExtension(@"\\server\share\directory\file"));
            Assert.AreEqual("file", Path.GetFileNameWithoutExtension(@"\\server\share\directory\file.ext"));
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
            var tests = new[]
            {
                "I:", @"I:\directory\file", @"I:\directory\file.ext", @"I:\file", @"I:\file.ext"
            };

            var answers = new[] { "I:", @"I:\", @"I:\", @"I:\", @"I:\" };

            for (var i = 0; i < tests.Length; i++)
            {
                var test = tests[i];
                var expected = answers[i];

                Assert.AreEqual(expected, Path.GetPathRoot(test), $"Case: {test}");
            }
        }

        [TestMethod]
        public void GetPathRoot_returns_root_UNC_paths()
        {
            Assert.SkipTest("UNC paths are not supported in the default build");

            var tests = new[]
            {
                @"\\server\share\directory\file", @"\\server\share\directory\file.ext"
            };

            var answers = new[] { @"\\server\share", @"\\server\share" };

            for (var i = 0; i < tests.Length; i++)
            {
                var test = tests[i];
                var expected = answers[i];

                Assert.AreEqual(expected, Path.GetPathRoot(test), $"Case: {test}");
            }
        }

        [DataRow(@"\dir1\dir2\file.ext")]
        [DataRow(@"\dir1\dir2\file.ext")]
        [DataRow(@"\dir1\..\dir2\file.ext")]
        [DataRow(@"\dir1\..\dir2\..\dir3\file.ext")]
        [TestMethod]
        public void GetFullPathWithFiles(string pathToTest)
        {
            string fullPath = Path.GetFullPath(pathToTest);
            Assert.AreEqual(pathToTest, fullPath);
        }

        [DataRow(@"\dir1\..\..\dir2\", @"\dir1\..\..\dir2\")]
        [DataRow(@"\dir1\..\..\dir2", @"\dir1\..\..\dir2")]
        [DataRow(@"dir1\dir2\", @"dir1\dir2\")]
        [DataRow(@"dir1\dir2", @"dir1\dir2")]
        [DataRow(@"\dir1\dir2\", @"\dir1\dir2\")]
        [TestMethod]
        public void GetFullPathWithDirectories(string pathToTest, string expectedPath)
        {
            string fullPath = Path.GetFullPath(pathToTest);
            Assert.AreEqual(expectedPath, fullPath);
        }

        [TestMethod]
        public void HasExtension_returns_false()
        {
            var tests = new[]
            {
                "file", @"\file.", @"\", "/", "I:", @"I:\", @"I:\directory\"
            };

            for (var i = 0; i < tests.Length; i++)
            {
                var test = tests[i];
                Assert.IsFalse(Path.HasExtension(test), $"Case: {test}");
            }
        }

        [TestMethod]
        public void HasExtension_returns_false_UNC_paths()
        {
            var tests = new[]
            {
                @"\\server\share\file.", @"\\server\share\directory\file"
            };

            for (var i = 0; i < tests.Length; i++)
            {
                var test = tests[i];
                Assert.IsFalse(Path.HasExtension(test), $"Case: {test}");
            }
        }

        [TestMethod]
        public void HasExtension_returns_true()
        {
            var tests = new[]
            {
                "file.ext", @"\file.ext", "/file.ext", "I:file.ext", @"I:\file.ext", @"I:\directory\file.ext"
            };

            for (var i = 0; i < tests.Length; i++)
            {
                var test = tests[i];
                Assert.IsTrue(Path.HasExtension(test), $"Case: {test}");
            }
        }

        [TestMethod]
        public void HasExtension_returns_true_UNC_paths()
        {
            var tests = new[]
            {
                @"\\server\share\file.ext", @"\\server\share\directory\file.ext"
            };

            for (var i = 0; i < tests.Length; i++)
            {
                var test = tests[i];
                Assert.IsTrue(Path.HasExtension(test), $"Case: {test}");
            }
        }

        [TestMethod]
        public void IsPathRooted_returns_true()
        {
            var tests = new[]
            {
                @"\", "/", "I:", @"I:\", @"I:\file.ext", @"I:\directory\file.ext"
            };

            for (var i = 0; i < tests.Length; i++)
            {
                var test = tests[i];
                Assert.IsTrue(Path.IsPathRooted(test), $"Case: {test}");
            }
        }

        [TestMethod]
        public void IsPathRooted_returns_true_UNC_paths()
        {
            var tests = new[]
            {
                @"\\server\share", @"\\server\share\file.ext", @"\\server\share\directory\file.ext"
            };

            for (var i = 0; i < tests.Length; i++)
            {
                var test = tests[i];
                Assert.IsTrue(Path.IsPathRooted(test), $"Case: {test}");
            }
        }
    }
}
