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
    /// Provides an event handler that is called when a SD card detect state change event occurs.
    /// </summary>
    /// <param name="sender">Specifies the object that sent the event.</param>
    /// <param name="e">Contains the Card detect changed event arguments</param>
    public delegate void CardDetectStateEventHandler(
      object sender,
      CardDetectChangedEventArgs e);

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
        readonly private SDInterfaceType _sdCardType;
        // Card detect parameters
        private bool _enableCardDetectPin = false;
        private bool _cardDetectedState = false;
        private uint _cardDetectPin;
        private uint _slotIndex;
        private bool _autoMount = true;
        // MMC parameters
        readonly private SDDataWidth _dataWidth;
        // SPI parameters
        readonly private uint _spiBus;
        readonly private uint _chipSelectPin;

        #region Properties
        /// <summary>
        /// Type of interface used by SDCard.
        /// </summary>
        public SDInterfaceType CardType => _sdCardType;

        /// <summary>
        /// Indicates if the SD card has been mounted.
        /// </summary>
        public bool IsMounted => _mounted;

        /// <summary>
        /// Return true if Card detection is enabled.
        /// </summary>
        public bool CardDetectEnabled => _enableCardDetectPin;

        /// <summary>
        /// SD card slot index.
        /// </summary>
        public uint SlotIndex => _slotIndex;

        /// <summary>
        /// The parameters for a MMC connected SD card.
        /// </summary>
        public SDCardMmcParameters MmcParameters { get => new SDCardMmcParameters { slotIndex = _slotIndex, dataWidth = _dataWidth}; }

        /// <summary>
        /// The parameters for a SPI connected SD card. 
        /// </summary>
        public SDCardSpiParameters SpiParameters { get => new SDCardSpiParameters { slotIndex = _slotIndex, spiBus = _spiBus, chipSelectPin = _chipSelectPin}; }

        /// <summary>
        /// The Card detect parameters for SD card.
        /// </summary>
        public CardDetectParameters CdParameters { get => new CardDetectParameters { autoMount=_autoMount, cardDetectedState = _cardDetectedState, cardDetectPin = _cardDetectPin, enableCardDetectPin = _enableCardDetectPin }; }

        /// <summary>
        /// Event that occurs when SD card detect changes state.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <see cref="SDCard"/> class raises the <see cref="CardDetectChanged"/> event when an SD Cards is inserted or removed.
        /// This is only raised if SD card is configured with a Card Detect pin. Some SD card holders don't have this feature.
        /// </para>
        /// You only need to use this event if the <see cref="CardDetectParameters"/> are configured for a manual mount of card on card detect. The default is automatic.
        /// </remarks>
        public event CardDetectStateEventHandler CardDetectChanged;


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

        /// <summary>
        /// Creates an instance of SDcard where parameters have already been defined in firmware. 
        /// </summary>
        public SDCard(uint slotIndex = 0)
        {
            _sdCardType = SDInterfaceType.System;

            Initialise(slotIndex, null);
        }

        /// <summary>
        /// Create an instance of SDCard for a MMC connected SD card.
        /// </summary>
        /// <param name="mmcParameters">Connection parameters</param>
        /// <param name="cdParameters">Card detect parameters</param>
        public SDCard(SDCardMmcParameters mmcParameters, CardDetectParameters cdParameters = null)
        {
            _sdCardType = SDInterfaceType.Mmc;
            _dataWidth = mmcParameters.dataWidth;

            Initialise(mmcParameters.slotIndex, cdParameters);
        }

        /// <summary>
        /// Create an instance of SDCard for a SPI connected SD card.
        /// </summary>
        /// <param name="spiParameters">Connection parameters</param>
        /// <param name="cdParameters">Card detect parameters</param>
        public SDCard(SDCardSpiParameters spiParameters, CardDetectParameters cdParameters = null)
        {
            _sdCardType = SDInterfaceType.Spi;
            _spiBus = spiParameters.spiBus;
            _chipSelectPin = spiParameters.chipSelectPin;
 
            Initialise(spiParameters.slotIndex, cdParameters);
        }

        private void Initialise(uint slotIndex, CardDetectParameters cdParameters)
        {
            _slotIndex = slotIndex;

            _enableCardDetectPin = false;
            if (cdParameters != null)
            {
                _enableCardDetectPin = cdParameters.enableCardDetectPin;
                _cardDetectedState = cdParameters.cardDetectedState;
                _cardDetectPin = cdParameters.cardDetectPin;
                _autoMount = cdParameters.autoMount;
            }

            if (!StorageEventManager.RegisterSDcardForEvents(this))
            {
                throw new ArgumentException();
            }

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

        internal void OnEvent(bool pinstate, uint slotIndex)
        {
            CardDetectState cdstate = (pinstate == _cardDetectedState) ? CardDetectState.Inserted : CardDetectState.Removed;
            if (_autoMount)
            {
                // ignore any exceptions
                try
                {
                    if (cdstate == CardDetectState.Inserted)
                    {
                        // Auto mount volume if not already mounted
                        if (!IsMounted)
                        {
                            Mount();
                        }
                    }
                    else
                    {
                        // Auto unmount volume if mounted
                        if (IsMounted)
                        {
                            Unmount();
                        }
                    }
                }
                catch (Exception) 
                {  
                    //ignore exception
                }
            }

            CardDetectChanged?.Invoke(this, new CardDetectChangedEventArgs(cdstate, slotIndex));
        }

        #region IDisposable Support

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                StorageEventManager.RemoveSDcardFromEvents(this);

                // Try to unmount if disposed
                if (IsMounted)
                {
                    Unmount();
                }

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
