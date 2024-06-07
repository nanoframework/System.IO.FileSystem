//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using nanoFramework.TestFramework;
using System.Threading;

namespace System.IO.FileSystem.UnitTests
{
    [TestClass]
    public class DirectoryUnitTests : FileSystemUnitTestsBase
    {
        [Setup]
        public void Setup()
        {
            //Assert.SkipTest("These test will only run on real hardware. Comment out this line if you are testing on real hardware.");

            //////////////////////////////////////////////////////////////////
            // these are needed when running the tests on a removable drive //
            //////////////////////////////////////////////////////////////////
            if (_waitForRemovableDrive)
            {
                DriveInfo.MountRemovableVolumes();

                // wait until all removable drives are mounted
                while (DriveInfo.GetDrives().Length < _numberOfDrives)
                {
                    Thread.Sleep(1000);
                }
            }
            //////////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////////////////
        }

        [TestMethod]
        public void TestCreateDirectory()
        {
            string path = @$"{Root}temp\testdir\";

            // make sure the directory doesn't exist
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }

            Directory.CreateDirectory(path);

            Assert.IsTrue(Directory.Exists(path));

            // Clean up after the test
            Directory.Delete(path);
        }

        [TestMethod]
        public void TestDeleteDirectory()
        {
            string path = @$"{Root}temp\testdir\";

            // make sure the directory doesn't exist
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }

            Directory.CreateDirectory(path);
            Directory.Delete(path);

