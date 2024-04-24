using System.Text;
using nanoFramework.TestFramework;

namespace System.IO.FileSystem.UnitTests
{
    [TestClass]
    public class DirectoryUnitTests
    {
        [Setup]
        public void Setup()
        {
            //Assert.SkipTest("These test will only run on real hardware. Comment out this line if you are testing on real hardware.");
        }

        private const string Root = @"I:";

        [TestMethod]
        public void TestCreateDirectory()
        {
            string path = @$"{Root}\temp\testdir";
            Directory.CreateDirectory(path);

            Assert.IsTrue(Directory.Exists(path));

            // Clean up after the test
            Directory.Delete(path);
        }

        [TestMethod]
        public void TestDeleteDirectory()
        {
            string path = @$"{Root}\temp\testdir";
            Directory.CreateDirectory(path);
            Directory.Delete(path);

            Assert.IsFalse(Directory.Exists(path));
        }

        [TestMethod]
        public void TestDeleteDirectoryRecursive()
        {
            string path = @$"{Root}\temp\testdir";
            Directory.CreateDirectory(path);
            File.Create($@"{path}\file1.txt").Close();
            File.Create($@"{path}\file2.txt").Close();
            Directory.CreateDirectory($@"{path}\subdir");
            File.Create($@"{path}\subdir\file3.txt").Close();
            File.Create($@"{path}\subdir\file4.txt").Close();

            Directory.Delete(path, true);

            Assert.IsFalse(Directory.Exists(path));
        }

        [TestMethod]
        public void TestEnumerateDirectories()
        {
            string path = @$"{Root}\temp\testdir";
            Directory.CreateDirectory(path);
            Directory.CreateDirectory($@"{path}\subdir1");
            Directory.CreateDirectory($@"{path}\subdir2");
            Directory.CreateDirectory($@"{path}\subdir3");

            var directories = Directory.GetDirectories(path);

            Assert.AreEqual(3, directories.Length);

            // Clean up after the test
            Directory.Delete(path, true);
        }

        [TestMethod]
        public void TestEnumerateFiles()
        {
            string path = @$"{Root}\temp\testdir";
            Directory.CreateDirectory(path);
            File.Create($@"{path}\file1.txt").Close();
            File.Create($@"{path}\file2.txt").Close();
            File.Create($@"{path}\file3.txt").Close();

            var files = Directory.GetFiles(path);

            Assert.AreEqual(3, files.Length);

            // Clean up after the test
            Directory.Delete(path, true);
        }

        [TestMethod]
        public void TestMoveDirectory()
        {
            string path = @$"{Root}\temp\testdir";
            Directory.CreateDirectory(path);
            File.Create($@"{path}\file1.txt").Close();
            File.Create($@"{path}\file2.txt").Close();
            File.Create($@"{path}\file3.txt").Close();

            string newPath = @$"{Root}\temp\testdir2";
            Directory.CreateDirectory(newPath);

            Directory.Move(path, newPath);

            Assert.IsFalse(Directory.Exists(path));
            Assert.IsTrue(Directory.Exists(newPath));

            // Clean up after the test
            Directory.Delete(newPath, true);
        }

        [TestMethod]
        public void TestMoveDirectoryWithFiles()
        {
            string path = @$"{Root}\temp\testdir";
            Directory.CreateDirectory(path);
            File.Create($@"{path}\file1.txt").Close();
            File.Create($@"{path}\file2.txt").Close();
            File.Create($@"{path}\file3.txt").Close();

            string newPath = @$"{Root}\temp\testdir2";
            Directory.CreateDirectory(newPath);

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
            string path = @$"{Root}\temp\testdir";
            Directory.CreateDirectory(path);
            File.Create($@"{path}\file1.txt").Close();
            File.Create($@"{path}\file2.txt").Close();
            File.Create($@"{path}\file3.txt").Close();
            Directory.CreateDirectory($@"{path}\subdir1");
            Directory.CreateDirectory($@"{path}\subdir2");
            Directory.CreateDirectory($@"{path}\subdir3");

            string newPath = @$"{Root}\temp\testdir2";
            Directory.CreateDirectory(newPath);

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
            string path = @$"{Root}\temp\testdir";
            Directory.CreateDirectory(path);
            File.Create($@"{path}\file1.txt").Close();
            File.Create($@"{path}\file2.txt").Close();
            File.Create($@"{path}\file3.txt").Close();
            Directory.CreateDirectory($@"{path}\subdir1");
            Directory.CreateDirectory($@"{path}\subdir2");
            Directory.CreateDirectory($@"{path}\subdir3");
            File.Create($@"{path}\subdir1\file1.txt").Close();
            File.Create($@"{path}\subdir1\file2.txt").Close();
            File.Create($@"{path}\subdir1\file3.txt").Close();
            File.Create($@"{path}\subdir2\file1.txt").Close();
            File.Create($@"{path}\subdir2\file2.txt").Close();
            File.Create($@"{path}\subdir2\file3.txt").Close();
            File.Create($@"{path}\subdir3\file1.txt").Close();
            File.Create($@"{path}\subdir3\file2.txt").Close();
            File.Create($@"{path}\subdir3\file3.txt").Close();

            string newPath = @$"{Root}\temp\testdir2";
            Directory.CreateDirectory(newPath);

            Directory.Move(path, newPath);

            Assert.IsFalse(Directory.Exists(path));
            Assert.IsTrue(Directory.Exists(newPath));
            Assert.AreEqual(3, Directory.GetFiles(newPath).Length);
            Assert.AreEqual(3, Directory.GetDirectories(newPath).Length);
            Assert.AreEqual(3, Directory.GetFiles($@"{newPath}\subdir1").Length);
            Assert.AreEqual(3, Directory.GetFiles($@"{newPath}\subdir2").Length);
            Assert.AreEqual(3, Directory.GetFiles($@"{newPath}\subdir3").Length);

            // Clean up after the test
            Directory.Delete(newPath, true);
        }

