//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System.Runtime.CompilerServices;
using Diagnostics = System.Diagnostics;

namespace nanoFramework.System.IO.FileSystem
{
    /// <summary>
    /// Class to allow a SD memory card to be configured and mounted on the system.
    /// </summary>
    public class SDCard
    {

#pragma warning disable 0649

        // this field is set in native code
        [Diagnostics.DebuggerBrowsable(Diagnostics.DebuggerBrowsableState.Never)]
        private bool _mounted = false;

#pragma warning restore 0649

        private SDInterfaceType _sdCardType;
        private bool _isCardDetectEnabled = false;
        private int  _cardDetectPin;

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
                if (_isCardDetectEnabled)
                {
                    return PollCardDetectNative(_cardDetectPin);
                }

                return false;
            }
        }
        #endregion

        #region Parameters
        /// <summary>
        /// Parameter used for creating a MMC card instance.
        /// </summary>
        public struct SDCardMmcParameters
        {
            /// <summary>
            /// Data width to use on MMC SD protocol.
            /// </summary>
            public SDDataWidth DataWidth;

            /// <summary>
            /// Set true when an Card Detect Pin is used. 
            /// The cardDetectPin parameter must have a valid GPIO pin.
            /// </summary>
            /// <remarks>
            /// Not all SD Card modules have a card detect pin or the pin connected to a GPIO pin. 
            /// </remarks>
            public bool EnableCardDetectPin;

            /// <summary>
            /// The optional card detect GPIO pin which must be set to a valid pin if EnableCardDetectPin is true.
            /// If defined a StorageEventManager event will be raised when a card is inserted or removed.
            /// </summary>
            public uint CardDetectPin;
        }

        /// <summary>
        /// Parameter used for creating a SPI card instance.
        /// </summary>
        public struct SpiSDCardParameters
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
            InitSpiNative(-1, -1, false, -1);
        }

        /// <summary>
        /// Create an instance of SDCard for a MMC connected SD card.
        /// </summary>
        /// <param name="parameters">Connection parameters</param>
        public SDCard(SDCardMmcParameters parameters)
        {
            _sdCardType = SDInterfaceType.Mmc;
            
            _isCardDetectEnabled = parameters.EnableCardDetectPin;
            _cardDetectPin = (int)parameters.CardDetectPin;

            InitMmcNative(parameters.DataWidth, parameters.EnableCardDetectPin, (int)parameters.CardDetectPin);
        }

        /// <summary>
        /// Create an instance of SDCard for a SPI connected SD card.
        /// </summary>
        /// <param name="parameters">Connection parameters</param>
        public SDCard(SpiSDCardParameters parameters)
        {
            _sdCardType = SDInterfaceType.Spi;

            _isCardDetectEnabled = parameters.enableCardDetectPin;
            _cardDetectPin = (int)parameters.cardDetectPin;

            InitSpiNative((int)parameters.spiBus, (int)parameters.chipSelectPin, parameters.enableCardDetectPin, (int)parameters.cardDetectPin);
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

        #region Native Calls

        [Diagnostics.DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void InitMmcNative(SDDataWidth dataWidth, bool enableCardDetectPin, int InsertPin);

        [Diagnostics.DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void InitSpiNative(int SpiBus, int ChipSelect, bool enableInsertPin, int InsertPin);

        [Diagnostics.DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void MountNative();
        
        [Diagnostics.DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void UnmountNative();

        [Diagnostics.DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern bool PollCardDetectNative(int cardDetectPin);

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
