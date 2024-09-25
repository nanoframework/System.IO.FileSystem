//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using static nanoFramework.System.IO.FileSystem.SDCard;

namespace nanoFramework.System.IO.FileSystem
{
    /// <summary>
    /// Parameter used for creating a MMC card instance.
    /// </summary>
    public class SDCardMmcParameters
    {
        /// <summary>
        /// The slot index to mount. Some devices can have more then 1 SD card slot
        /// Defaults to 0
        /// Slot 0 will mount as drive D:\ , slot 1 = E:\  etc
        /// </summary>
        public uint slotIndex = 0;

        /// <summary>
        /// Data width to use on MMC SD protocol.
        /// </summary>
        public SDDataWidth dataWidth;
    }
}
