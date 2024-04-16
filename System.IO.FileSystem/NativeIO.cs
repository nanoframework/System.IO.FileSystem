//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.IO
{
    internal static class NativeIO
    {
        internal const string FSRoot = @"\";

        ////////////////////////////////////////////////////////////////////////////////////////////////
        // !!! KEEP IN SYNC WITH src\System.IO.FileSystem\nf_sys_io_filesystem.h (in native code) !!! //
        ////////////////////////////////////////////////////////////////////////////////////////////////
        internal const uint EmptyAttribute = 0xFFFFFFFF;

        /////////////////////////////////////////////////////////////////////////////////////
        // !!! KEEP IN SYNC WITH src\PAL\Include\nanoPAL_FileSystem.h (in native code) !!! //
        /////////////////////////////////////////////////////////////////////////////////////
        internal const int FSMaxPathLength = 260 - 2;
        internal const int FSMaxFilenameLength = 256;
        internal const int FSNameMaxLength = 7 + 1;

        [DebuggerNonUserCode]
        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern void Delete(string path, bool recursive);

        [DebuggerNonUserCode]
        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern bool Move(
            string sourceFileName,
            string destFileName);

        [DebuggerNonUserCode]
        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern void CreateDirectory(string path);

        [DebuggerNonUserCode]
        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern uint GetAttributes(string path);

        [DebuggerNonUserCode]
        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern void SetAttributes(
            string path,
            uint attributes);
    }
}
