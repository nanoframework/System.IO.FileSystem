//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System.Runtime.CompilerServices;

namespace System.IO
{
    internal class NativeFileStream
    {
        // field is required for native interop
#pragma warning disable IDE0051
#pragma warning disable CS0169
        object _fs;
#pragma warning restore CS0169
#pragma warning restore IDE0051

        public const int TimeoutDefault = 0;
        public const int BufferSizeDefault = 0;

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern NativeFileStream(
            string path,
            int bufferSize);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern int Read(
            byte[] buf,
            int offset,
            int count,
            int timeout);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern int Write(
            byte[] buf,
            int offset,
            int count,
            int timeout);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern long Seek(
            long offset,
            uint origin);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void Flush();

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern long GetLength();

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void SetLength(long length);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void GetStreamProperties(
            out bool canRead,
            out bool canWrite,
            out bool canSeek);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void Close();
    }
}
