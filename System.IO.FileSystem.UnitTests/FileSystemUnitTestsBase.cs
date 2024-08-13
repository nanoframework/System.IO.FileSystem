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
        internal const int _numberOfDrives = 1;

        // set to the root of the drive to use for the tests
        // D: SD card
        // E: USB mass storage
        // I: and J: internal flash
        internal const string Root = @"I:\";

        public static bool WaitForRemovableDrive { get; set; } = false;

        public static bool ConfigAndMountSdCard { get; set; } = false;

        //////////////////////////////////////////////////

        /// <summary>
        /// Initializes the SD card. Can be overridden in the derived class to provide specific initialization.
        /// </summary>
        /// <returns></returns>
        protected SDCard InitializeSDCard()
        {
            // Example initialization logic
            SDCardMmcParameters parameters = new SDCardMmcParameters
            {
                slotIndex = 0,
                dataWidth = SDCard.SDDataWidth._4_bit,
            };

            SDCardCDParameters cdParameters = new SDCardCDParameters()
            {
                enableCardDetectPin = true,
                cardDetectPin = 21,
                autoMount = true
            };

            return new SDCard(parameters, cdParameters);
        }

        /// <summary>
        /// Helper method to be called from the tests to handle removable drives.
        /// </summary>
        internal void RemovableDrivesHelper()
        {
            if (ConfigAndMountSdCard)
            {
            TryToMountAgain:

                try
                {
                    SDCard card = InitializeSDCard();
                    card.Mount();
                }
                catch (Exception ex)
                {
                    OutputHelper.WriteLine($"SDCard mount failed: {ex.Message}");

                    Thread.Sleep(TimeSpan.FromSeconds(2));

                    goto TryToMountAgain;
                }
            }
            else
            {
                OutputHelper.WriteLine("***************************************");
                OutputHelper.WriteLine("*** Skipping SD card initialization ***");
                OutputHelper.WriteLine("***************************************");
            }

            if (WaitForRemovableDrive)
            {
                // wait until all removable drives are mounted
                while (DriveInfo.GetDrives().Length < _numberOfDrives)
                {
                    Thread.Sleep(1000);
                }
            }
            else
            {
                OutputHelper.WriteLine("******************************************");
                OutputHelper.WriteLine("*** Skipping wait for removable drives ***");
                OutputHelper.WriteLine("******************************************");
            }
        }
    }
}
