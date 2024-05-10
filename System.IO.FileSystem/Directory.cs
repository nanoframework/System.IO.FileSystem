//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System.Collections;
using System.Runtime.CompilerServices;

namespace System.IO
{
    /// <summary>
    /// Exposes static methods for creating, moving, and enumerating through directories and subdirectories. This class cannot be inherited.
    /// </summary>
    public static class Directory
    {
        /// <summary>
        /// Creates all directories and subdirectories in the specified path unless they already exist.
        /// </summary>
        /// <param name="path">The directory to create.</param>
        /// <returns>An object that represents the directory at the specified path. This object is returned regardless of whether a directory at the specified path already exists.</returns>
        /// <exception cref="IOException"> The directory specified by path is a file.</exception>
        public static DirectoryInfo CreateDirectory(string path)
        {
            // path validation happening in the call
            path = Path.GetFullPath(path);

            // According to the .NET API, Directory.CreateDirectory on an existing directory returns the DirectoryInfo object for the existing directory.
            NativeIO.CreateDirectory(path);

            return new DirectoryInfo(path);
        }

        /// <summary>
        /// Deletes an empty directory from a specified path.
        /// </summary>
        /// <param name="path">The name of the empty directory to remove. This directory must be writable and empty.</param>
        /// <exception cref="IOException">The directory specified by path is not empty.</exception>
        public static void Delete(string path)
        {
            Delete(
                path,
                false);
        }

        /// <summary>
        /// Deletes the specified directory and, if indicated, any subdirectories and files in the directory.
        /// </summary>
        /// <param name="path">The name of the directory to remove.</param>
        /// <param name="recursive"><see langword="true"/> to remove directories, subdirectories, and files in path; otherwise, <see langword="false"/>.</param>
        /// <exception cref="IOException">A file with the same name and location specified by path exists. -or- The directory specified by path is read-only, or recursive is false and path is not an empty directory. -or- The directory is the application's current working directory. -or- The directory contains a read-only file. -or- The directory is being used by another process.</exception>
        /// <remarks>The platform or storage file system may not support recursive deletion of directories. The default value for recursive is <see langword="false"/>. In this case an exception will be thrown.</remarks>
        public static void Delete(
            string path,
            bool recursive)
        {
            // path validation happening in the call
            path = Path.GetFullPath(path);

            object record = FileSystemManager.LockDirectory(path);

            try
            {
                uint attributes = NativeIO.GetAttributes(path);

                if (attributes == NativeIO.EmptyAttribute)
                {
                    throw new IOException(
                        string.Empty,
                        (int)IOException.IOExceptionErrorCode.DirectoryNotFound);
                }

                if (((attributes & (uint)FileAttributes.Directory) == 0) ||
                    ((attributes & (uint)FileAttributes.ReadOnly) != 0))
                {
                    // it's readonly or not a directory
                    throw new IOException(
                        string.Empty,
                        (int)IOException.IOExceptionErrorCode.UnauthorizedAccess);
                }

                // make sure it is indeed a directory (and not a file)
                if (!Exists(path))
                {
                    throw new IOException(
                        string.Empty,
                        (int)IOException.IOExceptionErrorCode.DirectoryNotFound);
                }

                NativeIO.Delete(
                    path,
                    recursive);
            }
            finally
            {
                // regardless of what happened, we need to release the directory when we're done
                FileSystemManager.UnlockDirectory(record);
            }
        }

