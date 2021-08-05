//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System;

namespace nanoFramework.Storage
{
    /// <summary>
    /// Contains argument values for Removable Storage Device events.
    /// </summary>
    public class RemovableStorageDeviceEventArgs : EventArgs
    {
        private readonly string _path;
        private readonly RemovableStorageDeviceEvent _event;

        internal RemovableStorageDeviceEventArgs(string path, RemovableStorageDeviceEvent deviceEvent)
        {
            _path = path;
            _event = deviceEvent;
        }

        /// <summary>
        /// The path of the Removable Storage Device.
        /// </summary>
        public string Path
        {
            get
            {
                return _path;
            }
        }

        /// <summary>
        /// The <see cref="RemovableStorageDeviceEvent"/> occurred.
        /// </summary>
        public RemovableStorageDeviceEvent Event
        {
            get
            {
                return _event;
            }
        }

        /// <summary>
        /// Specifies the type of event occurred with the Removable Storage Device specified.
        /// </summary>
        /// <remarks>
        /// This enum is specific to nanoFramework. There is no equivalent in the .Net API.
        /// </remarks>
        public enum RemovableStorageDeviceEvent
        {
            /// <summary>
            /// A Removable Device has been inserted.
            /// </summary>
            Inserted,

            /// <summary>
            /// A Removable Device has been removed.
            /// </summary>
            Removed,
        }
    }
}
