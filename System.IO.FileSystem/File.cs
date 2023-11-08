//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System.Runtime.CompilerServices;
using System.Text;

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
        /// <exception cref="ArgumentException"><paramref name="sourceFileName"/> or <paramref name="destFileName"/> is null or empty.</exception>
        public static void Copy(string sourceFileName, string destFileName)
        {
            Copy(sourceFileName, destFileName, overwrite: false);
        }

        /// <summary>
        /// Copies an existing file to a new file. Overwriting a file of the same name is allowed.
        /// </summary>
        /// <param name="sourceFileName">The file to copy.</param>
        /// <param name="destFileName">The name of the destination file. This cannot be a directory.</param>
        /// <param name="overwrite"><see langword="true"/> if the destination file can be overwritten; otherwise, <see langword="false"/>.</param>

        /// <exception cref="ArgumentException"><paramref name="sourceFileName"/> or <paramref name="destFileName"/> is <see langword="null"/> or empty.</exception>

        public static void Copy(string sourceFileName, string destFileName, bool overwrite)
        {
            if (string.IsNullOrEmpty(sourceFileName))
            {
                throw new ArgumentException();
            }

            if (string.IsNullOrEmpty(destFileName))
            {
                throw new ArgumentException();
            }

            if (sourceFileName == destFileName)
            {
                return;
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
        /// <exception cref="ArgumentException"><paramref name="path"/> is <see langword="null"/> or empty.</exception>

        /// <exception cref="IOException">Directory is not found or <paramref name="path"/> is read-only or a directory.</exception>
        public static void Delete(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException();
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

                // Check if file is directory or read-only (then not allowed to delete)
                if ((attributes & (byte)FileAttributes.Directory) != 0)
                {
                    throw new IOException(string.Empty, (int)IOException.IOExceptionErrorCode.UnauthorizedAccess);
                }

                if ((attributes & (byte)FileAttributes.ReadOnly) != 0)
                {
                    throw new IOException(string.Empty, (int)IOException.IOExceptionErrorCode.UnauthorizedAccess);
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
        /// <exception cref="IOException"><paramref name="path"/> cannot be not found.</exception>
        public static FileAttributes GetAttributes(string path)
        {
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
        /// Returns the date and time the specified file or directory was last written to.
        /// </summary>
        /// <param name="path">
        /// The file or directory for which to obtain write date and time information.
        /// </param>
        /// <returns>
        /// A <see cref="DateTime" /> structure set to the last write date and time for the specified file or directory.
        /// </returns>
        /// <exception cref="IOException"><paramref name="path"/> cannot be not found.</exception>
        public static DateTime GetLastWriteTime(string path)
        {
            if (!Exists(path))
            {
                throw new IOException(string.Empty, (int)IOException.IOExceptionErrorCode.FileNotFound);
            }

            return GetLastWriteTimeNative(path);
        }

        /// <summary>
        /// Moves a specified file to a new location, providing the option to specify a new file name.
        /// </summary>
        /// <param name="sourceFileName">The name of the file to move. Must be an absolute path.</param>
        /// <param name="destFileName">The new path and name for the file.</param>
        /// <exception cref="ArgumentException"><paramref name="sourceFileName"/> or <paramref name="destFileName"/> is null or empty.</exception>
        /// <exception cref="IOException"><paramref name="sourceFileName"/> does not exist or <paramref name="destFileName"/> exists.</exception>
        public static void Move(string sourceFileName, string destFileName)
        {
            if (string.IsNullOrEmpty(sourceFileName))
            {
                throw new ArgumentException();
            }

            if (string.IsNullOrEmpty(destFileName))
            {
                throw new ArgumentException();
            }

            if (!Exists(sourceFileName))
            {
                throw new IOException(string.Empty, (int)IOException.IOExceptionErrorCode.FileNotFound);
            }

            if (Exists(destFileName))
            {
                throw new IOException(string.Empty, (int)IOException.IOExceptionErrorCode.PathAlreadyExists);
            }

            if (sourceFileName == destFileName)
            {
                return;
            }

            // TODO: File Handling missing // <-- Leaving this here for not but not sure what it is indicating -- Cory

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
        /// Opens an existing file for reading.
        /// </summary>
        /// <param name="path">The file to be opened for reading.</param>
        /// <returns>A <see cref="FileStream"/> on the specified path.</returns>
        public static FileStream OpenRead(string path) => new(path, FileMode.Open, FileAccess.Read);

        /// <summary>
        /// Opens an existing UTF-8 encoded text file for reading.
        /// </summary>
        /// <param name="path">The file to be opened for reading.</param>
        /// <returns>A <see cref="StreamReader"/> on the specified path.</returns>
        public static StreamReader OpenText(string path) => new(new FileStream(path, FileMode.Open, FileAccess.Read));

        /// <summary>
        /// Opens an existing file or creates a new file for writing.
        /// </summary>
        /// <param name="path">The file to be opened for writing.</param>
        public static FileStream OpenWrite(string path) => new(path, FileMode.OpenOrCreate, FileAccess.Write);

        /// <summary>
        /// Opens a binary file, reads the contents of the file into a byte array, and then closes the file.
        /// </summary>
        /// <param name="path">The file to open for reading.</param>
        /// <exception cref="IOException">The end of the file was unexpectedly reached.</exception>
        public static byte[] ReadAllBytes(string path)
        {
            using var stream = OpenRead(path);

            var index = 0;
            var count = (int)stream.Length;
            var bytes = new byte[count];

            while (count > 0)
            {
                var read = stream.Read(bytes, index, count > ChunkSize ? ChunkSize : count);
                if (read <= 0)
                {
                    throw new IOException();
                }

                index += read;
                count -= read;
            }

            return bytes;
        }

        /// <summary>
        /// Opens a text file, reads all the text in the file, and then closes the file.
        /// </summary>
        /// <param name="path">The file to open for reading.</param>
        public static string ReadAllText(string path)
        {
            using var streamReader = OpenText(path);
            return streamReader.ReadToEnd();
        }

        /// <summary>
        /// Sets the specified <see cref="FileAttributes"/> of the file on the specified path.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <param name="fileAttributes">A bitwise combination of the enumeration values.</param>
        public static void SetAttributes(string path, FileAttributes fileAttributes)
        {
            if (!Exists(path))
            {
                throw new IOException(string.Empty, (int)IOException.IOExceptionErrorCode.FileNotFound);
            }

            SetAttributesNative(path, (byte)fileAttributes);
        }

        /// <summary>
        /// Creates a new file, writes the specified byte array to the file, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="path">The file to write to.</param>
        /// <param name="bytes">The bytes to write to the file.</param>
        public static void WriteAllBytes(string path, byte[] bytes)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException(nameof(path));
            }

            if (bytes is null)
            {
                throw new ArgumentException(nameof(bytes));
            }

            Create(path);

            if (bytes.Length <= 0)
            {
                return;
            }

            using var stream = new FileStream(path, FileMode.Open, FileAccess.Write);
            for (var bytesWritten = 0L; bytesWritten < bytes.Length;)
            {
                var bytesToWrite = bytes.Length - bytesWritten;
                bytesToWrite = bytesToWrite < ChunkSize ? bytesToWrite : ChunkSize;

                stream.Write(bytes, (int)bytesWritten, (int)bytesToWrite);
                stream.Flush();

                bytesWritten += bytesToWrite;
            }
        }

        /// <summary>
        /// Creates a new file, writes the specified string to the file, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="path">The file to write to.</param>
        /// <param name="contents">The string to write to the file.</param>
        public static void WriteAllText(string path, string contents) => WriteAllBytes(path, string.IsNullOrEmpty(contents) ? EmptyBytes : Encoding.UTF8.GetBytes(contents));

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