            Assert.IsFalse(Directory.Exists(path));
        }

        [TestMethod]
        public void TestDeleteDirectoryRecursive()
        {
            string path = @$"{Root}temp\testdir\";

            // make sure the directory doesn't exist
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }

            Directory.CreateDirectory(path);

            File.Create($@"{path}file1.txt").Close();
            File.Create($@"{path}file2.txt").Close();

            Directory.CreateDirectory($@"{path}subdir\");

            File.Create($@"{path}subdir\file3.txt").Close();
            File.Create($@"{path}subdir\file4.txt").Close();

            Directory.Delete(path, true);

            Assert.IsFalse(Directory.Exists(path));
        }

        [TestMethod]
        public void TestEnumerateDirectories()
        {
            string path = @$"{Root}temp\testdir\";

            // make sure the directory doesn't exist
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }

            Directory.CreateDirectory(path);
            Directory.CreateDirectory($@"{path}subdir1\");
            Directory.CreateDirectory($@"{path}subdir2\");
            Directory.CreateDirectory($@"{path}subdir3\");

            var directories = Directory.GetDirectories(path);

            Assert.AreEqual(3, directories.Length);

            // Clean up after the test
            Directory.Delete(path, true);
        }

        [TestMethod]
        public void TestEnumerateFiles()
        {
            string path = @$"{Root}temp\testdir\";

            // make sure the directory doesn't exist
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }

            Directory.CreateDirectory(path);

            File.Create($@"{path}file1.txt").Close();
            File.Create($@"{path}file2.txt").Close();
            File.Create($@"{path}file3.txt").Close();

            var files = Directory.GetFiles(path);

            Assert.AreEqual(3, files.Length);

            // Clean up after the test
            Directory.Delete(path, true);
        }

        [TestMethod]
        public void TestMoveDirectory()
        {
            string path = @$"{Root}temp\testdir\";
            string newPath = @$"{Root}temp\testdir2\";

            // make sure both directories doesn't exist
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
            if (Directory.Exists(newPath))
            {
                Directory.Delete(newPath, true);
            }

            // create the directory and some files
            Directory.CreateDirectory(path);

            File.Create($@"{path}file1.txt").Close();
            File.Create($@"{path}file2.txt").Close();
            File.Create($@"{path}file3.txt").Close();

            // perform the move
            Directory.Move(path, newPath);

            // check if the directory was moved
            Assert.IsFalse(Directory.Exists(path));
            // check if the directory exists in the new location
            Assert.IsTrue(Directory.Exists(newPath));
            // check if the files were moved
            Assert.AreEqual(3, Directory.GetFiles(newPath).Length);

            // Clean up after the test
            Directory.Delete(newPath, true);
        }

        [TestMethod]
        public void TestMoveDirectoryWithFiles()
        {
            string path = @$"{Root}temp\testdir\";

            // make sure the directory doesn't exist
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }

            Directory.CreateDirectory(path);

            File.Create($@"{path}file1.txt").Close();
            File.Create($@"{path}file2.txt").Close();
            File.Create($@"{path}file3.txt").Close();

            string newPath = @$"{Root}temp\testdir2\";

            Directory.Move(path, newPath);

            Assert.IsFalse(Directory.Exists(path));
            Assert.IsTrue(Directory.Exists(newPath));
            Assert.AreEqual(3, Directory.GetFiles(newPath).Length);

            // Clean up after the test
            Directory.Delete(newPath, true);
        }

        [TestMethod]
        public void TestMoveDirectoryWithSubdirectories()
        {
            string path = @$"{Root}temp\testdir\";

            // make sure the directory doesn't exist
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }

            Directory.CreateDirectory(path);

            File.Create($@"{path}file1.txt").Close();
            File.Create($@"{path}file2.txt").Close();
            File.Create($@"{path}file3.txt").Close();

            Directory.CreateDirectory($@"{path}subdir1\");
            Directory.CreateDirectory($@"{path}subdir2\");
            Directory.CreateDirectory($@"{path}subdir3\");

            string newPath = @$"{Root}temp\testdir2\";

            Directory.Move(path, newPath);

            Assert.IsFalse(Directory.Exists(path));
            Assert.IsTrue(Directory.Exists(newPath));
            Assert.AreEqual(3, Directory.GetFiles(newPath).Length);
            Assert.AreEqual(3, Directory.GetDirectories(newPath).Length);

            // Clean up after the test
            Directory.Delete(newPath, true);
        }

        [TestMethod]
        public void TestMoveDirectoryWithSubdirectoriesAndFiles()
        {
            string path = @$"{Root}temp\testdir\";

            // make sure the directory doesn't exist
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }

            Directory.CreateDirectory(path);

            File.Create($@"{path}file1.txt").Close();
            File.Create($@"{path}file2.txt").Close();
            File.Create($@"{path}file3.txt").Close();

            Directory.CreateDirectory($@"{path}subdir1\");
            Directory.CreateDirectory($@"{path}subdir2\");
            Directory.CreateDirectory($@"{path}subdir3\");

            File.Create($@"{path}subdir1\file1.txt").Close();
            File.Create($@"{path}subdir1\file2.txt").Close();
            File.Create($@"{path}subdir1\file3.txt").Close();
            File.Create($@"{path}subdir2\file1.txt").Close();
            File.Create($@"{path}subdir2\file2.txt").Close();
            File.Create($@"{path}subdir2\file3.txt").Close();
            File.Create($@"{path}subdir3\file1.txt").Close();
            File.Create($@"{path}subdir3\file2.txt").Close();
            File.Create($@"{path}subdir3\file3.txt").Close();

            string newPath = @$"{Root}temp\testdir2\";

            Directory.Move(path, newPath);

            Assert.IsFalse(Directory.Exists(path));
            Assert.IsTrue(Directory.Exists(newPath));
            Assert.AreEqual(3, Directory.GetFiles(newPath).Length);
            Assert.AreEqual(3, Directory.GetDirectories(newPath).Length);
            Assert.AreEqual(3, Directory.GetFiles($@"{newPath}subdir1").Length);
            Assert.AreEqual(3, Directory.GetFiles($@"{newPath}subdir2").Length);
            Assert.AreEqual(3, Directory.GetFiles($@"{newPath}subdir3").Length);

            // Clean up after the test
            Directory.Delete(newPath, true);
        }

        [TestMethod]
        public void TestMoveDirectoryWithSubdirectoriesAndFilesAndOverwrite()
        {
            string path = @$"{Root}temp\testdir\";
            string newPath = @$"{Root}temp\testdir2\";

            // make sure the directory doesn't exist
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }

            if (Directory.Exists(newPath))
            {
                Directory.Delete(newPath, true);
            }

            Directory.CreateDirectory(path);
            File.Create($@"{path}file1.txt").Close();
            File.Create($@"{path}file2.txt").Close();
            File.Create($@"{path}file3.txt").Close();
            Directory.CreateDirectory($@"{path}subdir1\");
            Directory.CreateDirectory($@"{path}subdir2\");
            Directory.CreateDirectory($@"{path}subdir3\");
            File.Create($@"{path}subdir1\file1.txt").Close();
            File.Create($@"{path}subdir1\file2.txt").Close();
            File.Create($@"{path}subdir1\file3.txt").Close();
            File.Create($@"{path}subdir2\file1.txt").Close();
            File.Create($@"{path}subdir2\file2.txt").Close();
            File.Create($@"{path}subdir2\file3.txt").Close();
            File.Create($@"{path}subdir3\file1.txt").Close();
            File.Create($@"{path}subdir3\file2.txt").Close();
            File.Create($@"{path}subdir3\file3.txt").Close();

            Directory.Move(path, newPath);

            Assert.IsFalse(Directory.Exists(path), "Origin path exists after move and it shoudln't.");
            Assert.IsTrue(Directory.Exists(newPath), "Destination doesn't exist and it should.");
            Assert.AreEqual(3, Directory.GetFiles(newPath).Length, $"Wrong file count @ {newPath}.");
            Assert.AreEqual(3, Directory.GetDirectories(newPath).Length, $"Wrong directory count @ {newPath}.");
            Assert.AreEqual(3, Directory.GetFiles($@"{newPath}subdir1").Length, $@"Wrong file count @ {newPath}\subdir1.");
            Assert.AreEqual(3, Directory.GetFiles($@"{newPath}subdir2").Length, $@"Wrong file count @ {newPath}\subdir2.");
            Assert.AreEqual(3, Directory.GetFiles($@"{newPath}subdir3").Length, $@"Wrong file count @ {newPath}\subdir3.");

            // Clean up after the test
            Directory.Delete(newPath, true);
        }

        [TestMethod]
        public void TestGetDirectoryRoot()
        {
            string path = @$"{Root}temp\testdir\";

            // make sure the directory doesn't exist
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }

            var directoryInfo = Directory.CreateDirectory(path);

            Assert.AreEqual(Root, directoryInfo.Root.ToString());

            // Clean up after the test
            Directory.Delete(path);
        }

        [TestMethod]
        public void TestGetParentDirectory()
        {
            string path = @$"{Root}temp\testdir\";

            // make sure the directory doesn't exist
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }

            var directoryInfo = Directory.CreateDirectory(path);

            Assert.AreEqual(Path.Combine(Root, "temp"), directoryInfo.Parent.ToString());

            // Clean up after the test
            Directory.Delete(path);
        }

        [TestMethod]
        public void TestGetCurrentDirectory()
        {
            string currentDirectory = Directory.GetCurrentDirectory();

            Assert.AreEqual(NativeIO.FSRoot, currentDirectory);
        }

        [TestMethod]
        public void TestSetCurrentDirectory()
        {
            string newCurrentDirectory = @$"{Root}temp\testdir\";

            // make sure the directory doesn't exist
            if (Directory.Exists(newCurrentDirectory))
            {
                Directory.Delete(newCurrentDirectory, true);
            }

            Directory.CreateDirectory(newCurrentDirectory);

            Directory.SetCurrentDirectory(newCurrentDirectory);

            string currentDirectory = Directory.GetCurrentDirectory();

            Assert.AreEqual(newCurrentDirectory, currentDirectory);

            // Clean up after the test
            Directory.SetCurrentDirectory(Root);
            Directory.Delete(newCurrentDirectory);
        }
    }
}
