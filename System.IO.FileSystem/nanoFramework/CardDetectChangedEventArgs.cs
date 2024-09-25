//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System;

namespace nanoFramework.System.IO
{
    /// <summary>
    /// State of card detect
    /// </summary>
    public enum CardDetectState
    {
        /// <summary>
        /// Card Inserted
        /// </summary>
        Inserted,

        /// <summary>
        /// Card removed
        /// </summary>
        Removed
    };

    /// <summary>
    /// Arguments for Card detect event
    /// </summary>
    public class CardDetectChangedEventArgs : EventArgs
    {
        private CardDetectState _cardState;
        private uint _slotIndex;

        internal CardDetectChangedEventArgs(CardDetectState state, uint SlotIndex)
        {
            _cardState = state;
            _slotIndex = SlotIndex;
        }

        /// <summary>
        /// State of Card Detect.
        /// </summary>
        public CardDetectState CardState  => _cardState;

        /// <summary>
        /// SD card slot index
        /// </summary>
        public uint SlotIndex => _slotIndex;
    }
}
