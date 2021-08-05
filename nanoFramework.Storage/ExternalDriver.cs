using System;
using System.Runtime.CompilerServices;

// TODO: it is debatible whether this class should exist, and parts of it could be auto detected in the native initialization!
namespace nanoFramework.Storage
{
    /// <summary>
    /// Class to allow a single SDCard to be mounted on the system.
    /// Only allows for 1 device to be mounted, either via MMC or SPI
    /// </summary>
    public static class ExternalDriver //TODO: this was based on https://raw.githubusercontent.com/nanoframework/Windows.Storage/develop/Windows.Storage/StorageDevices.cs incase of reference.
    {

#pragma warning disable 0649

        // this field is set in native code
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private static bool _mounted = false;

#pragma warning restore 0649

        /// <summary>
        /// Indcates if the SDscard has been mounted
        /// </summary>
        public static bool IsMounted => _mounted;

        /// <summary>
        /// Mount the SDcard device on the MMC interface 
        /// </summary>
        /// <param name="Data1bit">If true denotes 1 bit data path will be used otherwise it will be 4 bits.</param>
        /// <remarks>
        /// This will try to mount the SDCard on the specified interface.
        /// If the Card is not present or the card is unable to be read then an exception will be thrown.
        /// </remarks>
        [System.Diagnostics.DebuggerStepThrough]
        public static void MountMMC(bool Data1bit)
        {
            MountMMCNative(Data1bit);
            _mounted = true;
        }

        /// <summary>
        /// Mount the SPI SDcard device on the specified SPI bus 
        /// </summary>
        /// <param name="SpiController">The name for the SPI device, i.e "SPI1" </param>
        /// <param name="ChipSelect">The GPIO pin used for chip select on SDcard.</param>
        /// <remarks>
        /// This will try to mount the SDCard on the specified interface.
        /// If the Card is not present or the card is unable to be read then an exception will be thrown.
        /// </remarks>
        [System.Diagnostics.DebuggerStepThrough]
        public static void MountSpi(string SpiController, int ChipSelect)
        {
            // the SpiDevice is an ASCII string with the format 'SPIn'
            // need to grab 'n' from the string and convert that to the integer value from the ASCII code (do this by subtracting 48 from the char value)
            int spiBus = SpiController[3] - '0';
            MountSpiNative(spiBus, ChipSelect);

            // If no exception then set mounted flag
            _mounted = true;
        }

        /// <summary>
        /// Unmount the mounted SDcard.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        public static void Unmount()
        {
            UnmountNative();
 
            // If no exception then set mounted flag
            _mounted = false;
        }

#region Native Calls

        [System.Diagnostics.DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern static void MountMMCNative(bool Data1bit);

        [System.Diagnostics.DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void MountSpiNative(int SpiBus, int ChipSelect);

        [System.Diagnostics.DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void UnmountNative();

#endregion

    }
}

