//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using nanoFramework.TestFramework;
using System.Text;
using System.Threading;

namespace System.IO.FileSystem.UnitTests
{
    [TestClass]
    public class FileUnitTests : FileSystemUnitTestsBase
    {
        private static readonly string Destination = $"{Root}{nameof(FileUnitTests)}-Destination.test";
        private static readonly string Source = $"{Root}{nameof(FileUnitTests)}-Source.test";

        private static readonly byte[] BinaryContent = Encoding.UTF8.GetBytes(TextContent);
        private static readonly byte[] EmptyContent = new byte[0];
        private const string TextContent = "Lorem ipsum dolor sit amet, consectetur adipiscing elit.";

        [Setup]
        public void Setup()
        {
            Assert.SkipTest("These test will only run on real hardware. Comment out this line if you are testing on real hardware.");

            //////////////////////////////////////////////////////////////////
            // these are needed when running the tests on a removable drive //
            //////////////////////////////////////////////////////////////////
            if (_waitForRemovableDrive)
            {
                // wait until all removable drives are mounted
                while (DriveInfo.GetDrives().Length < _numberOfDrives)
                {
                    Thread.Sleep(1000);
                }
            }
            //////////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////////////////
        }

        #region Test helpers

        private static void AssertContentEquals(string path, byte[] expected)
        {
            var buffer = File.ReadAllBytes(path);

            AssertContentEquals(
                buffer,
                expected);
        }
        private static void AssertContentEquals(Stream stream, byte[] expected)
        {
            var buffer = new byte[expected.Length];

            stream.Read(
                buffer,
                0,
                buffer.Length);

            AssertContentEquals(
                buffer,
                expected);
        }

        private static void AssertContentEquals(byte[] content, byte[] expected)
        {
            Assert.AreEqual(
                expected.Length,
                content.Length,
                "File content has wrong length.");

            for (var i = 0; i < content.Length; i++)
            {
                Assert.AreEqual(
                    expected[i],
                    content[i],
                    "File does not contain the expected data.");
            }
        }

        private void AssertContentEquals(string path, string content)
        {
            AssertContentEquals(
                path,
                Encoding.UTF8.GetBytes(content));
        }

        private static void AssertFileDoesNotExist(string path)
        {
            Assert.IsFalse(
                File.Exists(path),
                $"'{path}' exists when it shouldn't.");
        }

        private static void AssertFileExists(string path)
        {
            Assert.IsTrue(
                File.Exists(path),
                $"'{path}' does not exist when it should.");
        }

        /// <summary>
        /// Creates a file and verifies that <paramref name="content"/> was written to it.
        /// </summary>
        private static void CreateFile(string path, byte[] content)
        {
            if (content is not null)
            {
                OutputHelper.WriteLine($"Creating file: {path}...");

                File.WriteAllBytes(
                    path,
                    content);

                AssertFileExists(path);
                AssertContentEquals(path, content);
            }
        }

        /// <summary>
        /// Creates a file and verifies that <paramref name="content"/> was written to it.
        /// </summary>
        private static void CreateFile(string path, string content)
        {
            OutputHelper.WriteLine($"Creating file: {path}...");

            CreateFile(
                path,
                Encoding.UTF8.GetBytes(content));
        }

        private static void DeleteFile(string path)
        {
            OutputHelper.WriteLine($"Deleting file: {path}...");

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
            ExecuteTestAndTearDown(() =>
            {
                var content = BinaryContent;

                CreateFile(
                    Source,
                    content);

                File.Copy(
                    Source,
                    Destination);

                AssertContentEquals(
                    Source,
                    content);

                AssertContentEquals(
                    Destination,
                    content);
            });

            ExecuteTestAndTearDown(() =>
            {
                var content = BinaryContent;

                CreateFile(
                    Source,
                    content);

                File.Copy(
                    Source,
                    Destination,
                    overwrite: false);

                AssertContentEquals(
                    Source,
                    content);

                AssertContentEquals(
                    Destination,
                    content);
            });
        }

        [TestMethod]
        public void Copy_overwrites_destination()
        {
            ExecuteTestAndTearDown(() =>
            {
                var content = BinaryContent;

                CreateFile(
                    Source,
                    content);

                CreateFile(
                    Destination,
                    new byte[100]);

                File.Copy(
                    Source,
                    Destination,
                    overwrite: true);

                AssertContentEquals(
                    Source,
                    content);

                AssertContentEquals(
                    Destination,
                    content);
            });
        }