        /// <summary>
        /// Determines whether the given path refers to an existing directory on disk.
        /// </summary>
        /// <param name="path">The path to test.</param>
        /// <returns><see langword="true"/> if <paramref name="path"/> refers to an existing directory; <see langword="false"/> if the directory does not exist or an error occurs when trying to determine if the specified directory exists.</returns>
        /// <exception cref="ArgumentNullException">Path must be defined.</exception>
        /// <exception cref="IOException">Invalid drive or path to the parent folder doesn't exist.</exception>
        public static bool Exists(string path)
        {
            // path validation happening in the call
            path = Path.GetFullPath(path);

            // is this the absolute root? this always exists.
            if (path == NativeIO.FSRoot)
            {
                return true;
            }

            uint attributes = NativeIO.GetAttributes(path);

            if (attributes == NativeIO.EmptyAttribute)
            {
                // this means not found
                return false;
            }

            if ((((FileAttributes)attributes)
                 & FileAttributes.Directory) == FileAttributes.Directory)
            {
                // this is a directory
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the current working directory of the application.
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentDirectory()
        {
            return FileSystemManager.CurrentDirectory;
        }

        /// <summary>
        /// Returns the names of files (including their paths) in the specified directory.
        /// </summary>
        /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
        /// <returns>
        /// An array of the full names (including paths) for the files in the specified directory, or an empty array if no files are found. 
        /// </returns>
        /// <exception cref="IOException"> Logical drive or a directory under given path does not exist. </exception>
        public static string[] GetFiles(string path)
        {
            return GetChildren(
                path,
                "*",
                false);
        }

        /// <summary>
        /// Returns the names of subdirectories (including their paths) in the specified directory.
        /// </summary>
        /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
        /// <returns> 
        /// An array of the full names (including paths) of subdirectories in the specified path, or an empty array if no directories are found.
        /// </returns>
        /// <exception cref="IOException"> Logical drive or a directory under given path does not exist. </exception>
        public static string[] GetDirectories(string path)
        {
            return GetChildren(
                path,
                "*",
                true);
        }

        /// <summary>
        /// Moves a file or a directory and its contents to a new location.
        /// </summary>
        /// <param name="sourceDirName">The path of the file or directory to move.</param>
        /// <param name="destDirName">The path to the new location for <paramref name="sourceDirName"/> or its contents. If <paramref name="sourceDirName"/> is a file, then <paramref name="destDirName"/> must also be a file name.</param>
        /// <exception cref="IOException">n attempt was made to move a directory to a different volume. -or- destDirName already exists. See the note in the Remarks section. -or- The source directory does not exist. -or- The source or destination directory name is <see langword="null"/>. -or- The <paramref name="sourceDirName"/> and <paramref name="destDirName"/> parameters refer to the same file or directory. -or- The directory or a file within it is being used by another process.</exception>
        public static void Move(
            string sourceDirName,
            string destDirName)
        {
            // sourceDirName and destDirName validation happening in the call
            sourceDirName = Path.GetFullPath(sourceDirName);
            destDirName = Path.GetFullPath(destDirName);

            bool tryCopyAndDelete = false;
            object srcRecord = FileSystemManager.AddToOpenList(sourceDirName);

            try
            {
                // make sure is actually a directory
                if (!Exists(sourceDirName))
                {
                    throw new IOException(
                        string.Empty,
                        (int)IOException.IOExceptionErrorCode.DirectoryNotFound);
                }

                // If Move() returns false, we'll try doing copy and delete to accomplish the move
                tryCopyAndDelete = !NativeIO.Move(sourceDirName, destDirName);
            }
            finally
            {
                FileSystemManager.RemoveFromOpenList(srcRecord);
            }

            if (tryCopyAndDelete)
            {
                RecursiveCopyAndDelete(sourceDirName, destDirName);
            }
        }

        /// <summary>
        /// Sets the application's current working directory to the specified directory.
        /// </summary>
        /// <param name="path">The path to which the current working directory is set.</param>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        public static void SetCurrentDirectory(string path)
        {
            // path validation happening in the call
            path = Path.GetFullPath(path);

            // lock the directory for read-access first, to ensure path won't get deleted
            object record = FileSystemManager.AddToOpenListForRead(path);

            try
            {
                if (!Exists(path))
                {
                    throw new IOException(
                        string.Empty,
                        (int)IOException.IOExceptionErrorCode.DirectoryNotFound);
                }

                // put the lock on path. (also read-access)
                FileSystemManager.SetCurrentDirectory(path);
            }
            finally
            {
                // take lock off
                FileSystemManager.RemoveFromOpenList(record);
            }
        }

        private static string[] GetChildren(
            string path,
            string searchPattern,
            bool isDirectory)
        {
            // path and searchPattern validation happening in the call

            path = Path.GetFullPath(path);

            if (!Exists(path))
            {
                throw new IOException(
                    string.Empty,
                    (int)IOException.IOExceptionErrorCode.DirectoryNotFound);
            }

            ArrayList fileNames = new();

            object record = FileSystemManager.AddToOpenListForRead(path);

            NativeFindFile ff = null;

            try
            {
                ff = new NativeFindFile(path, searchPattern);

                uint targetAttribute = isDirectory ? (uint)FileAttributes.Directory : 0;

                NativeFileInfo fileinfo = ff.GetNext();

                while (fileinfo != null)
                {
                    if ((fileinfo.Attributes & (uint)FileAttributes.Directory) == targetAttribute)
                    {
                        fileNames.Add(fileinfo.FileName);
                    }

                    fileinfo = ff.GetNext();
                }
            }
            finally
            {
                ff?.Close();
                FileSystemManager.RemoveFromOpenList(record);
            }

            return (string[])fileNames.ToArray(typeof(string));
        }

        private static void RecursiveCopyAndDelete(string sourceDirName,
                                                   string destDirName)
        {
            string[] files;
            int filesCount, i;

            // relative path starts after the sourceDirName and a path seperator
            int relativePathIndex = sourceDirName.Length + 1; 
            
            // make sure no other thread/process can modify it (for example, delete the directory and create a file of the same name) while we're moving
            object recordSrc = FileSystemManager.AddToOpenList(sourceDirName);

            try
            {
                // check that ake sure sourceDir is actually a directory
                if (!Exists(sourceDirName))
                {
                    throw new IOException(
                        "",
                        (int)IOException.IOExceptionErrorCode.DirectoryNotFound);
                }

                // make sure destDir does not yet exist
                if (Exists(destDirName))
                {
                    throw new IOException(
                        "",
                        (int)IOException.IOExceptionErrorCode.PathAlreadyExists);
                }

                NativeIO.CreateDirectory(destDirName);

                files = GetFiles(sourceDirName);
                filesCount = files.Length;

                for (i = 0; i < filesCount; i++)
                {
                    File.Copy(
                        files[i],
                        Path.Combine(destDirName, files[i].Substring(relativePathIndex)),
                        false,
                        true);
                }

                files = GetDirectories(sourceDirName);
                filesCount = files.Length;

                for (i = 0; i < filesCount; i++)
                {
                    RecursiveCopyAndDelete(
                        files[i],
                        Path.Combine(destDirName, files[i].Substring(relativePathIndex)));
                }

                NativeIO.Delete(
                    sourceDirName,
                    true);
            }
            finally
            {
                FileSystemManager.RemoveFromOpenList(recordSrc);
            }
        }
    }
}
