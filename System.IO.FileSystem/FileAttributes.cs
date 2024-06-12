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
        ReadOnly = 0x1,

        /// <summary>
        /// The file is hidden, and thus is not included in an ordinary directory listing.
        /// </summary>
        Hidden = 0x2,

        /// <summary>
        /// The file is a system file. That is, the file is part of the operating system or is used exclusively by the operating system.
        /// </summary>
        System = 0x4,

        /// <summary>
        /// The file is a directory.
        /// </summary>
        Directory = 0x10,

        /// <summary>
        /// This file is marked to be included in incremental backup operation.
        /// </summary>
        Archive = 0x20,

        /// <summary>
        /// The file is a standard file that has no special attributes. This attribute is valid only if it is used alone.
        /// </summary>
        Normal = 0x80,
    }
}
