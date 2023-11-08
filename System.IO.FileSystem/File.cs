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
        private const int ChunkSize = 2048;
        private static readonly byte[] EmptyBytes = new byte[0];

        /// <summary>
        /// Copies an existing file to a new file. Overwriting a file of the same name is not allowed.
        /// </summary>
        /// <param name="sourceFileName">The file to copy.</param>
        /// <param name="destFileName">The name of the destination file. This cannot be a directory or an existing file.</param>
        /// <exception cref="ArgumentException">sourceFileName or destFileName is null or empty</exception>
        public static void Copy(string sourceFileName, string destFileName) => Copy(sourceFileName, destFileName, overwrite: false);

        /// <summary>
        /// Copies an existing file to a new file. Overwriting a file of the same name is allowed.
        /// </summary>
        /// <param name="sourceFileName">The file to copy.</param>
        /// <param name="destFileName">The name of the destination file. This cannot be a directory.</param>
        /// <param name="overwrite"><c>true&lt;/c&gt; if the destination file can be overwritten; otherwise, <c>false</c>.</param>
        /// <exception cref="ArgumentException">sourceFileName or destFileName is null or empty</exception>
        public static void Copy(string sourceFileName, string destFileName, bool overwrite)
        {
            if (string.IsNullOrEmpty(sourceFileName))
            {
                throw new ArgumentException(nameof(sourceFileName));
            }

            if (string.IsNullOrEmpty(destFileName))
            {
                throw new ArgumentException(nameof(destFileName));
            }

            if (sourceFileName == destFileName)
            {
                throw new ArgumentException();
            }

            var destMode = overwrite ? FileMode.Create : FileMode.CreateNew;

            using var sourceStream = new FileStream(sourceFileName, FileMode.Open, FileAccess.Read);
            using var destStream = new FileStream(destFileName, destMode, FileAccess.Write);

            var buffer = new byte[ChunkSize];
            var bytesRead = 0;

            while ((bytesRead = sourceStream.Read(buffer, 0, ChunkSize)) > 0)
            {
                destStream.Write(buffer, 0, bytesRead);
            }

            // Copy the attributes too
            SetAttributes(destFileName, GetAttributes(sourceFileName));
        }

        /// <summary>
        /// Creates or overwrites a file in the specified path.
        /// </summary>
        /// <param name="path">The path and name of the file to create.</param>
        public static FileStream Create(string path)
        {
            return new FileStream(path, FileMode.Create, FileAccess.ReadWrite);
        }

        /// <summary>
        /// Deletes the specified file.
        /// </summary>
        /// <param name="path">The name of the file to be deleted. Wildcard characters are not supported.</param>
        /// <exception cref="ArgumentException"><paramref name="path"/> is null or empty.</exception>
        /// <exception cref="IOException">Directory is not found or <paramref name="path"/> is read-only or a directory.</exception>
        public static void Delete(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException(nameof(path));
            }

            Path.CheckInvalidPathChars(path);

            try
            {
                byte attributes;
                var directoryName = Path.GetDirectoryName(path);

                // Only check folder if its not the Root
                if (directoryName != Path.GetPathRoot(path))
                {
                    attributes = GetAttributesNative(directoryName);

                    // Check if Directory existing
                    if (attributes == 0xFF)
                    {
                        throw new IOException(string.Empty, (int)IOException.IOExceptionErrorCode.DirectoryNotFound);
                    }
                }

                // Folder exists, now verify whether the file itself exists.
                attributes = GetAttributesNative(path);

                if (attributes == 0xFF)
                {
                    // No-op on file not found
                    return;
                }

                // Check if file is director or read-only (then not allowed to delete)
                if ((attributes & (byte)FileAttributes.Directory) != 0)
                {
                    throw new IOException("Cannot delete directory.", (int)IOException.IOExceptionErrorCode.UnauthorizedAccess);
                }

                if ((attributes & (byte)FileAttributes.ReadOnly) != 0)
                {
                    throw new IOException("File is read-only", (int)IOException.IOExceptionErrorCode.UnauthorizedAccess);
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
        /// <returns><c>true</c> if the file exists; otherwise <c>false</c>.</returns>
        public static bool Exists(string path)
        {
            return ExistsNative(Path.GetDirectoryName(path), Path.GetFileName(path));
        }

        /// <summary>
        /// Gets the <see cref="FileAttributes"/> of the file on the path.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <exception cref="IOException">File not found.</exception>
        public static FileAttributes GetAttributes(string path)
        {
            // Adding this check because on my device the GetAttributesNative call throws an Exception instead of IOException if the file is not found
            if (!Exists(path))
            {
                throw new IOException(string.Empty, (int)IOException.IOExceptionErrorCode.FileNotFound);
            }

            var attributes = GetAttributesNative(path);

            if (attributes == 0xFF)
            {
                throw new IOException(string.Empty, (int)IOException.IOExceptionErrorCode.FileNotFound);
            }

            return (FileAttributes)attributes;
        }

        /// <summary>
        /// Determines the time of the last write/modification to file under given path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns>Time of the last write/modification.</returns>
        /// <exception cref="IOException"> Logical drive or a file under given path does not exist. </exception>
        public static DateTime GetLastWriteTime(string path)
        {
            return GetLastWriteTimeNative(path);
        }

        /// <summary>
        /// Moves a specified file to a new location, providing the option to specify a new file name.
        /// </summary>
        /// <param name="sourceFileName">The name of the file to move. Absolute path.</param>
        /// <param name="destFileName">The new path and name for the file.</param>
        /// <exception cref="Exception">Source File not existing or Destination File already existing.</exception>
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
        /// Sets the specified FileAttributes of the file on the specified path.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <param name="fileAttributes">A bitwise combination of the enumeration values.</param>
        public static void SetAttributes(string path, FileAttributes fileAttributes)
        {
            SetAttributesNative(path, (byte)fileAttributes);
        }

        #region Native Methods
        [Diagnostics.DebuggerStepThrough]
        [Diagnostics.DebuggerHidden]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void DeleteNative(string path);

        [Diagnostics.DebuggerStepThrough]
        [Diagnostics.DebuggerHidden]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern bool ExistsNative(string path, string fileName);

        [Diagnostics.DebuggerStepThrough]
        [Diagnostics.DebuggerHidden]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern byte GetAttributesNative(string path);

        [Diagnostics.DebuggerStepThrough]
        [Diagnostics.DebuggerHidden]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern DateTime GetLastWriteTimeNative(string path);

        [Diagnostics.DebuggerStepThrough]
        [Diagnostics.DebuggerHidden]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void MoveNative(string pathSrc, string pathDest);

        [Diagnostics.DebuggerStepThrough]
        [Diagnostics.DebuggerHidden]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void SetAttributesNative(string path, byte attributes);
        #endregion
    }
}
