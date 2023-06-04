//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

namespace System.IO
{
    /// <summary>
    /// Contains constants for controlling file sharing options while
    /// opening files. You can specify what access other processes trying
    /// to open the same file concurrently can have.
    /// </summary>
    [Flags]
    public enum FileShare
    {
        /// <summary>
        /// No sharing. Any request to open the file (by this process or another
        /// process) will fail until the file is closed.
        /// </summary>
        None = 0x00,

        /// <summary>
        /// Allows subsequent opening of the file for reading. If this flag is not
        /// specified, any request to open the file for reading (by this process or
        /// another process) will fail until the file is closed.
        /// </summary>
        Read = 0x01,

        /// <summary>
        /// Allows subsequent opening of the file for writing. If this flag is not
        /// specified, any request to open the file for writing (by this process or
        /// another process) will fail until the file is closed.
        /// </summary>
        Write = 0x02,

        /// <summary>
        /// Allows subsequent opening of the file for writing or reading. If this flag
        /// is not specified, any request to open the file for writing or reading (by
        /// this process or another process) will fail until the file is closed.
        /// </summary>
        ReadWrite = 0x03
    }
}
