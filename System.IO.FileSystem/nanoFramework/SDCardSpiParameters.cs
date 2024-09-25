//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

namespace nanoFramework.System.IO.FileSystem
{
    /// <summary>
    /// Parameter used for creating a SPI card instance.
    /// </summary>
    public class SDCardSpiParameters
    {
        /// <summary>
        /// The slot index to mount. Some devices can have more then 1 SD card slot
        /// Defaults to 0.  
        /// Slot 0 will mount as drive D:\ , slot 1 = E:\  etc
        /// </summary>
        public uint slotIndex = 0;

        /// <summary>
        /// The SPI bus to use for SD Card.
        /// </summary>
        public uint spiBus;

        /// <summary>
        /// The chip select pin to use for SD Card.
        /// </summary>
        public uint chipSelectPin;
    };
}
