//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using nanoFramework.Runtime.Events;
using System;
using static nanoFramework.Storage.RemovableStorageDeviceEventArgs;

namespace nanoFramework.Storage
{
    /// <summary>
    /// Provides an event handler that is called when a Removable Storage Device event occurs.
    /// </summary>
    /// <param name="sender">Specifies the object that sent the Removable Device event. </param>
    /// <param name="e">Contains the Removable Storage Device event arguments. </param>
    public delegate void RemovableStorageDeviceEventHandler(Object sender, RemovableStorageDeviceEventArgs e);

    /// <summary>
    /// Event manager for Removable Storage events.
    /// </summary>
    public static class RemovableStorageEventManager //TODO: this was based on https://raw.githubusercontent.com/nanoframework/Windows.Storage/develop/Windows.Storage/StorageEventManager.cs incase of reference.
    {
        [Flags]
        internal enum RemovableStorageEventType : byte
        {
            Invalid = 0,
            StorageDeviceInserted = 1,
            StorageDeviceRemoved = 2,
        }

        internal class RemovableStorageEvent : BaseEvent
        {
            public RemovableStorageEventType EventType;
            public byte DriveIndex;
            public DateTime Time;
        }

        internal class RemovableStorageEventListener : IEventListener, IEventProcessor
        {
            public void InitializeForEventSource()
            {
                // nothing to initialize here
            }

            public BaseEvent ProcessEvent(uint data1, uint data2, DateTime time)
            {
                RemovableStorageEvent RemovableStorageEvent = new RemovableStorageEvent
                {
                    EventType = (RemovableStorageEventType)((data1 >> 16) & 0xFF),
                    DriveIndex = (byte)(data2 & 0xFF),
                    Time = time
                };

                return RemovableStorageEvent;
            }

            public bool OnEvent(BaseEvent ev)
            {
                if (ev is RemovableStorageEvent)
                {
                    OnRemovableStorageEventCallback((StorageEvent)ev);
                }

                return true;
            }
        }

        /// <summary>
        /// Event that occurs when a Removable Storage Device is inserted.
        /// </summary>
        /// <remarks>
        /// The <see cref="RemovableStorageEventManager"/> class raises <see cref="RemovableStorageDeviceEventArgs"/> events when Removable Storage Devices (typically SD Cards and USB mass storage device) are inserted and removed.
        /// 
        /// To have a <see cref="RemovableStorageEventManager"/> object call an event-handling method when a <see cref="RemovableStorageDeviceInserted"/> event occurs, 
        /// you must associate the method with a <see cref="RemovableStorageDeviceEventHandler"/> delegate, and add this delegate to this event. 
        /// </remarks>
        public static event RemovableStorageDeviceEventHandler RemovableStorageDeviceInserted;

        /// <summary>
        /// Event that occurs when a Removable Storage Device is removed.
        /// </summary>
        /// <remarks>
        /// The <see cref="RemovableStorageEventManager"/> class raises <see cref="RemovableStorageDeviceEventArgs"/> events when Removable Storage Devices (typically SD Cards and USB mass storage device) are inserted and removed.
        /// 
        /// To have a <see cref="RemovableStorageEventManager"/> object call an event-handling method when a <see cref="RemovableStorageDeviceRemoved"/> event occurs, 
        /// you must associate the method with a <see cref="RemovableStorageDeviceEventHandler"/> delegate, and add this delegate to this event. 
        /// </remarks>
        public static event RemovableStorageDeviceEventHandler RemovableStorageDeviceRemoved;

        static RemovableStorageEventManager()
        {
            RemovableStorageEventListener RemovableStorageEventListener = new RemovableStorageEventListener();

            EventSink.AddEventProcessor(EventCategory.Storage, RemovableStorageEventListener);
            EventSink.AddEventListener(EventCategory.Storage, RemovableStorageEventListener);
        }

        internal static void OnRemovableStorageEventCallback(RemovableStorageEvent removableStorageEvent)
        {
            switch (removableStorageEvent.EventType)
            {
                case RemovableStorageEventType.StorageDeviceInserted:
                    {
                        if (RemovableStorageDeviceInserted != null)
                        {
                            RemovableStorageDeviceEventArgs args = new RemovableStorageDeviceEventArgs(DriveIndexToPath(removableStorageEvent.DriveIndex), RemovableStorageDeviceEvent.Inserted);

                            RemovableStorageDeviceInserted(null, args);
                        }
                        break;
                    }
                case RemovableStorageEventType.StorageDeviceRemoved:
                    {
                        if (RemovableStorageDeviceRemoved != null)
                        {
                            RemovableStorageDeviceEventArgs args = new RemovableStorageDeviceEventArgs(DriveIndexToPath(removableStorageEvent.DriveIndex), RemovableStorageDeviceEvent.Removed);

                            RemovableStorageDeviceRemoved(null, args);
                        }

                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        internal static string DriveIndexToPath(byte driveIndex)
        {
            /////////////////////////////////////////////////////////////////////////////////////
            // Drive indexes have a fixed mapping with a driver letter
            // Keep the various INDEX0_DRIVE_LETTER in sync with nanoHAL_Windows_Storage.h in native code
            /////////////////////////////////////////////////////////////////////////////////////

//TODO: should just base the drive index (0) against the ASCII letter `D` and continue increasing until `Z` before throwing an exception...
            switch (driveIndex)
            {
                // INDEX0_DRIVE_LETTER
                case 0:
                    return "D:";

                // INDEX1_DRIVE_LETTER
                case 1:
                    return "E:";

                // INDEX2_DRIVE_LETTER
                case 2:
                    return "F:";

                default:
#pragma warning disable S112 // General exceptions should never be thrown
                    throw new IndexOutOfRangeException();
#pragma warning restore S112 // General exceptions should never be thrown
            }
        }
    }
}
