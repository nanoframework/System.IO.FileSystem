//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using Diagnostics = System.Diagnostics;
using System.Runtime.CompilerServices;

namespace nanoFramework.System.IO
{
    /// <summary>
    /// Class to allow a SD memory card to be mounted on the system.
    /// Only allows for one device to be mounted, either using SD or SPI protocols.
    /// </summary>
    /// <remarks>
    /// The system supports a single SD memory card.
    /// </remarks>
    public static class SDCard
    {

#pragma warning disable 0649

        // this field is set in native code
        [Diagnostics.DebuggerBrowsable(Diagnostics.DebuggerBrowsableState.Never)]
        private static bool _mounted = false;

#pragma warning restore 0649

        /// <summary>
        /// Indcates if the SDscard has been mounted
        /// </summary>
        public static bool IsMounted => _mounted;

        /// <summary>
        /// Mount the SD memory card device on MMC using SD protocol. 
        /// </summary>
        /// <param name="dataWidth">Setting for the SD protocol data width.</param>
        /// <remarks>
        /// This will try to mount the SD memory card on the specified interface.
        /// If the Card is not present or the card is unable to be read then an exception will be thrown.
        /// </remarks>
        public static void MountMMC(SDDataWidth dataWidth)
        {
            // upon return, and on successful execution, the native code has updated the _mounted field
            MountMMCNative(dataWidth);
        }

        /// <summary>
        /// Mount the SD memory card device on SPI bus. The SPI configuration it's fixed and part of the device configuration.
        /// </summary>
        /// <remarks>
        /// This requires that the SPI configuration is fixed in the device configurations.
        /// This will try to mount the SD memory card on the specified interface.
        /// If the Card is not present or the card is unable to be read then an exception will be thrown.
        /// </remarks>
        public static void MountSpi()
        {
            // upon return, and on successful execution, the native code has updated the _mounted field
            // using -1 (an "impossible" bus number) as SPI bus index to signal that the fixed configuration should be used
            MountSpiNative(-1, -1);
        }

        /// <summary>
        /// Mount the SD memory card device on the specified SPI bus.
        /// </summary>
        /// <param name="spiBus">The SPI Bus of the device</param>
        /// <param name="chipSelectPin">The GPIO pin used for chip select on the card electrical interface.</param>
        /// <remarks>
        /// This will try to mount the SD memory card on the specified interface.
        /// If the Card is not present or the card is unable to be read then an exception will be thrown.
        /// </remarks>
        public static void MountSpi(uint spiBus, uint chipSelectPin)
        {
            // upon return, and on successful execution, the native code has updated the _mounted field
            MountSpiNative((int)spiBus, (int)chipSelectPin);
        }

        /// <summary>
        /// Unmount a mounted SD memory card.
        /// </summary>
        public static void Unmount()
        {
            // upon return, and on successful execution, the native code has updated the _mounted field
            UnmountNative();
        }

        #region Native Calls

        [Diagnostics.DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern static void MountMMCNative(SDDataWidth dataWidth);

        [Diagnostics.DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void MountSpiNative(int SpiBus, int ChipSelect);

        [Diagnostics.DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void UnmountNative();

        #endregion

        /// <summary>
        /// Data width to use on MMC SD protocol.
        /// </summary>
        public enum SDDataWidth
        {
            /// <summary>
            /// 1-bit width.
            /// </summary>
            _1_bit = 1,

            /// <summary>
            /// 4-bit width.
            /// </summary>
            _4_bit = 2
        }
    }
}
