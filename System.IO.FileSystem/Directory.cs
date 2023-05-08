//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System.Runtime.CompilerServices;

namespace System.IO
{
    /// <summary>
    /// Class for managing directories
    /// </summary>
    public static class Directory
    {
        #region Static Methods

        /// <summary>
        /// Determines a list of available logical drives.
        /// </summary>
        /// <returns>A String[] of available storage drive letters."</returns>
        public static string[] GetLogicalDrives()
        {
            return GetLogicalDrivesNative();
        }

        /// <summary>
        /// Creates directory with the provided path.
        /// </summary>
        /// <param name="path">Path and name of the directory to create.</param>
        /// <exception cref="IOException">Path for creating the folder doesn't exist. This method does not create directories recursively.</exception>
        public static void CreateDirectory(string path)
        {
            CreateNative(path);
        }
        /// <summary>
        /// Deletes directory from storage.
        /// </summary>
        /// <param name="path">Path to the directory to be removed.</param>
        /// <param name="recursive">Parameter to be implemented.</param>
        /// <exception cref="IOException">This method will throw DirectoryNotEmpty exception if folder is not empty.</exception>
        public static void Delete(string path, bool recursive = false)
        {
            DeleteNative(path);
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
            return ExistsNative(path);
        }

        /// <summary>
        /// Moves directory from specified path to a new location.
        /// </summary>
        /// <param name="sourcePath">Name of directory to move. Absolute path.</param>
        /// <param name="destinationPath">New path and name for the directory.</param>
        /// <exception cref="Exception">Source directory not existing or destination folder already existing.</exception>
        public static void Move(string sourcePath, string destinationPath)
        {
            MoveNative(sourcePath, destinationPath);
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
            return GetFilesNative(path);
        }

        /// <summary>
        /// List directories from the specified folder.
        /// </summary>
        /// <param name="path">The specified folder.</param>
        /// <returns> 
        /// When this method completes successfully, it returns an array of absolute paths to the subfolders in the specified directory.
        /// </returns>
        /// <exception cref="IOException"> Logical drive or a directory under given path does not exist. </exception>
        public static string[] GetDirectories(string path)
        {
            return GetDirectoriesNative(path);
        }

        /// <summary>
        /// Determines the time of the last write/modification to directory under given path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns>Time of the last write/modification.</returns>
        /// <exception cref="IOException"> Logical drive or a directory under given path does not exist. </exception>
        public static DateTime GetLastWriteTime(string path)
        {
            return GetLastWriteTimeNative(path);
        }

        #endregion

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
