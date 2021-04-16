//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System.Runtime.CompilerServices;

namespace System.IO
{
    /// <summary>
    /// Class for creating FileStream objects, and some basic file management
    /// routines such as Delete, etc.
    /// </summary>
    public static class File
    {
        #region Constants

        private const int _defaultCopyBufferSize = 2048;

        #endregion


        #region Static Methods

        /// <summary>
        /// Copies an existing file to a new file. Overwriting a file of the same name is not allowed.
        /// </summary>
        /// <param name="sourceFileName">The file to copy.</param>
        /// <param name="destFileName">The name of the destination file. This cannot be a directory or an existing file.</param>
        /// <exception cref="ArgumentException">sourceFileName or destFileName is null or empty</exception>
        public static void Copy(
            string sourceFileName,
            string destFileName)
        {
            Copy(sourceFileName, destFileName, false);
        }

        /// <summary>
        /// Copies an existing file to a new file. Overwriting a file of the same name is allowed.
        /// </summary>
        /// <param name="sourceFileName">The file to copy.</param>
        /// <param name="destFileName">The name of the destination file. This cannot be a directory.</param>
        /// <param name="overwrite">true if the destination file can be overwritten; otherwise, false.</param>
        /// <exception cref="ArgumentException">sourceFileName or destFileName is null or empty</exception>
        public static void Copy(
            string sourceFileName,
            string destFileName,
            bool overwrite)
        {
            if (string.IsNullOrEmpty(sourceFileName))
            {
                throw new ArgumentException(nameof(sourceFileName));
            }

            if (string.IsNullOrEmpty(destFileName))
            {
                throw new ArgumentException(nameof(sourceFileName));
            }

            FileMode writerMode = overwrite ? FileMode.Create : FileMode.CreateNew;

            FileStream reader = new FileStream(sourceFileName, FileMode.Open, FileAccess.Read);

            try
            {
                using (FileStream writer = new FileStream(destFileName, writerMode, FileAccess.Write))
                {
                    long fileLength = reader.Length;
                    //writer.SetLength(fileLength);

                    byte[] buffer = new byte[_defaultCopyBufferSize];

                    while (true)
                    {
                        int readSize = reader.Read(buffer, 0, _defaultCopyBufferSize);

                        if (readSize <= 0)
                        {
                            break;
                        }

                        writer.Write(buffer, 0, readSize);
                    }

                    // Copy the attributes too
                    SetAttributes(destFileName, GetAttributes(sourceFileName));
                }
            }
            finally
            {
                reader.Dispose();
            }
        }

        /// <summary>
        /// Creates or overwrites a file in the specified path.
        /// </summary>
        /// <param name="path">The path and name of the file to create.</param>
        /// <returns></returns>
        public static FileStream Create(string path)
        {
            return new FileStream(path, FileMode.Create, FileAccess.ReadWrite);
        }

        /// <summary>
        /// Deletes the specified file.
        /// </summary>
        /// <param name="path">The name of the file to be deleted. Wild-card characters are not supported.</param>
        /// <exception cref="ArgumentNullException">Path must be defined.</exception>
        /// <exception cref="IOException">Directory not found. or Not allowed to delete ReadOnly Files. or Not allowed to delete Directories.</exception>
        public static void Delete(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            Path.CheckInvalidPathChars(path);

            try
            {
                byte attributes;
                string folderPath = Path.GetDirectoryName(path);

                // Only check folder if its not the Root
                if (folderPath != Path.GetPathRoot(path))
                {
                    attributes = GetAttributesNative(folderPath);

                    // Check if Directory existing
                    if (attributes == 0xFF)
                    {
                        throw new IOException("", (int)IOException.IOExceptionErrorCode.DirectoryNotFound);
                    }
                }

                // Folder exists, now verify whether the file itself exists.
                attributes = GetAttributesNative(path);

                if (attributes == 0xFF)
                {
                    // No-op on file not found
                    return;
                }

                // Check if file is ReadOnly or Directory (then not allowed to delete)
                if ((attributes & (byte)(FileAttributes.ReadOnly)) != 0)
                {
                    throw new IOException("ReadOnly Files.", (int)IOException.IOExceptionErrorCode.UnauthorizedAccess);
                }

                if ((attributes & (byte)(FileAttributes.Directory)) != 0)
                {
                    throw new IOException("Not allowed to delete Directories.", (int)IOException.IOExceptionErrorCode.UnauthorizedAccess);
                }

                DeleteNative(path);
            }
            finally
            {
                // TODO: File Handling missing. (Should not be possible to delete File in use!)
            }
        }

