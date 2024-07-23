//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.IO
{
    /// <summary>
    /// Provides access to information on a drive.
    /// </summary>
    public sealed class DriveInfo
    {
        private DriveType _driveType;
        private string _name;
        private long _totalSize;
        internal uint _volumeIndex;

        /// <summary>
        /// Gets the drive type, such as removable, fixed or RAM.
        /// </summary>
        /// <value>
        /// One of the enumeration values that specifies a drive type.
        /// </value>
        public DriveType DriveType { get => _driveType; set => _driveType = value; }

        /// <summary>
        /// Gets the name of a drive, such as C:\.
        /// </summary>
        /// <value>
        /// The name of the drive.
        /// </value>
        public string Name { get => _name; set => _name = value; }

        /// <summary>
        /// Gets the total size of storage space on a drive, in bytes.
        /// </summary>
        /// <value>
        /// The total size of the drive, in bytes.
        /// </value>
        /// <exception cref="IOException">An I/O error occurred (for example, a disk error or a drive was not ready).</exception>
        public long TotalSize { get => _totalSize; set => _totalSize = value; }

        /// <summary>
        /// Creates a new instance of the <see cref="DriveInfo"/> class.
        /// </summary>
        /// <param name="driveName">A valid drive letter or fully qualified path.</param>
        /// <exception cref="ArgumentNullException">The drive letter cannot be <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="driveName"/> does not refer to a valid drive.</exception>
        /// <remarks>
        /// <para>
        /// You can't use this constructor to obtain information on drive names that are null or use UNC (\\server\share) paths.
        /// </para>
        /// <para>
        /// On .NET nanoFramework, this constructor only supports drive letters.
        /// </para>
        /// </remarks>
        public DriveInfo(string driveName)
        {
            _name = driveName;

            DriveInfoNative(driveName);
        }

        /// <summary>
        /// Retrieves the drive names of all logical drives on a computer.
        /// </summary>
        /// <returns>
        /// An array of type <see cref="DriveInfo"/> that represents the logical drives on a computer.
        /// </returns>
        public static DriveInfo[] GetDrives()
        {
            return GetDrivesNative();
        }

        /// <summary>
        /// Formats the specified drive.
        /// *** NOTE THAT THIS OPERATION IS NOT REVERSIBLE ***.
        /// </summary>
        /// <param name="fileSystem">File system to use for the format operation.</param>
        /// <param name="parameter">A parameter to pass to the format operation.</param>
        /// <exception cref="NotSupportedException">Thrown when the target doesn't have support for performing the format operation on the specified drive.</exception>
        /// <exception cref="IOException">Thrown when the operation fails.</exception>
        /// <remarks>
        /// <para>This method is not reversible. Once the drive is formatted, all data on the drive is lost.</para>
        /// <para>This method is implemented in the .NET nanoFramework API but it is not supported on all target devices nor on all file systems.</para>
        /// </remarks>
        public void Format(
            string fileSystem,
            uint parameter)
        {
            bool needToRestoreCurrentDir = FileSystemManager.CurrentDirectory == Name;

            if (FileSystemManager.IsInDirectory(FileSystemManager.CurrentDirectory, Name))
            {
                FileSystemManager.SetCurrentDirectory(NativeIO.FSRoot);
            }

            FileSystemManager.ForceRemoveRootname(Name);

            object record = FileSystemManager.LockDirectory(Name);

            try
            {
                NativeIO.Format(
                    Name,
                    fileSystem,
                    parameter);

                Refresh();
            }
            finally
            {
                FileSystemManager.UnlockDirectory(record);
            }

            if (needToRestoreCurrentDir)
            {
                FileSystemManager.SetCurrentDirectory(Name);
            }
        }

        /// <summary>
        /// Refreshes the information about the drive.
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void Refresh();

        /// <summary>
        /// Retrieves the names of the file systems available on the connected device.
        /// </summary>
        /// <returns>An array of strings that represent the names of the file systems available on the connected device.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern static string[] GetFileSystems();

        /// <summary>
        /// Tries to mount all removable volumes.
        /// </summary>
        /// <remarks>
        /// This method is implemented in the .NET nanoFramework API but it is not supported on all target devices.
        /// </remarks>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void MountRemovableVolumes();

        #region Native Calls

        [DebuggerNonUserCode]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void DriveInfoNative(string driveName);

        [DebuggerNonUserCode]
        [MethodImpl(MethodImplOptions.InternalCall)]
        internal extern DriveInfo(uint volumeIndex);

        [DebuggerNonUserCode]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern DriveInfo[] GetDrivesNative();

        #endregion
    }
}
