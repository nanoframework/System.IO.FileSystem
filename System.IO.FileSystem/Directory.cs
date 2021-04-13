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
            return null;
        }

        public static bool CreateDirectory(string path)
        {
            CreateNative(path);
            return true;
        }

        public static void Delete(string path, bool recursive = false)
        {

        }

        public static bool Exist(string path)
        {
            return false;
        }

        public static string[] GetFiles(string path)
        {
            return null;
        }

        public static string[] GetDirectories(string path)
        {
            return null;
        }

        #endregion

        #region Stubs (Native Calls)

        [Diagnostics.DebuggerStepThrough]
        [Diagnostics.DebuggerHidden]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern bool ExistsNative(string path, string fileName);

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
        private static extern void GetFilesNative(string path);

        [Diagnostics.DebuggerStepThrough]
        [Diagnostics.DebuggerHidden]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void GetDirectoriesNative(string path);

        [Diagnostics.DebuggerStepThrough]
        [Diagnostics.DebuggerHidden]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void GetLogicalDrivesNative(string path);

        #endregion
    }
}
