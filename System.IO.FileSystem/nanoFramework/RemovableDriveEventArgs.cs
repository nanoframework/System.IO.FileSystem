//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System;
using System.IO;

namespace nanoFramework.System.IO
{
    /// <summary>
    /// Contains argument values for Removable Drive events.
    /// </summary>
    public class RemovableDriveEventArgs : EventArgs
    {
        private readonly DriveInfo _drive;
        private readonly RemovableDeviceEvent _event;

        internal RemovableDriveEventArgs(
            DriveInfo drive,
            RemovableDeviceEvent deviceEvent)
        {
            _drive = drive;
            _event = deviceEvent;
        }

        /// <summary>
        /// The <see cref="DriveInfo"/> of the removable drive.
        /// </summary>
        public DriveInfo Drive => _drive;

        /// <summary>
        /// The <see cref="RemovableDeviceEvent"/> occurred.
        /// </summary>
        public RemovableDeviceEvent Event => _event;

        /// <summary>
        /// Specifies the type of event occurred with the Removable Device specified.
        /// </summary>
        /// <remarks>
        /// This enum is specific to nanoFramework. There is no equivalent in the UWP API.
        /// </remarks>
        public enum RemovableDeviceEvent
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