        [TestMethod]
        public void Copy_throws_if_destination_is_null_or_empty()
        {
            Assert.ThrowsException(typeof(ArgumentNullException), () =>
            {
                File.Copy(
                    Source,
                    null);
            });

            Assert.ThrowsException(typeof(ArgumentException), () =>
            {
                File.Copy(
                    Source,
                    string.Empty);
            });

            Assert.ThrowsException(typeof(ArgumentNullException), () =>
            {
                File.Copy(
                    Source,
                    null,
                    overwrite: false);
            });

            Assert.ThrowsException(typeof(ArgumentException), () =>
            {
                File.Copy(
                    Source,
                    string.Empty,
                    overwrite: false);
            });

            Assert.ThrowsException(typeof(ArgumentNullException), () =>
            {
                File.Copy(
                    Source,
                    null,
                    overwrite: true);
            });

            Assert.ThrowsException(typeof(ArgumentException), () =>
            {
                File.Copy(
                    Source,
                    string.Empty,
                    overwrite: true);
            });
        }

        [TestMethod]
        public void Copy_throws_if_destination_exists()
        {
            ExecuteTestAndTearDown(() =>
            {
                CreateFile(
                    Destination,
                    EmptyContent);

                Assert.ThrowsException(typeof(IOException), () =>
                {
                    File.Copy(
                        Source,
                        Destination);
                });

                Assert.ThrowsException(typeof(IOException), () =>
                {
                    File.Copy(
                        Source,
                        Destination,
                        overwrite: false);
                });
            });
        }

        [TestMethod]
        public void Copy_throws_if_source_is_null_or_empty()
        {
            Assert.ThrowsException(typeof(ArgumentNullException), () =>
            {
                File.Copy(
                    null,
                    Destination);
            });

            Assert.ThrowsException(typeof(ArgumentException), () =>
            {
                File.Copy(
                    string.Empty,
                    Destination);
            });

            Assert.ThrowsException(typeof(ArgumentNullException), () =>
            {
                File.Copy(
                    null,
                    Destination,
                    overwrite: false);
            });

            Assert.ThrowsException(typeof(ArgumentException), () =>
            {
                File.Copy(
                    string.Empty,
                    Destination,
                    overwrite: false);
            });

            Assert.ThrowsException(typeof(ArgumentNullException), () =>
            {
                File.Copy(
                    null,
                    Destination,
                    overwrite: true);
            });

            Assert.ThrowsException(typeof(ArgumentException), () =>
            {
                File.Copy(
                    string.Empty,
                    Destination,
                    overwrite: true);
            });
        }

        [TestMethod]
        public void Create_creates_file()
        {
            ExecuteTestAndTearDown(() =>
            {
                OutputHelper.WriteLine($"Creating file {Destination} WITHOUT content...");
                using var stream = File.Create(Destination);

                Console.WriteLine("Checking it file exists...");
                AssertFileExists(Destination);

                Console.WriteLine("Checking file content...");
                AssertContentEquals(
                    stream,
                    EmptyContent);
            });

            ExecuteTestAndTearDown(() =>
            {
                OutputHelper.WriteLine($"Creating file: {Destination} WITH content...");
                CreateFile(
                    Destination,
                    new byte[100]);

                Console.WriteLine("Creating file and truncating it...");
                using var stream = File.Create(Destination);

                Console.WriteLine("Checking it file exists...");
                AssertFileExists(Destination);

                Console.WriteLine("Checking file content...");
                AssertContentEquals(
                    stream,
                    EmptyContent);
            });
        }

