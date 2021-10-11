//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System;
using System.Runtime.CompilerServices;
using Diagnostics = System.Diagnostics;

namespace nanoFramework.System.IO.FileSystem
{
    /// <summary>
    /// Class to allow a SD memory card to be configured and mounted on the system.
    /// </summary>
    public class SDCard : IDisposable
    {

#pragma warning disable 0649

        // this field is set in native code
        [Diagnostics.DebuggerBrowsable(Diagnostics.DebuggerBrowsableState.Never)]
        private bool _mounted = false;

#pragma warning restore 0649

        [Diagnostics.DebuggerBrowsable(Diagnostics.DebuggerBrowsableState.Never)]
        private bool _disposed;


        // Common parameters
        private SDInterfaceType _sdCardType;
        private bool _enableCardDetectPin;
        private uint _cardDetectPin;
        // MMC parameters
        private SDDataWidth _dataWidth;
        // SPI parameters
        private uint _spiBus;
        private uint _chipSelectPin;


        #region Properties
        /// <summary>
        /// Type of interface used by SDCard.
        /// </summary>
        public SDInterfaceType CardType => _sdCardType;

        /// <summary>
        /// Indicates if the SD card has been mounted
        /// </summary>
        public bool IsMounted => _mounted;

        /// <summary>
        /// Return true if Card detection is enabled
        /// </summary>
        public bool CardDetectEnabled => _enableCardDetectPin;

        /// <summary>
        /// The parameters for a MMC connected SD card.
        /// </summary>
        public SDCardMmcParameters MmcParameters { get => new SDCardMmcParameters { dataWidth=_dataWidth, enableCardDetectPin=_enableCardDetectPin, cardDetectPin=_cardDetectPin }; }

        /// <summary>
        /// The parameters for a SPI connected SD card. 
        /// </summary>
        public SDCardSpiParameters SpiParameters { get => new SDCardSpiParameters { spiBus=_spiBus, chipSelectPin=_chipSelectPin, enableCardDetectPin = _enableCardDetectPin, cardDetectPin = _cardDetectPin }; }

        /// <summary>
        /// Indicates if SD card has been detected if optional cardDetectPin parameter is enabled with a valid GPIO pin.
        /// If not enabled will always return false.
        /// </summary>
        /// <remarks>
        /// Not all SD Card modules have a card detect pin or the pin connected to a GPIO pin. 
        /// </remarks>
        public bool IsCardDetected
        {
            get
            {
                if (_enableCardDetectPin)
                {
                    return PollCardDetectNative();
                }
                return false;
            }
        }
        #endregion

        #region Parameters
        /// <summary>
        /// Parameter used for creating a MMC card instance.
        /// </summary>
        public class SDCardMmcParameters
        {
            /// <summary>
            /// Data width to use on MMC SD protocol.
            /// </summary>
            public SDDataWidth dataWidth;

            /// <summary>
            /// Set true when an Card Detect Pin is used. 
            /// The cardDetectPin parameter must have a valid GPIO pin.
            /// </summary>
            /// <remarks>
            /// Not all SD Card modules have a card detect pin or the pin connected to a GPIO pin. 
            /// </remarks>
            public bool enableCardDetectPin;

            /// <summary>
            /// The optional card detect GPIO pin which must be set to a valid pin if EnableCardDetectPin is true.
            /// If defined a StorageEventManager event will be raised when a card is inserted or removed.
            /// </summary>
            public uint cardDetectPin;
        }

        /// <summary>
        /// Parameter used for creating a SPI card instance.
        /// </summary>
        public class SDCardSpiParameters
        {
            /// <summary>
            /// The SPI bus to use for SD Card.
            /// </summary>
            public uint spiBus;
            /// <summary>
            /// The chip select pin to use for SD Card.
            /// </summary>
            public uint chipSelectPin;

            /// <summary>
            /// Set true when an Card Detect Pin is used. 
            /// The cardDetectPin parameter must have a valid GPIO pin.
            /// </summary>
            /// <remarks>
            /// Not all SD Card modules have a card detect pin or the pin connected to a GPIO pin. 
            /// </remarks>
            public bool enableCardDetectPin;

            /// <summary>
            /// The optional card detect GPIO pin which must be set to a valid pin if EnableCardDetectPin is true.
            /// If defined a StorageEventManager event will be raised when a card is inserted or removed.
            /// </summary>
            public uint cardDetectPin;
        };

        #endregion

        /// <summary>
        /// Creates an instance of SDcard where parameters have already been defined in firmware. 
        /// </summary>
        public SDCard()
        {
            _sdCardType = SDInterfaceType.System;

            InitNative();
        }

        /// <summary>
        /// Create an instance of SDCard for a MMC connected SD card.
        /// </summary>
        /// <param name="parameters">Connection parameters</param>
        public SDCard(SDCardMmcParameters parameters)
        {
            _sdCardType = SDInterfaceType.Mmc;
            _dataWidth = parameters.dataWidth;
            _enableCardDetectPin = parameters.enableCardDetectPin;
            _cardDetectPin = parameters.cardDetectPin;

            InitNative();
        }

        /// <summary>
        /// Create an instance of SDCard for a SPI connected SD card.
        /// </summary>
        /// <param name="parameters">Connection parameters</param>
        public SDCard(SDCardSpiParameters parameters)
        {
            _sdCardType = SDInterfaceType.Spi;
            _spiBus = parameters.spiBus;
            _chipSelectPin = parameters.chipSelectPin;
            _enableCardDetectPin = parameters.enableCardDetectPin;
            _cardDetectPin = parameters.cardDetectPin;

            InitNative();
        }

        /// <summary>
        /// Mount the SD memory card device 
        /// </summary>
        /// <remarks>
        /// This will try to mount the SD memory card on the specified interface.
        /// If the Card is not present or the card is unable to be read then an exception will be thrown.
        /// </remarks>
        public void Mount()
        {
            // upon return, and on successful execution, the native code has updated the _mounted field
            MountNative();
        }

         /// <summary>
        /// Unmount a mounted SD memory card.
        /// </summary>
        public void Unmount()
        {
            // upon return, and on successful execution, the native code has updated the _mounted field
            UnmountNative();
        }

        #region IDisposable Support

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                NativeDispose();

                _disposed = true;
            }
        }

#pragma warning disable 1591
        ~SDCard()
        {
            Dispose(false);
        }

        /// <summary>
        /// <inheritdoc cref="IDisposable.Dispose"/>
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                Dispose(true);

                GC.SuppressFinalize(this);
            }
        }

        #endregion

        #region Native Calls

        [Diagnostics.DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void InitNative();

        [Diagnostics.DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeDispose();

        [Diagnostics.DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void MountNative();
        
        [Diagnostics.DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void UnmountNative();

        [Diagnostics.DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern bool PollCardDetectNative();

        #endregion

        #region Enum

        /// <summary>
        /// SDCard interface type.
        /// </summary>
        public enum SDInterfaceType 
        { 
            /// <summary>
            /// Interface already defined in firmware. 
            /// </summary>
            System,

            /// <summary>
            /// MMC SDcard interface type
            /// </summary>
            Mmc,

            /// <summary>
            /// SPI SDCard interface type
            /// </summary>
            Spi
        };

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
        #endregion
    }
}
