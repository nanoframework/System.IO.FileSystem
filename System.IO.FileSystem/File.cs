//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System.Text;

namespace System.IO
{
    /// <summary>
    /// Provides static methods for the creation, copying, deletion, moving, and opening of a single file, and aids in the creation of <see cref="FileStream"/> objects.
    /// </summary>
    public static class File
    {
        private const int ChunkSize = 2048;
        private static readonly byte[] EmptyBytes = new byte[0];

        /// <summary>
        /// Creates a <see cref="StreamWriter"/> that appends UTF-8 encoded text to an existing file, or to a new file if the specified file does not exist.
        /// </summary>
        /// <param name="path">The path to the file to append to.</param>
        /// <returns>A stream writer that appends UTF-8 encoded text to the specified file or to a new file.</returns>
        public static StreamWriter AppendText(string path)
        {
            // path validation happening in the call
            path = Path.GetFullPath(path);

            return new StreamWriter(OpenWrite(path));
        }

        /// <summary>
        /// Opens a file, appends the specified string to the file, and then closes the file. If the file does not exist, this method creates a file, writes the specified string to the file, then closes the file.
        /// </summary>
        /// <param name="path">The file to append the specified string to.</param>
        /// <param name="contents">The string to append to the file.</param>
        public static void AppendAllText(
            string path,
            string contents)
        {
            // path validation happening in the call
            path = Path.GetFullPath(path);

            using var stream = new FileStream(
                path,
                FileMode.Append,
                FileAccess.Write);

            using var streamWriter = new StreamWriter(stream);

            streamWriter.Write(contents);
        }

        /// <summary>
        /// Copies an existing file to a new file. Overwriting a file of the same name is not allowed.
        /// </summary>
        /// <param name="sourceFileName">The file to copy.</param>
        /// <param name="destFileName">The name of the destination file. This cannot be a directory or an existing file.</param>
        /// <exception cref="ArgumentException"><paramref name="sourceFileName"/> or <paramref name="destFileName"/> is <see langword="null"/> or empty.</exception>
        public static void Copy(
            string sourceFileName,
            string destFileName)
        {
            Copy(
                sourceFileName,
                destFileName,
                false,
                false);
        }

        /// <summary>
        /// Copies an existing file to a new file. Overwriting a file of the same name is allowed.
        /// </summary>
        /// <param name="sourceFileName">The file to copy.</param>
        /// <param name="destFileName">The name of the destination file. This cannot be a directory.</param>
        /// <param name="overwrite"><see langword="true"/> if the destination file can be overwritten; otherwise, <see langword="false"/>.</param>
        /// <exception cref="ArgumentException"><paramref name="sourceFileName"/> or <paramref name="destFileName"/> is <see langword="null"/> or empty.</exception>
        public static void Copy(
            string sourceFileName,
            string destFileName,
            bool overwrite)
        {
            Copy(
                sourceFileName,
                destFileName,
                overwrite,
                false);
        }

        /// <summary>
        /// Creates, or truncates and overwrites, a file in the specified path.
        /// </summary>
        /// <param name="path">The path and name of the file to create.</param>
        /// <returns>A <see cref="FileStream"/> that provides read/write access to the file specified in <paramref name="path"/>.</returns>
        public static FileStream Create(string path) => new FileStream(
            path,
            FileMode.Create,
            FileAccess.ReadWrite,
            FileShare.None,
            NativeFileStream.BufferSizeDefault);

        /// <summary>
        /// Creates or opens a file for writing UTF-8 encoded text.
        /// </summary>
        /// <param name="path">The path and name of the file to create.</param>
        /// <param name="bufferSize">The number of bytes buffered for reads and writes to the file.</param>
        /// <returns>A <see cref="FileStream"/> that provides read/write access to the file specified in <paramref name="path"/>.</returns>
        public static FileStream Create(
            string path,
            int bufferSize) => new FileStream(
                path,
                FileMode.Create,
                FileAccess.ReadWrite,
                FileShare.None,
                bufferSize);

