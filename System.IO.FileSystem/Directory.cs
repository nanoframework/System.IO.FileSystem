//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System.Collections;
using System.Runtime.CompilerServices;

namespace System.IO
{
    /// <summary>
    /// Class for managing directories
    /// </summary>
    public static class Directory
    {
        /// <summary>
        /// Determines a list of available logical drives.
        /// </summary>
        /// <returns>String[] of available drives, ex. "D:\\"</returns>
        [Obsolete("Use DriveInfo.GetDrives() instead.")]
        public static string[] GetLogicalDrives()
        {
            return GetLogicalDrivesNative();
        }

        /// <summary>
        /// Creates all directories and subdirectories in the specified path unless they already exist.
        /// </summary>
        /// <param name="path">The directory to create.</param>
        /// <returns>An object that represents the directory at the specified path. This object is returned regardless of whether a directory at the specified path already exists.</returns>
        /// <exception cref="IOException"> The directory specified by path is a file.</exception>
        public static DirectoryInfo CreateDirectory(string path)
        {
            // path validation in Path.GetFullPath()
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
            Delete(path, false);
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
            // path validation in Path.GetFullPath()
            path = Path.GetFullPath(path);

            object record = FileSystemManager.LockDirectory(path);

            try
            {
                uint attributes = NativeIO.GetAttributes(path);

                if (attributes == 0xFFFFFFFF)
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
        /// Determines whether the specified directory exists.
        /// </summary>
        /// <param name="path">Path to the directory.</param>
        /// <returns>True if directory under given path exists, otherwise it returns false.</returns>
        /// <exception cref="ArgumentNullException">Path must be defined.</exception>
        /// <exception cref="IOException">Invalid drive or path to the parent folder doesn't exist.</exception>
        public static bool Exists(string path)
        {
            // path validation in Path.GetFullPath()
            path = Path.GetFullPath(path);

            // Is this the absolute root? this always exists.
            if (path == NativeIO.FSRoot)
            {
                return true;
            }
            else
            {
                try
                {
                    uint attributes = NativeIO.GetAttributes(path);

                    // This is essentially file not found.
                    if (attributes == 0xFFFFFFFF)
                    {
                        return false;
                    }

                    if ((((FileAttributes)attributes) & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        // this is a directory.
                        return true;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// List files from the specified folder.
        /// </summary>
        /// <param name="path">Path to the directory to list files from.</param>
        /// <returns>
        /// When this method completes successfully, it returns a array of paths of the files in the given folder. 
        /// </returns>
        /// <exception cref="IOException"> Logical drive or a directory under given path does not exist. </exception>
        public static string[] GetFiles(string path)
        {
            return GetChildren(path, "*", false);
        }

        /// <summary>
        /// List directories from the specified folder.
        /// </summary>
        /// <param name="path"></param>
        /// <returns> 
        /// When this method completes successfully, it returns an array of absolute paths to the subfolders in the specified directory.
        /// </returns>
        /// <exception cref="IOException"> Logical drive or a directory under given path does not exist. </exception>
        public static string[] GetDirectories(string path)
        {
            return GetChildren(path, "*", true);
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
        /// Moves directory from specified path to a new location.
        /// </summary>
        /// <param name="sourceDirName">The path of the file or directory to move.</param>
        /// <param name="destDirName">The path to the new location for <paramref name="sourceDirName"/> or its contents. If <paramref name="sourceDirName"/> is a file, then <paramref name="destDirName"/> must also be a file name.</param>
        /// <exception cref="IOException">n attempt was made to move a directory to a different volume. -or- destDirName already exists.See the note in the Remarks section. -or- The source directory does not exist. -or- The source or destination directory name is null. -or- The sourceDirName and destDirName parameters refer to the same file or directory. -or- The directory or a file within it is being used by another process.</exception>
        public static void Move(
            string sourceDirName,
            string destDirName)
        {
            // sourceDirName and destDirName validation in Path.GetFullPath()
            sourceDirName = Path.GetFullPath(sourceDirName);
            destDirName = Path.GetFullPath(destDirName);

            object srcRecord = FileSystemManager.AddToOpenList(sourceDirName);

            try
            {
                // Make sure sourceDir is actually a directory
                if (!Exists(sourceDirName))
                {
                    throw new IOException(
                        string.Empty,
                        (int)IOException.IOExceptionErrorCode.DirectoryNotFound);
                }

                NativeIO.Move(sourceDirName, destDirName);
            }
            finally
            {
                FileSystemManager.RemoveFromOpenList(srcRecord);
            }
        }

        /// <summary>
        /// Sets the application's current working directory to the specified directory.
        /// </summary>
        /// <param name="path">he path to which the current working directory is set.</param>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        public static void SetCurrentDirectory(string path)
        {
            // path validation in Path.GetFullPath()
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
            // path and searchPattern validation in Path.GetFullPath() and Path.NormalizePath()

            path = Path.GetFullPath(path);

            if (!Exists(path)) throw new IOException(
                "",
                (int)IOException.IOExceptionErrorCode.DirectoryNotFound);

            ArrayList fileNames = new ArrayList();

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

        #region Stubs (Native Calls)

        [Diagnostics.DebuggerStepThrough]
        [Diagnostics.DebuggerHidden]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern bool ExistsNative(string path);

        [Diagnostics.DebuggerStepThrough]
        [Diagnostics.DebuggerHidden]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void MoveNative(string pathSrc, string pathDest);

        [Diagnostics.DebuggerStepThrough]
        [Diagnostics.DebuggerHidden]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void DeleteNative(string path);

        [Diagnostics.DebuggerStepThrough]
        [Diagnostics.DebuggerHidden]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void CreateNative(string path);

        [Diagnostics.DebuggerStepThrough]
        [Diagnostics.DebuggerHidden]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern string[] GetFilesNative(string path);

        [Diagnostics.DebuggerStepThrough]
        [Diagnostics.DebuggerHidden]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern string[] GetDirectoriesNative(string path);

        [Diagnostics.DebuggerStepThrough]
        [Diagnostics.DebuggerHidden]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern string[] GetLogicalDrivesNative();

        [Diagnostics.DebuggerStepThrough]
        [Diagnostics.DebuggerHidden]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern DateTime GetLastWriteTimeNative(string path);

        #endregion
    }
}
