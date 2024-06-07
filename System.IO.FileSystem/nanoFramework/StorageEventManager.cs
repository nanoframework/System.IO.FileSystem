//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using nanoFramework.Runtime.Events;
using System;
using System.Collections;
using System.IO;
using static nanoFramework.System.IO.RemovableDriveEventArgs;

namespace nanoFramework.System.IO
{
    /// <summary>
    /// Provides an event handler that is called when a Removable Device event occurs.
    /// </summary>
    /// <param name="sender">Specifies the object that sent the Removable Device event. </param>
    /// <param name="e">Contains the Removable Device event arguments. </param>
    public delegate void RemovableDeviceEventHandler(
        object sender,
        RemovableDriveEventArgs e);

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
            public uint VolumeIndex;
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
                    EventType = (StorageEventType)(data1 & 0xFF),
                    VolumeIndex = data2,
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
        /// The <see cref="StorageEventManager"/> class raises <see cref="RemovableDriveEventArgs"/> events when Removable Devices (typically SD Cards and USB mass storage device) are inserted and removed.
        /// 
        /// To have a <see cref="StorageEventManager"/> object call an event-handling method when a <see cref="RemovableDeviceInserted"/> event occurs, 
        /// you must associate the method with a <see cref="RemovableDeviceEventHandler"/> delegate, and add this delegate to this event. 
        /// </remarks>
        public static event RemovableDeviceEventHandler RemovableDeviceInserted;

        /// <summary>
        /// Event that occurs when a Removable Device is removed.
        /// </summary>
        /// <remarks>
        /// The <see cref="StorageEventManager"/> class raises <see cref="RemovableDriveEventArgs"/> events when Removable Devices (typically SD Cards and USB mass storage device) are inserted and removed.
        /// 
        /// To have a <see cref="StorageEventManager"/> object call an event-handling method when a <see cref="RemovableDeviceRemoved"/> event occurs, 
        /// you must associate the method with a <see cref="RemovableDeviceEventHandler"/> delegate, and add this delegate to this event. 
        /// </remarks>
        public static event RemovableDeviceEventHandler RemovableDeviceRemoved;

        private static ArrayList _drives;

        static StorageEventManager()
        {
            StorageEventListener storageEventListener = new();

            EventSink.AddEventProcessor(EventCategory.Storage, storageEventListener);
            EventSink.AddEventListener(EventCategory.Storage, storageEventListener);

            _drives = new ArrayList();

            DriveInfo.MountRemovableVolumes();
        }

        internal static void OnStorageEventCallback(StorageEvent storageEvent)
        {
            lock (_drives)
            {
                switch (storageEvent.EventType)
                {
                    case StorageEventType.RemovableDeviceInsertion:
                        {
                            DriveInfo drive = new(storageEvent.VolumeIndex);
                            _drives.Add(drive);

                            RemovableDeviceInserted?.Invoke(null, new RemovableDriveEventArgs(
                                drive,
                                RemovableDeviceEvent.Inserted));
                            break;
                        }

                    case StorageEventType.RemovableDeviceRemoval:
                        {
                            DriveInfo drive = RemoveDrive(storageEvent.VolumeIndex);

                            if (drive != null)
                            {
                                FileSystemManager.ForceRemoveNameSpace(drive.Name);

                                RemovableDeviceRemoved?.Invoke(null, new RemovableDriveEventArgs(drive, RemovableDeviceEvent.Removed));
                            }

                            break;
                        }

                    default:
                        break;
                }
            }
        }


        private static DriveInfo RemoveDrive(uint volumeIndex)
        {
            for (int i = 0; i < _drives.Count; i++)
            {
                DriveInfo drive = (DriveInfo)_drives[i];

                if (drive._volumeIndex == volumeIndex)
                {
                    _drives.RemoveAt(i);
                    return drive;
                }
            }

            return null;
        }
    }
}