        /// <summary>
        /// Deletes the specified file.
        /// </summary>
        /// <param name="path">The name of the file to be deleted. Wildcard characters are not supported.</param>
        /// <exception cref="ArgumentException"><paramref name="path"/> is <see langword="null"/> or empty.</exception>
        /// <exception cref="IOException">Directory is not found or <paramref name="path"/> is read-only or a directory.</exception>
        public static void Delete(string path)
        {
            // path validation happening in the call
            path = Path.GetFullPath(path);
            string folderPath = Path.GetDirectoryName(path);

            // make sure no one else has the file opened, and no one else can modify it when we're deleting
            object record = FileSystemManager.AddToOpenList(path);

            try
            {
                uint attributes = NativeIO.GetAttributes(folderPath);

                // in case the folder does not exist or is invalid we throw DirNotFound Exception
                if (attributes == NativeIO.EmptyAttribute)
                {
                    throw new IOException(
                        string.Empty,
                        (int)IOException.IOExceptionErrorCode.DirectoryNotFound);
                }

                // folder exists, lets verify whether the file itself exists
                attributes = NativeIO.GetAttributes(path);
                if (attributes == NativeIO.EmptyAttribute)
                {
                    // No-op on file not found
                    return;
                }

                if ((attributes
                     & (uint)(FileAttributes.Directory | FileAttributes.ReadOnly)) != 0)
                {
                    // it's a readonly file or a directory
                    throw new IOException(
                        string.Empty,
                        (int)IOException.IOExceptionErrorCode.UnauthorizedAccess);
                }

                NativeIO.Delete(
                    path,
                    false);
            }
            finally
            {
                // regardless of what happened, we need to release the file when we're done
                FileSystemManager.RemoveFromOpenList(record);
            }
        }

        /// <summary>
        /// Determines whether the specified file exists.
        /// </summary>
        /// <param name="path">The file to check.</param>
        /// <returns><see langword="true"/> if the caller has the required permissions and <paramref name="path"/> contains the name of an existing file; otherwise, <see langword="false"/>. This method also returns <see langword="false"/> if <paramref name="path"/> is <see langword="null"/>, an invalid <paramref name="path"/>, or a zero-length string. If the caller does not have sufficient permissions to read the specified file, no exception is thrown and the method returns <see langword="false"/> regardless of the existence of <paramref name="path"/>.</returns>
        public static bool Exists(string path)
        {
            try
            {
                // path validation happening in the call
                path = Path.GetFullPath(path);

                // Is this the absolute root? this is not a file.
                string root = Path.GetPathRoot(path);

                if (string.Equals(root, path))
                {
                    return false;
                }

                uint attributes = NativeIO.GetAttributes(path);

                if (attributes == NativeIO.EmptyAttribute)
                {
                    // this means not found
                    return false;
                }

                if ((attributes
                        & (uint)FileAttributes.Directory) == 0)
                {
                    // not a directory, it must be a file.
                    return true;
                }
            }
            catch
            {
                // like the full .NET this does not throw exception in a number of cases, instead returns false.
            }

            return false;
        }

        /// <summary>
        /// Gets the <see cref="FileAttributes"/> of the file on the path.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <exception cref="IOException"><paramref name="path"/> cannot be not found.</exception>
        public static FileAttributes GetAttributes(string path)
        {
            // path validation happening in the call
            string fullPath = Path.GetFullPath(path);

            uint attributes = NativeIO.GetAttributes(fullPath);

            if (attributes == NativeIO.EmptyAttribute)
            {
                throw new IOException(
                    string.Empty,
                    (int)IOException.IOExceptionErrorCode.FileNotFound);
            }
            else if (attributes == 0x0)
            {
                return FileAttributes.Normal;
            }
            else
            {
                return (FileAttributes)attributes;
            }
        }

        /// <summary>
        /// Moves a specified file to a new location, providing the option to specify a new file name.
        /// </summary>
        /// <param name="sourceFileName">The name of the file to move. Must be an absolute path.</param>
        /// <param name="destFileName">The new path and name for the file.</param>
        /// <exception cref="ArgumentException"><paramref name="sourceFileName"/> or <paramref name="destFileName"/> is <see langword="null"/> or empty.</exception>

