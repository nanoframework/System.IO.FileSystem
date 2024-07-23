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
        [MethodImpl(MethodImplOptions.InternalCall)]
        [DebuggerNonUserCode]
        public static extern NativeFileInfo GetFileInfo(string path);
    }
}