        /// <summary>
        /// Determines whether the specified file exists.
        /// </summary>
        /// <param name="path">The file to check.</param>
        /// <returns></returns>
        public static bool Exists(string path)
        {
            return ExistsNative(Path.GetDirectoryName(path), Path.GetFileName(path));
        }

        /// <summary>
        /// Moves a specified file to a new location, providing the option to specify a new file name.
        /// </summary>
        /// <param name="sourceFileName">The name of the file to move. Absolute path.</param>
        /// <param name="destFileName">The new path and name for the file.</param>
        /// /// <exception cref="Exception">Source File not existing or Destination File already existing.</exception>
        public static void Move(
            string sourceFileName,
            string destFileName)
        {
            // Src File must exists!
            if (!Exists(sourceFileName))
            {
#pragma warning disable S112 // General exceptions should never be thrown
                throw new Exception(nameof(sourceFileName));
#pragma warning restore S112 // General exceptions should never be thrown
            }

            // Dest must not exist!
            if (Exists(destFileName))
            {
#pragma warning disable S112 // General exceptions should never be thrown
                throw new Exception(nameof(destFileName));
#pragma warning restore S112 // General exceptions should never be thrown
            }

            // TODO: File Handling missing

            // Check the volume of files
            if (Path.GetPathRoot(sourceFileName) != Path.GetPathRoot(destFileName))
            {
                // Cross Volume move (FAT_FS move not working)
                Copy(sourceFileName, destFileName);
                Delete(sourceFileName);
            }
            else
            {
                // Same Volume (FAT_FS move)
                MoveNative(sourceFileName, destFileName);
            }
        }

        /// <summary>
        /// Gets the FileAttributes of the file on the path.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns>The FileAttributes of the file on the path.</returns>
        /// <exception cref="IOException">File not found.</exception>
        public static FileAttributes GetAttributes(string path)
        {
            byte attributes;

            attributes = GetAttributesNative(path);

            if (attributes == 0xFF)
            {
                throw new IOException("", (int)IOException.IOExceptionErrorCode.FileNotFound);
            }
            else
            {
                return (FileAttributes)attributes;
            }
        }

        /// <summary>
        /// Sets the specified FileAttributes of the file on the specified path.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <param name="fileAttributes">A bitwise combination of the enumeration values.</param>
        public static void SetAttributes(string path, FileAttributes fileAttributes)
        {
            SetAttributesNative(path, (byte)fileAttributes);
        }

        public static DateTime GetCreationTime(string path)
        {
            return GetCreationTimeNative(path);
        }

        public static DateTime GetLastAccessTime(string path)
        {
            return GetLastAccessTimeNative(path);
        }

        public static DateTime GetLastWriteTime(string path)
        {
            return GetLastWriteTimeNative(path);
        }

        #endregion

        #region Stubs (Native Calls)

        [Diagnostics.DebuggerStepThrough]
        [Diagnostics.DebuggerHidden]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern bool ExistsNative(string path, string fileName);

        [Diagnostics.DebuggerStepThrough]
        [Diagnostics.DebuggerHidden]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void MoveNative(string pathSrc, string pathDest);

        [Diagnostics.DebuggerStepThrough]
        [Diagnostics.DebuggerHidden]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void DeleteNative(string path);

        [Diagnostics.DebuggerStepThrough]
        [Diagnostics.DebuggerHidden]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern byte GetAttributesNative(string path);

        [Diagnostics.DebuggerStepThrough]
        [Diagnostics.DebuggerHidden]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void SetAttributesNative(string path, byte attributes);

        [Diagnostics.DebuggerStepThrough]
        [Diagnostics.DebuggerHidden]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern DateTime GetLastWriteTimeNative(string path);

        #endregion
    }
}
