using System.Text;
using nanoFramework.TestFramework;

namespace System.IO.FileSystem.UnitTests
{
    [TestClass]
    public class FileUnitTests
    {
        private const string Root = @"I:\";
        private static readonly string Destination = $"{Root}{nameof(FileUnitTests)}-Destination.test";
        private static readonly string Source = $"{Root}{nameof(FileUnitTests)}-Source.test";

        private static readonly byte[] BinaryContent = Encoding.UTF8.GetBytes(TextContent);
        private static readonly byte[] EmptyContent = new byte[0];
        private const string TextContent = "Lorem ipsum dolor sit amet, consectetur adipiscing elit.";

        #region Test helpers
        private static void AssertContentEquals(string path, byte[] expected)
        {
            using var inputStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            AssertContentEquals(inputStream, expected);
        }

        private static void AssertContentEquals(Stream stream, byte[] expected)
        {
            Assert.AreEqual(expected!.Length, stream.Length, "File is not the correct length.");

            var content = new byte[stream.Length];
            stream.Read(content, 0, content.Length);

            for (var i = 0; i < content.Length; i++)
            {
                Assert.AreEqual(expected[i], content[i], "File does not contain the expected data.");
            }
        }

        private static void AssertFileDoesNotExist(string path)
        {
            Assert.IsFalse(File.Exists(path), $"'{path}' exists when it shouldn't.");
        }

        private static void AssertFileExists(string path)
        {
            Assert.IsTrue(File.Exists(path), $"'{path}' does not exist when it should.");
        }

        /// <summary>
        /// Creates a file and verifies that <paramref name="content"/> was written to it.
        /// </summary>
        private static void CreateFile(string path, byte[] content)
        {
            File.Create(path);

            if (content!.Length > 0)
            {
                using var outputStream = new FileStream(path, FileMode.Open, FileAccess.Write);
                outputStream.Write(content, 0, content.Length);
            }

            AssertFileExists(path);
            AssertContentEquals(path, content);
        }

        private static void DeleteFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            AssertFileDoesNotExist(path);
        }

        /// <summary>
        /// Deleted test files, executes <paramref name="action"/>, and deletes test files.
        /// </summary>
        private static void ExecuteTestAndTearDown(Action action)
        {
            try
            {
                DeleteFile(Destination);
                DeleteFile(Source);

                action();
            }
            finally
            {
                DeleteFile(Destination);
                DeleteFile(Source);
            }
        }
        #endregion

        [TestMethod]
        public void Copy_copies_to_destination()
        {
            var content = BinaryContent;

            ExecuteTestAndTearDown(() =>
            {
                CreateFile(Source, content);

                File.Copy(Source, Destination);

                AssertContentEquals(Source, content);
                AssertContentEquals(Destination, content);
            });

            ExecuteTestAndTearDown(() =>
            {
                CreateFile(Source, content);

                File.Copy(Source, Destination, overwrite: false);

                AssertContentEquals(Source, content);
                AssertContentEquals(Destination, content);
            });
        }

        [TestMethod]
        public void Copy_overwrites_destination()
        {
            var content = BinaryContent;

            ExecuteTestAndTearDown(() =>
            {
                CreateFile(Source, content);
                CreateFile(Destination, new byte[100]);

                File.Copy(Source, Destination, overwrite: true);

                AssertContentEquals(Source, content);
                AssertContentEquals(Destination, content);
            });
        }

        [TestMethod]
        public void Copy_throws_if_destFileName_is_null_or_empty()
        {
            Assert.ThrowsException(typeof(ArgumentException), () => File.Copy(Source, null));
            Assert.ThrowsException(typeof(ArgumentException), () => File.Copy(Source, string.Empty));
            Assert.ThrowsException(typeof(ArgumentException), () => File.Copy(Source, null, overwrite: false));
            Assert.ThrowsException(typeof(ArgumentException), () => File.Copy(Source, string.Empty, overwrite: false));
            Assert.ThrowsException(typeof(ArgumentException), () => File.Copy(Source, null, overwrite: true));
            Assert.ThrowsException(typeof(ArgumentException), () => File.Copy(Source, string.Empty, overwrite: true));
        }