        [TestMethod]
        public void TestMoveDirectoryWithSubdirectoriesAndFilesAndOverwrite()
        {
            string path = @$"{Root}\temp\testdir";
            Directory.CreateDirectory(path);
            File.Create($@"{path}\file1.txt").Close();
            File.Create($@"{path}\file2.txt").Close();
            File.Create($@"{path}\file3.txt").Close();
            Directory.CreateDirectory($@"{path}\subdir1");
            Directory.CreateDirectory($@"{path}\subdir2");
            Directory.CreateDirectory($@"{path}\subdir3");
            File.Create($@"{path}\subdir1\file1.txt").Close();
            File.Create($@"{path}\subdir1\file2.txt").Close();
            File.Create($@"{path}\subdir1\file3.txt").Close();
            File.Create($@"{path}\subdir2\file1.txt").Close();
            File.Create($@"{path}\subdir2\file2.txt").Close();
            File.Create($@"{path}\subdir2\file3.txt").Close();
            File.Create($@"{path}\subdir3\file1.txt").Close();
            File.Create($@"{path}\subdir3\file2.txt").Close();
            File.Create($@"{path}\subdir3\file3.txt").Close();

            string newPath = @$"{Root}\temp\testdir2";
            Directory.CreateDirectory(newPath);
            File.Create($@"{newPath}\file1.txt").Close();
            File.Create($@"{newPath}\file2.txt").Close();
            File.Create($@"{newPath}\file3.txt").Close();
            Directory.CreateDirectory($@"{newPath}\subdir1");
            Directory.CreateDirectory($@"{newPath}\subdir2");
            Directory.CreateDirectory($@"{newPath}\subdir3");
            File.Create($@"{newPath}\subdir1\file1.txt").Close();
            File.Create($@"{newPath}\subdir1\file2.txt").Close();
            File.Create($@"{newPath}\subdir1\file3.txt").Close();
            File.Create($@"{newPath}\subdir2\file1.txt").Close();
            File.Create($@"{newPath}\subdir2\file2.txt").Close();
            File.Create($@"{newPath}\subdir2\file3.txt").Close();
            File.Create($@"{newPath}\subdir3\file1.txt").Close();
            File.Create($@"{newPath}\subdir3\file2.txt").Close();
            File.Create($@"{newPath}\subdir3\file3.txt").Close();

            Directory.Move(path, newPath);

            Assert.IsFalse(Directory.Exists(path));
            Assert.IsTrue(Directory.Exists(newPath));
            Assert.AreEqual(3, Directory.GetFiles(newPath).Length);
            Assert.AreEqual(3, Directory.GetDirectories(newPath).Length);
            Assert.AreEqual(3, Directory.GetFiles($@"{newPath}\subdir1").Length);
            Assert.AreEqual(3, Directory.GetFiles($@"{newPath}\subdir2").Length);
            Assert.AreEqual(3, Directory.GetFiles($@"{newPath}\subdir3").Length);

            // Clean up after the test
            Directory.Delete(newPath, true);
        }
        [TestMethod]
        public void TestGetDirectoryRoot()
        {
            string path = @$"{Root}\temp\testdir";
            var directoryInfo = Directory.CreateDirectory(path);

            Assert.AreEqual(Root, directoryInfo.Root);

            // Clean up after the test
            Directory.Delete(path);
        }

        [TestMethod]
        public void TestGetParentDirectory()
        {
            string path = @$"{Root}\temp\testdir";
            var directoryInfo = Directory.CreateDirectory(path);

            Assert.AreEqual(Root + @"\temp", directoryInfo.Parent);

            // Clean up after the test
            Directory.Delete(path);
        }

        [TestMethod]
        public void TestGetCurrentDirectory()
        {
            string currentDirectory = Directory.GetCurrentDirectory();

            Assert.AreEqual(Root, currentDirectory);
        }

        [TestMethod]
        public void TestSetCurrentDirectory()
        {
            string newCurrentDirectory = @$"{Root}\temp\testdir";
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
