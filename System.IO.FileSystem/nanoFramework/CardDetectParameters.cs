//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

namespace nanoFramework.System.IO.FileSystem
{
    /// <summary>
    /// Parameter used for Card detection when creating a SDcard instance.
    /// </summary>
    public class CardDetectParameters
    {
        /// <summary>
        /// Set true when an Card Detect Pin is used. 
        /// The cardDetectPin parameter must have a valid GPIO pin.
        /// </summary>
        /// <remarks>
        /// Not all SD Card modules have a card detect pin or the pin connected to a GPIO pin. 
        /// </remarks>
        public bool enableCardDetectPin;

        /// <summary>
        /// The state of the pin when the card is detected.
        /// Defaults to false(low) if not specified.
        /// If using card detect logic then this depends on connected hardware.
        /// </summary>
        public bool cardDetectedState;

        /// <summary>
        /// The optional card detect GPIO pin which must be set to a valid pin if EnableCardDetectPin is true.
        /// If defined a StorageEventManager event will be raised when a card is inserted or removed.
        /// </summary>
        public uint cardDetectPin;

        /// <summary>
        /// When enabled will try to automatically mount the SD card when the card is inserted.
        /// If the card is removed unexpectedly it will try to unmount card to release resources.
        /// </summary>
        public bool autoMount;
    }
}
