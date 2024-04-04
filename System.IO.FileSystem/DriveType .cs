//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

namespace System.IO
{
    /// <summary>
    /// Defines constants for drive types, including CDRom, Fixed, Network, NoRootDirectory, Ram, Removable, and Unknown.
    /// </summary>
    public enum DriveType
    {
        /// <summary>
        /// The type of drive is unknown.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// The drive does not have a root directory.
        /// </summary>
        NoRootDirectory = 1,

        /// <summary>
        /// The drive is a removable storage device, such as a USB flash drive or SD Card.
        /// </summary>
        Removable = 2,

        /// <summary>
        /// The drive is fixed, such as internal flash or eeprom.
        /// </summary>
        Fixed = 3,

        /// <summary>
        /// The drive is a RAM disk.
        /// </summary>
        Ram = 4
    }
}
