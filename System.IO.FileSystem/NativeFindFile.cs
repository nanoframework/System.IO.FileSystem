//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.IO
{
    internal class NativeFindFile
    {
        // field is required for native interop
#pragma warning disable IDE0051
#pragma warning disable IDE0044
#pragma warning disable CS0169
        object _ff;
#pragma warning restore CS0169
#pragma warning restore IDE0044
#pragma warning restore IDE0051

        [MethodImpl(MethodImplOptions.InternalCall)]
        [DebuggerNonUserCode]
        public extern NativeFindFile(
            string path,
            string searchPattern);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [DebuggerNonUserCode]
        public extern NativeFileInfo GetNext();

        [MethodImpl(MethodImplOptions.InternalCall)]
        [DebuggerNonUserCode]
        public extern void Close();

        [MethodImpl(MethodImplOptions.InternalCall)]
        [DebuggerNonUserCode]
        public static extern NativeFileInfo GetFileInfo(string path);
    }
}