        /// <exception cref="IOException"><paramref name="sourceFileName"/> does not exist or <paramref name="destFileName"/> exists.</exception>
        /// <remarks>
        /// .NET nanoFramework implementation differs from the full framework as it requires that <paramref name="sourceFileName"/> be an absolute path. This is a limitation coming from the platform.
        /// </remarks>
        public static void Move(string sourceFileName, string destFileName)
        {
            // sourceFileName and destFileName validation happening in the call
            sourceFileName = Path.GetFullPath(sourceFileName);
            destFileName = Path.GetFullPath(destFileName);

            bool tryCopyAndDelete = false;

            // We only need to lock the source, not the dest because if dest is taken
            // Move() will failed at the driver's level anyway. (there will be no conflict even if
            // another thread is creating dest, as only one of the operations will succeed --
            // the native calls are atomic)
            object srcRecord = FileSystemManager.AddToOpenList(sourceFileName);

            try
            {
                if (!Exists(sourceFileName))
                {
                    throw new IOException(
                        string.Empty,
                        (int)IOException.IOExceptionErrorCode.FileNotFound);
                }

                tryCopyAndDelete = NativeIO.Move(
                    sourceFileName,
                    destFileName);
            }
            finally
            {
                FileSystemManager.RemoveFromOpenList(srcRecord);
            }

            if (tryCopyAndDelete)
            {
                Copy(
                    sourceFileName,
                    destFileName,
                    false,
                    true);
            }
        }

        /// <summary>
        /// Opens a <see cref="FileStream"/> on the specified path with read/write access with no sharing.
        /// </summary>
        /// <param name="path">The file to open.</param>
        /// <param name="mode">A <see cref="FileMode"/> value that specifies whether a file is created if one does not exist, and determines whether the contents of existing files are retained or overwritten.</param>
        /// <returns>A <see cref="FileStream"/> opened in the specified mode and path, with read/write access and not shared.</returns>
        public static FileStream Open(
            string path,
            FileMode mode)
        {
            return new FileStream(
                path,
                mode,
                (mode == FileMode.Append ? FileAccess.Write : FileAccess.ReadWrite),
                FileShare.None,
                NativeFileStream.BufferSizeDefault);
        }

        /// <summary>
        /// Opens a FileStream on the specified path, with the specified mode and access with no sharing.
        /// </summary>
        /// <param name="path">The file to open.</param>
        /// <param name="mode">A <see cref="FileMode"/> value that specifies whether a file is created if one does not exist, and determines whether the contents of existing files are retained or overwritten.</param>
        /// <param name="access">A <see cref="FileAccess"/> value that specifies the operations that can be performed on the file.</param>
        /// <returns>An unshared <see cref="FileStream"/> that provides access to the specified file, with the specified mode and access.</returns>
        public static FileStream Open(
            string path,
            FileMode mode,
            FileAccess access)
        {
            return new FileStream(
                path,
                mode,
                access,
                FileShare.None,
                NativeFileStream.BufferSizeDefault);
        }

        /// <summary>
        /// Opens a FileStream on the specified path, having the specified mode with read, write, or read/write access and the specified sharing option.
        /// </summary>
        /// <param name="path">The file to open.</param>
        /// <param name="mode">A <see cref="FileMode"/> value that specifies whether a file is created if one does not exist, and determines whether the contents of existing files are retained or overwritten.</param>
        /// <param name="access">A <see cref="FileAccess"/> value that specifies the operations that can be performed on the file.</param>
        /// <param name="share">A FileShare value specifying the type of access other threads have to the file.</param>
        /// <returns>A <see cref="FileStream"/> on the specified path, having the specified mode with read, write, or read/write access and the specified sharing option.</returns>
        public static FileStream Open(
            string path,
            FileMode mode,
            FileAccess access,
            FileShare share)
        {
            return new FileStream(
                path,
                mode,
                access,
                share,
                NativeFileStream.BufferSizeDefault);
        }

        /// <summary>
        /// Opens an existing file for reading.
        /// </summary>
        /// <param name="path">The file to be opened for reading.</param>
        /// <returns>A <see cref="FileStream"/> on the specified path.</returns>
        public static FileStream OpenRead(string path) => new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            NativeFileStream.BufferSizeDefault);

