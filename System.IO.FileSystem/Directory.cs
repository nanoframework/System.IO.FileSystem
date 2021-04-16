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

        public static string[] GetLogicalDrives()
        {
            return GetLogicalDrivesNative();
        }

        public static bool CreateDirectory(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException(nameof(path));
            }
            CreateNative(path);
            return true;
        }

        public static void Delete(string path, bool recursive = false)
        {
            DeleteNative(path);
        }

        public static bool Exist(string path)
        {
            return ExistsNative(path);
        }

        public static void Move(string sourcePath, string destinationPath)
        {
            MoveNative(sourcePath, destinationPath);
        }

        public static string[] GetFiles(string path)
        {
            return GetFilesNative(path);
        }

        public static string[] GetDirectories(string path)
        {
            return GetDirectoriesNative(path);
        }

        public static DateTime GetCreationTime(string path)
        {
            return GetCreationTimeNative(path);
        }

        public static DateTime GetLastAccessTime(string path)
        {
            return GetLastAccessTimeNative(path);
        }

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
        private static extern DateTime GetCreationTimeNative(string path);

        [Diagnostics.DebuggerStepThrough]
        [Diagnostics.DebuggerHidden]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern DateTime GetLastAccessTimeNative(string path);

        [Diagnostics.DebuggerStepThrough]
        [Diagnostics.DebuggerHidden]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern DateTime GetLastWriteTimeNative(string path);

        #endregion
    }
}
