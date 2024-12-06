//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using nanoFramework.Runtime.Events;
using nanoFramework.System.IO.FileSystem;
using System;
using System.Collections;
using System.IO;
using System.Threading;
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
        internal enum StorageEventTypes : byte
        {
            None = 0,
            RemovableDeviceInsertion = 1,
            RemovableDeviceRemoval = 2,
            CardDetectChanged = 3
        }

        internal class StorageEvent : BaseEvent
        {
            public StorageEventTypes EventType;
            public uint VolumeIndex;
            public uint SlotIndex;
            public DateTime Time;
            public bool state;
        }

        internal class StorageEventListener : IEventListener, IEventProcessor
        {
            public void InitializeForEventSource()
            {
                // nothing to initialize here
            }

            public BaseEvent ProcessEvent(uint data1, uint data2, DateTime time)
            {
                // Data1 =  0xddddccss
                // dddd = data1
                // cc = category (StorageEvent)
                // ss = subCategory ( insert, remove )
                StorageEvent storageEvent = new StorageEvent()
                {
                    // EventType = subCategory
                    EventType = (StorageEventTypes)(data1 & 0xFF),
                    state = ((data1 >> 16) == 1),
                    VolumeIndex = data2,
                    SlotIndex = data2,
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
        /// The <see cref="StorageEventManager"/> class raises <see cref="RemovableDeviceInserted"/> events when Removable Devices (typically SD Cards and USB mass storage device) are inserted and removed.
        /// 
        /// To have a <see cref="StorageEventManager"/> object call an event-handling method when a <see cref="RemovableDeviceInserted"/> event occurs, 
        /// you must associate the method with a <see cref="RemovableDeviceEventHandler"/> delegate, and add this delegate to this event. 
        /// </remarks>
        public static event RemovableDeviceEventHandler RemovableDeviceInserted;

        /// <summary>
        /// Event that occurs when a Removable Device is removed.
        /// </summary>
        /// <remarks>
        /// The <see cref="StorageEventManager"/> class raises <see cref="RemovableDeviceRemoved"/> events when Removable Devices (typically SD Cards and USB mass storage device) are inserted and removed.
        /// 
        /// To have a <see cref="StorageEventManager"/> object call an event-handling method when a <see cref="RemovableDeviceRemoved"/> event occurs, 
        /// you must associate the method with a <see cref="RemovableDeviceEventHandler"/> delegate, and add this delegate to this event. 
        /// </remarks>
        public static event RemovableDeviceEventHandler RemovableDeviceRemoved;

        private static ArrayList _drives;
        private static ArrayList _sdCardList;

        static StorageEventManager()
        {
            StorageEventListener storageEventListener = new();

            EventSink.AddEventProcessor(EventCategory.Storage, storageEventListener);
            EventSink.AddEventListener(EventCategory.Storage, storageEventListener);

            _drives = new ArrayList();
            _sdCardList = new ArrayList();

            DriveInfo.MountRemovableVolumes();
        }

        internal static void OnStorageEventCallback(StorageEvent storageEvent)
        {
            lock (_drives)
            {
                switch (storageEvent.EventType)
                {
                    case StorageEventTypes.RemovableDeviceInsertion:
                        {
                            DriveInfo drive = new(storageEvent.VolumeIndex);
                            _drives.Add(drive);

                            RemovableDeviceInserted?.Invoke(null, new RemovableDriveEventArgs(
                                drive,
                                RemovableDeviceEvent.Inserted));
                            break;
                        }

                    case StorageEventTypes.RemovableDeviceRemoval:
                        {
                            DriveInfo drive = RemoveDrive(storageEvent.VolumeIndex);

                            if (drive != null)
                            {
                                FileSystemManager.ForceRemoveRootname(drive.Name);

                                RemovableDeviceRemoved?.Invoke(null, new RemovableDriveEventArgs(drive, RemovableDeviceEvent.Removed));
                            }

                            break;
                        }

                    case StorageEventTypes.CardDetectChanged:
                        {
                            SDCard card = FindRegisteredEvent(storageEvent.SlotIndex);
                            if (card != null)   
                            {
                                card.OnEvent(storageEvent.state, storageEvent.SlotIndex);
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

        /// <summary>
        /// Register SDCard object for events
        /// </summary>
        /// <param name="card">SDcard object reference.</param>
        /// <returns>True if successfully registered, false for duplicate index</returns>
        internal static bool RegisterSDcardForEvents(SDCard card)
        {
            if (FindRegisteredEvent(card.SlotIndex) == null)
            {
                _sdCardList.Add(card);
                return true;
            }

            // Slot index already registered for events, duplicate
            return false;
        }

        internal static void RemoveSDcardFromEvents(SDCard card)
        {
            for (int i = 0; i < _sdCardList.Count; i++)
            {
                SDCard item = (SDCard)_sdCardList[i];

                if (item.SlotIndex == card.SlotIndex)
                {
                    _sdCardList.RemoveAt(i);
                }
            }
        }

        internal static SDCard FindRegisteredEvent(uint slotIndex)
        {
            for (int i = 0; i < _sdCardList.Count; i++)
            {
                SDCard item = (SDCard)_sdCardList[i];
                if (item.SlotIndex == slotIndex)
                {
                    return item;
                }
            }
            return null;
        }
    }
}