        /// <summary>
        /// Opens an existing UTF-8 encoded text file for reading.
        /// </summary>
        /// <param name="path">The file to be opened for reading.</param>
        /// <returns>A <see cref="StreamReader"/> on the specified path.</returns>
        public static StreamReader OpenText(string path)
        {
            // path validation happening in the call
            path = Path.GetFullPath(path);

            return new StreamReader(OpenRead(path));
        }

        /// <summary>
        /// Opens an existing file or creates a new file for writing.
        /// </summary>
        /// <param name="path">The file to be opened for writing.</param>
        public static FileStream OpenWrite(string path) => new FileStream(
            path,
            FileMode.OpenOrCreate,
            FileAccess.Write,
            NativeFileStream.BufferSizeDefault);

        /// <summary>
        /// Opens a binary file, reads the contents of the file into a byte array, and then closes the file.
        /// </summary>
        /// <param name="path">The file to open for reading.</param>
        /// <exception cref="IOException">The end of the file was unexpectedly reached.</exception>
        public static byte[] ReadAllBytes(string path)
        {
            byte[] bytes;

            using var fs = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                NativeFileStream.BufferSizeDefault);

            // blocking read
            int index = 0;
            long fileLength = fs.Length;

            if (fileLength > int.MaxValue)
            {
                throw new IOException();
            }

            int count = (int)fileLength;
            bytes = new byte[count];

            while (count > 0)
            {
                int n = fs.Read(
                    bytes,
                    index,
                    count);

                index += n;
                count -= n;
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
                throw new IOException(
                    string.Empty,
                    (int)IOException.IOExceptionErrorCode.FileNotFound);
            }

            NativeIO.SetAttributes(path, (byte)fileAttributes);
        }

        /// <summary>
        /// Creates a new file, writes the specified byte array to the file, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="path">The file to write to.</param>
        /// <param name="bytes">The bytes to write to the file.</param>
        public static void WriteAllBytes(string path, byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException();
            }

            using var fs = new FileStream(
                path,
                FileMode.Create,
                FileAccess.Write,
                FileShare.Read,
                NativeFileStream.BufferSizeDefault);

            fs.Write(
                bytes,
                0,
                bytes.Length);
        }

        /// <summary>
        /// Creates a new file, writes the specified string to the file, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="path">The file to write to.</param>
        /// <param name="contents">The string to write to the file.</param>
        public static void WriteAllText(
            string path,
            string contents) => WriteAllBytes(
                path,
                string.IsNullOrEmpty(contents) ? EmptyBytes : Encoding.UTF8.GetBytes(contents));

        internal static void Copy(
            string sourceFileName,
            string destFileName,
            bool overwrite,
            bool deleteOriginal)
        {
            // sourceFileName and destFileName validation happening in the call

            sourceFileName = Path.GetFullPath(sourceFileName);
            destFileName = Path.GetFullPath(destFileName);

            FileMode writerMode = (overwrite) ? FileMode.Create : FileMode.CreateNew;

            FileStream reader = new FileStream(
                sourceFileName,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                NativeFileStream.BufferSizeDefault);

            try
            {
                using FileStream writer = new FileStream(
                    destFileName,
                    writerMode,
                    FileAccess.Write,
                    FileShare.None,
                    NativeFileStream.BufferSizeDefault);

                long fileLength = reader.Length;

                writer.SetLength(fileLength);

                byte[] buffer = new byte[ChunkSize];

                for (; ; )
                {
                    int readSize = reader.Read(buffer, 0, ChunkSize);

                    if (readSize <= 0)
                    {
                        break;
                    }

                    writer.Write(buffer, 0, readSize);
                }

                // copy the attributes too
                NativeIO.SetAttributes(
                    destFileName,
                    NativeIO.GetAttributes(sourceFileName));
            }
            finally
            {
                if (deleteOriginal)
                {
                    reader.DisposeAndDelete();
                }
                else
                {
                    reader.Dispose();
                }
            }
        }
    }
}