        [TestMethod]
        public void Create_creates_multiple_files()
        {
            var testFileNames = new[]
            {
                $"{Root}{Guid.NewGuid()}.tmp",
                $"{Root}{Guid.NewGuid()}.tmp",
                $"{Root}{Guid.NewGuid()}.tmp",
                $"{Root}{Guid.NewGuid()}.tmp",
                $"{Root}{Guid.NewGuid()}.tmp",
                $"{Root}{Guid.NewGuid()}.tmp",
            };

            var fileContent = new[]
       {
                $"{Guid.NewGuid()}{Guid.NewGuid()}{Guid.NewGuid()}{Guid.NewGuid()}",
                $"{Guid.NewGuid()}{Guid.NewGuid()}{Guid.NewGuid()}{Guid.NewGuid()}",
                $"{Guid.NewGuid()}{Guid.NewGuid()}{Guid.NewGuid()}{Guid.NewGuid()}",
                $"{Guid.NewGuid()}{Guid.NewGuid()}{Guid.NewGuid()}{Guid.NewGuid()}",
                $"{Guid.NewGuid()}{Guid.NewGuid()}{Guid.NewGuid()}{Guid.NewGuid()}",
                $"{Guid.NewGuid()}{Guid.NewGuid()}{Guid.NewGuid()}{Guid.NewGuid()}",
            };

            // delete files if they exist
            for (var i = 0; i < testFileNames.Length; i++)
            {
                var fileName = testFileNames[i];
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
            }

            // create files, assert they exist and have the right content
            for (var i = 0; i < testFileNames.Length; i++)
            {
                var fileName = testFileNames[i];
                var content = fileContent[i];

                OutputHelper.WriteLine($"Creating file: {fileName}...");
                File.WriteAllText(fileName, content);

                Console.WriteLine("Checking it file exists...");
                AssertFileExists(fileName);

                Console.WriteLine("Checking file content...");
                AssertContentEquals(
                    fileName,
                    content);
            }

            // delete files
            for (var i = 0; i < testFileNames.Length; i++)
            {
                var fileName = testFileNames[i];
                File.Delete(fileName);
            }
        }

        [TestMethod]
        public void Delete_deletes_file()
        {
            ExecuteTestAndTearDown(() =>
            {
                CreateFile(
                    Source,
                    BinaryContent);

                File.Delete(Source);

                AssertFileDoesNotExist(Source);
            });
        }

