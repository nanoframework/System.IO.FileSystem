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

        // all these values are from FS_decl.h
        internal const int FSMaxPathLength = 260 - 2;
        internal const int FSMaxFilenameLength = 256;
        internal const int FSNameMaxLength = 7 + 1;

        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern void Format(
            string nameSpace,
            string fileSystem,
            string volumeLabel,
            uint parameter);

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
