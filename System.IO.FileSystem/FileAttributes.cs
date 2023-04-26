//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

namespace System.IO
{
    /// <summary>
    /// Provides attributes for files and directories.
    /// </summary>
    [Flags]
    public enum FileAttributes
    {
        /// <summary>
        /// The file is read-only.
        /// </summary>
        ReadOnly = 0x0001,

        /// <summary>
        /// The file is hidden, and thus is not included in an ordinary directory listing.
        /// </summary>
        Hidden = 0x0002,

        /// <summary>
        /// The file is a system file.
        /// That is, the file is part of the operating system or is used exclusively by the operating system.
        /// </summary>
        System = 0x0004,

        /// <summary>
        /// The file is a directory.
        /// </summary>
        Directory = 0x0010,

        /// <summary>
        /// This file is marked to be included in incremental backup operation.
        /// </summary>
        Archive = 0x0020
    }
}