        [TestMethod]
        public void Exists_returns_false_if_file_does_not_exist()
        {
            Assert.IsFalse(File.Exists($@"{Root}file_does_not_exist-{nameof(FileUnitTests)}.pretty_sure"));
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
        public void Move_moves_to_destination()
        {
            ExecuteTestAndTearDown(() =>
            {
                var content = BinaryContent;

                CreateFile(Source, content);

                File.Move(Source, Destination);

                AssertFileDoesNotExist(Source);
                AssertContentEquals(Destination, content);
            });
        }

        [TestMethod]
        public void Move_throws_if_destination_exists()
        {
            ExecuteTestAndTearDown(() =>
            {
                CreateFile(Source, BinaryContent);
                CreateFile(Destination, BinaryContent);

                Assert.ThrowsException(
                    typeof(IOException),
                    () => File.Move(
                        Source,
                        Destination));
            });
        }

        [TestMethod]
        public void Move_throws_if_destination_is_null_or_empty()
        {
            Assert.ThrowsException(
                typeof(ArgumentNullException),
                () => File.Move(
                    Source,
                    null));

            Assert.ThrowsException(
                typeof(ArgumentException),
                () => File.Move(
                    Source,
                    string.Empty));
        }

        public void Move_throws_if_source_does_not_exist()
        {
            ExecuteTestAndTearDown(() =>
            {
                Assert.ThrowsException(
                    typeof(IOException),
                    () => File.Move(
                        Source,
                        Destination));
            });
        }

        [TestMethod]
        public void Move_throws_if_source_is_null_or_empty()
        {
            Assert.ThrowsException(
                typeof(ArgumentNullException),
                () => File.Move(
                    null,
                    Destination));

            Assert.ThrowsException(
                typeof(ArgumentException),
                () => File.Move(
                    string.Empty,
                    Destination));
        }

        [TestMethod]
        public void OpenRead_should_open_existing_file()
        {
            ExecuteTestAndTearDown(() =>
            {
                CreateFile(
                    Source,
                    BinaryContent);

                using var actual = File.OpenRead(Source);

                AssertContentEquals(
                    actual,
                    BinaryContent);
            });
        }

        [TestMethod]
        public void OpenRead_should_throw_if_file_does_not_exist()
        {
            ExecuteTestAndTearDown(() =>
            {
                Assert.ThrowsException(
                    typeof(IOException),
                    () => { _ = File.OpenRead(Source); });
            });
        }

        [TestMethod]
        public void OpenText_should_open_existing_file()
        {
            ExecuteTestAndTearDown(() =>
            {
                CreateFile(
                    Source,
                    TextContent);

                using var actual = File.OpenText(Source);

                Assert.AreEqual(
                    TextContent,
                    actual.ReadToEnd());
            });
        }

        [TestMethod]
        public void OpenText_should_throw_if_file_does_not_exist()
        {
            ExecuteTestAndTearDown(() =>
            {
                Assert.ThrowsException(
                    typeof(IOException),
                    () => { _ = File.OpenText(Source); });
            });
        }

        [TestMethod]
        public void ReadAllBytes_should_read_all_content_from_file()
        {
            ExecuteTestAndTearDown(() =>
            {
                CreateFile(
                    Source,
                    BinaryContent);

                var actual = File.ReadAllBytes(Source);

                AssertContentEquals(
                    Source,
                    actual);
            });
        }

        [TestMethod]
        public void ReadAllBytes_should_throw_if_file_does_not_exist()
        {
            ExecuteTestAndTearDown(() =>
            {
                Assert.ThrowsException(
                    typeof(IOException),
                    () => { _ = File.ReadAllBytes(Source); });
            });
        }

        [TestMethod]
        public void ReadAllText_should_read_all_content_from_file()
        {
            ExecuteTestAndTearDown(() =>
            {
                CreateFile(
                    Source,
                    TextContent);

                var actual = File.ReadAllText(Source);

                Assert.AreEqual(
                    TextContent,
                    actual);
            });
        }

        [TestMethod]
        public void ReadAllText_should_throw_if_file_does_not_exist()
        {
            ExecuteTestAndTearDown(() =>
            {
                Assert.ThrowsException(
                    typeof(IOException),
                    () => { _ = File.ReadAllText(Source); });
            });
        }

        [TestMethod]
        public void SetAttributes_sets_FileAttributes()
        {
            ExecuteTestAndTearDown(() =>
            {
                CreateFile(
                    Source,
                    BinaryContent);

                File.SetAttributes(
                    Source,
                    FileAttributes.Hidden);

                var fileAttributes = File.GetAttributes(Source);

                Assert.IsTrue(
                    fileAttributes.HasFlag(FileAttributes.Hidden),
                    "File does not have hidden attribute");
            });
        }

        [TestMethod]
        public void SetAttributes_throws_if_file_does_not_exist()
        {
            ExecuteTestAndTearDown(() =>
            {
                Assert.ThrowsException(
                    typeof(IOException),
                    () =>
                    {
                        File.SetAttributes(
                        Source,
                        FileAttributes.Hidden);
                    });
            });
        }

        [TestMethod]
        public void WriteAllBytes_should_create_file()
        {
            ExecuteTestAndTearDown(() =>
            {
                File.WriteAllBytes(
                    Source,
                    EmptyContent);

                AssertFileExists(Source);
            });
        }

        [TestMethod]
        public void WriteAllBytes_should_overwrite_existing_file()
        {
            ExecuteTestAndTearDown(() =>
            {
                CreateFile(
                    Source,
                    new byte[100]);

                File.WriteAllBytes(
                    Source,
                    BinaryContent);

                AssertContentEquals(
                    Source,
                    BinaryContent);
            });
        }

        [TestMethod]
        public void WriteAllText_should_create_file()
        {
            ExecuteTestAndTearDown(() =>
            {
                File.WriteAllText(
                    Source,
                    TextContent);

                AssertFileExists(Source);
            });
        }


        [TestMethod]
        public void WriteAllTextLargeContent_should_create_file()
        {
            ExecuteTestAndTearDown(() =>
            {
                var largeContent = new StringBuilder();
                largeContent.Append(TextContent);
                largeContent.AppendLine(Guid.NewGuid().ToString());
                largeContent.Append(TextContent);
                largeContent.AppendLine(Guid.NewGuid().ToString());
                largeContent.Append(TextContent);
                largeContent.AppendLine(Guid.NewGuid().ToString());
                largeContent.Append(TextContent);
                largeContent.AppendLine(Guid.NewGuid().ToString());
                largeContent.Append(TextContent);
                largeContent.AppendLine(Guid.NewGuid().ToString());
                largeContent.Append(TextContent);
                largeContent.AppendLine(Guid.NewGuid().ToString());
                largeContent.Append(TextContent);

                File.WriteAllText(
                    Source,
                    largeContent.ToString());

                AssertFileExists(Source);

                AssertContentEquals(
                    Source,
                    largeContent.ToString());
            });
        }

        [TestMethod]
        public void WriteAllText_should_overwrite_existing_file()
        {
            ExecuteTestAndTearDown(() =>
            {
                CreateFile(
                    Source,
                    EmptyContent);

                File.WriteAllText(
                    Source,
                    TextContent);

                AssertContentEquals(
                    Source,
                    TextContent);
            });
        }
    }
}
