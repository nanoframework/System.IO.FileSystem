//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using nanoFramework.System.IO.FileSystem;
using nanoFramework.TestFramework;
using System.Threading;

namespace System.IO.FileSystem.UnitTests
{
    [TestClass]
    public abstract class FileSystemUnitTestsBase
    {
        /////////////////////////////////////////////////////////////////////
        // The test execution can be configured using the following fields //
        /////////////////////////////////////////////////////////////////////

        // set to the number of drives available in the target
        internal const int _numberOfDrives = 2;

        // set to the root of the drive to use for the tests
        // D: SD card
        // E: USB mass storage
        // I: and J: internal flash
        internal const string Root = @"D:\";

        // set to true to wait for removable drive(s) to be mounted
        internal const bool _waitForRemovableDrive = true;

        // set to true to have SPI SD card mounted
        internal const bool _configAndMountSdCard = true;

        //////////////////////////////////////////////////

        private SDCard _mycardBacking;

        internal SDCard MyCard
        {
            set
            {
                _mycardBacking = value;
            }

            get
            {
                _mycardBacking ??= InitializeSDCard();

                return _mycardBacking;
            }
        }

        /// <summary>
        /// Initializes the SD card. Can be overridden in the derived class to provide specific initialization.
        /// </summary>
        /// <returns></returns>
        protected SDCard InitializeSDCard()
        {
            // Example initialization logic
            SDCard.SDCardMmcParameters parameters = new SDCard.SDCardMmcParameters
            {
                dataWidth = SDCard.SDDataWidth._4_bit,
                enableCardDetectPin = true,
                cardDetectPin = 21
            };

            return new SDCard(parameters);
        }

        /// <summary>
        /// Helper method to be called from the tests to handle removable drives.
        /// </summary>
        internal void RemovableDrivesHelper()
        {
            if (_configAndMountSdCard)
            {
            TryToMountAgain:

                try
                {
                    MyCard.Mount();
                }
                catch (Exception ex)
                {
                    OutputHelper.WriteLine($"SDCard mount failed: {ex.Message}");

                    Thread.Sleep(TimeSpan.FromSeconds(2));

                    MyCard = null;

                    goto TryToMountAgain;
                }
            }

            if (_waitForRemovableDrive)
            {
                // wait until all removable drives are mounted
                while (DriveInfo.GetDrives().Length < _numberOfDrives)
                {
                    Thread.Sleep(1000);
                }
            }
        }
    }
}