        [TestMethod]
        public void Copy_throws_if_destFileName_equals_sourceFileName()
        {
            Assert.ThrowsException(typeof(ArgumentException), () => File.Copy(Source, Source));
            Assert.ThrowsException(typeof(ArgumentException), () => File.Copy(Source, Source, overwrite: false));
            Assert.ThrowsException(typeof(ArgumentException), () => File.Copy(Source, Source, overwrite: true));
        }

        [TestMethod]
        public void Copy_throws_if_destination_exists()
        {
            ExecuteTestAndTearDown(() =>
            {
                CreateFile(Destination, EmptyContent);

                Assert.ThrowsException(typeof(IOException), () => { File.Copy(Source, Destination); });
                Assert.ThrowsException(typeof(IOException), () => { File.Copy(Source, Destination, overwrite: false); });
            });
        }

        [TestMethod]
        public void Copy_throws_if_sourceFileName_is_null_or_empty()
        {
            Assert.ThrowsException(typeof(ArgumentException), () => File.Copy(null, Destination));
            Assert.ThrowsException(typeof(ArgumentException), () => File.Copy(string.Empty, Destination));
            Assert.ThrowsException(typeof(ArgumentException), () => File.Copy(null, Destination, overwrite: false));
            Assert.ThrowsException(typeof(ArgumentException), () => File.Copy(string.Empty, Destination, overwrite: false));
            Assert.ThrowsException(typeof(ArgumentException), () => File.Copy(null, Destination, overwrite: true));
            Assert.ThrowsException(typeof(ArgumentException), () => File.Copy(string.Empty, Destination, overwrite: true));
        }

        [TestMethod]
        public void Create_creates_file()
        {
            ExecuteTestAndTearDown(() =>
            {
                using var stream = File.Create(Destination);

                AssertFileExists(Destination);
                AssertContentEquals(stream, EmptyContent);
            });

            ExecuteTestAndTearDown(() =>
            {
                CreateFile(Destination, new byte[100]);

                using var stream = File.Create(Destination);

                AssertFileExists(Destination);
                AssertContentEquals(stream, EmptyContent);
            });
        }

        [TestMethod]
        public void Delete_deletes_file()
        {
            ExecuteTestAndTearDown(() =>
            {
                CreateFile(Source, BinaryContent);

                File.Delete(Source);

                AssertFileDoesNotExist(Source);
            });
        }

        [TestMethod]
        public void Exists_returns_false_if_file_does_not_exist()
        {
            Assert.IsFalse(File.Exists($@"I:\file_does_not_exist-{nameof(FileUnitTests)}.pretty_sure"));
        }

        [TestMethod]
        public void Exists_returns_true_if_file_exists()
        {
            ExecuteTestAndTearDown(() =>
            {
                CreateFile(Source, BinaryContent);

                Assert.IsTrue(File.Exists(Source));
            });
        }

        [TestMethod]
        public void GetAttributes_returns_FileAttributes()
        {
            ExecuteTestAndTearDown(() =>
            {
                CreateFile(Source, BinaryContent);

                var fileAttributes = File.GetAttributes(Source);

                // TODO: Set an attribute to a non-default value for better assertion?
                Assert.AreEqual(false, fileAttributes.HasFlag(FileAttributes.Directory), "File has directory attribute");
            });
        }

        [TestMethod]
        public void GetAttributes_throws_if_file_does_not_exist()
        {
            ExecuteTestAndTearDown(() =>
            {
                Assert.ThrowsException(typeof(IOException), () => { File.GetAttributes(Source); });
            });
        }

        [TestMethod]
        public void GetLastWriteTime_returns_DateTime()
        {
            ExecuteTestAndTearDown(() =>
            {
                CreateFile(Source, BinaryContent);

                var actual = File.GetLastWriteTime(Source);

                Assert.IsTrue(actual != default, "Failed to get last write time.");
            });
        }

        [TestMethod]
        public void GetLastWriteTime_throws_if_file_does_not_exist()
        {
            ExecuteTestAndTearDown(() =>
            {
                Assert.ThrowsException(typeof(IOException), () => { File.GetLastWriteTime(Source); });
            });
        }
    }
}
