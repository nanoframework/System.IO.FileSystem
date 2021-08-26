//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using nanoFramework.Runtime.Events;
using System;

namespace nanoFramework.System.IO
{
    /// <summary>
    /// Provides an event handler that is called when a Removable Device event occurs.
    /// </summary>
    /// <param name="sender">Specifies the object that sent the Removable Device event. </param>
    /// <param name="e">Contains the Removable Device event arguments. </param>
    public delegate void RemovableDeviceEventHandler(Object sender, RemovableDeviceEventArgs e);

    /// <summary>
    /// Event manager for Storage events.
    /// </summary>
    public static class StorageEventManager
    {
        [Flags]
        internal enum StorageEventType : byte
        {
            Invalid = 0,
            RemovableDeviceInsertion = 1,
            RemovableDeviceRemoval = 2,
        }

        internal class StorageEvent : BaseEvent
        {
            public StorageEventType EventType;
            public byte DriveIndex;
            public DateTime Time;
        }

        internal class StorageEventListener : IEventListener, IEventProcessor
        {
            public void InitializeForEventSource()
            {
                // nothing to initialize here
            }

            public BaseEvent ProcessEvent(uint data1, uint data2, DateTime time)
            {
                StorageEvent storageEvent = new StorageEvent
                {
                    EventType = (StorageEventType)((data1 >> 16) & 0xFF),
                    DriveIndex = (byte)(data2 & 0xFF),
                    Time = time
                };

                return storageEvent;
            }

            public bool OnEvent(BaseEvent ev)
            {
                if (ev is StorageEvent)
                {
                    OnStorageEventCallback((StorageEvent)ev);
                }

                return true;
            }
        }

        /// <summary>
        /// Event that occurs when a Removable Device is inserted.
        /// </summary>
        /// <remarks>
        /// The <see cref="StorageEventManager"/> class raises <see cref="RemovableDeviceEventArgs"/> events when Removable Devices (typically SD Cards and USB mass storage device) are inserted and removed.
        /// 
        /// To have a <see cref="StorageEventManager"/> object call an event-handling method when a <see cref="RemovableDeviceInserted"/> event occurs, 
        /// you must associate the method with a <see cref="RemovableDeviceEventHandler"/> delegate, and add this delegate to this event. 
        /// </remarks>
        public static event RemovableDeviceEventHandler RemovableDeviceInserted;

        /// <summary>
        /// Event that occurs when a Removable Device is removed.
        /// </summary>
        /// <remarks>
        /// The <see cref="StorageEventManager"/> class raises <see cref="RemovableDeviceEventArgs"/> events when Removable Devices (typically SD Cards and USB mass storage device) are inserted and removed.
        /// 
        /// To have a <see cref="StorageEventManager"/> object call an event-handling method when a <see cref="RemovableDeviceRemoved"/> event occurs, 
        /// you must associate the method with a <see cref="RemovableDeviceEventHandler"/> delegate, and add this delegate to this event. 
        /// </remarks>
        public static event RemovableDeviceEventHandler RemovableDeviceRemoved;

        static StorageEventManager()
        {
            StorageEventListener storageEventListener = new StorageEventListener();

            EventSink.AddEventProcessor(EventCategory.Storage, storageEventListener);
            EventSink.AddEventListener(EventCategory.Storage, storageEventListener);
        }

        internal static void OnStorageEventCallback(StorageEvent storageEvent)
        {
            switch (storageEvent.EventType)
            {
                case StorageEventType.RemovableDeviceInsertion:
                    {
                        if (RemovableDeviceInserted != null)
                        {
                            RemovableDeviceEventArgs args = new RemovableDeviceEventArgs(DriveIndexToPath(storageEvent.DriveIndex), RemovableDeviceEvent.Inserted);

                            RemovableDeviceInserted(null, args);
                        }
                        break;
                    }
                case StorageEventType.RemovableDeviceRemoval:
                    {
                        if (RemovableDeviceRemoved != null)
                        {
                            RemovableDeviceEventArgs args = new RemovableDeviceEventArgs(DriveIndexToPath(storageEvent.DriveIndex), RemovableDeviceEvent.Removed);

                            RemovableDeviceRemoved(null, args);
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
