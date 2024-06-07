//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System.Runtime.CompilerServices;

namespace System.IO
{
    /// <summary>
    /// Provides a Stream for a file, supporting both synchronous and asynchronous read and write operations.
    /// </summary>
    public class FileStream : Stream
    {
        #region backing fields

        private bool _canRead;
        private bool _canWrite;
        private bool _canSeek;

        private readonly long _seekLimit;

        private bool _disposed;

        private string _fileName;

        private NativeFileStream _nativeFileStream;
        private FileSystemManager.FileRecord _fileRecord;

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value that indicates whether the current stream supports reading.
        /// </summary>
        /// <value><see langword="true"/> if the stream supports reading; otherwise, <see langword="false"/>.</value>
        public override bool CanRead => _canRead;

        /// <summary>
        /// Gets a value that indicates whether the current stream supports seeking.
        /// </summary>
        /// <value><see langword="true"/> if the stream supports seeking; <see langword="false"/> if the stream is closed or if the <see cref="FileStream"/> was constructed from an operating-system handle such as a pipe or output to the console.</value>
        public override bool CanSeek => _canSeek;

        /// <summary>
        /// Gets a value that indicates whether the current stream supports writing.
        /// </summary>
        /// <value><see langword="true"/> if the stream supports writing; <see langword="false"/> if the stream is closed or was opened with read-only access.</value>
        public override bool CanWrite => _canWrite;

        /// <summary>
        /// Gets the length in bytes of the stream.
        /// </summary>
        /// <value>The length in bytes of the stream.</value>
        public override long Length
        {
            get
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException();
                }

                if (!_canSeek)
                {
                    throw new NotSupportedException();
                }

                return _nativeFileStream.GetLength();
            }
        }

        /// <summary>
        /// Gets or sets the current position of this stream.
        /// </summary>
        /// <value>The current position of this stream.</value>
        public override long Position
        {
            get
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException();
                }

                if (!_canSeek)
                {
                    throw new NotSupportedException();
                }

                // argument validation in interop layer
                return _nativeFileStream.Seek(
                    0,
                    (uint)SeekOrigin.Current);
            }

            set
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException();
                }

                if (!_canSeek)
                {
                    throw new NotSupportedException();
                }

                if (value < _seekLimit)
                {
                    throw new IOException();
                }

                // argument validation in interop layer
                _ = _nativeFileStream.Seek(
                    value,
                    (uint)SeekOrigin.Begin);
            }
        }

        /// <summary>
        /// Gets the absolute path of the file opened in the <see cref="FileStream"/>.
        /// </summary>
        /// <value>
        /// A string that is the absolute path of the file.
        /// </value>
        public string Name => _fileName;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FileStream"/> class with the specified path and creation mode.
        /// </summary>
        /// <param name="path">A relative or absolute path for the file that the current FileStream object will encapsulate.</param>
        /// <param name="mode">One of the enumeration values that determines how to open or create the file.</param>
        public FileStream(
            string path,
            FileMode mode)
            : this(
                  path,
                  mode,
                  (mode == FileMode.Append ? FileAccess.Write : FileAccess.ReadWrite),
                  FileShare.Read,
                  NativeFileStream.BufferSizeDefault)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileStream"/> class with the specified path, creation mode, and read/write permission.
        /// </summary>
        /// <param name="path">A relative or absolute path for the file that the current FileStream object will encapsulate.</param>
        /// <param name="mode">One of the enumeration values that determines how to open or create the file.</param>
        /// <param name="access">A bitwise combination of the enumeration values that determines how the file can be accessed by the <see cref="FileStream"/> object. This also determines the values returned by the <see cref="CanRead"/> and <see cref="CanWrite"/> properties of the <see cref="FileStream"/> object. <see cref="CanSeek"/> is <see langword="true"/> if path specifies a disk file.</param>
        public FileStream(
            string path,
            FileMode mode,
            FileAccess access)
         : this(
               path,
               mode,
               access,
               FileShare.Read,
               NativeFileStream.BufferSizeDefault)
        {
        }

        /// <summary>
        /// Initializes a new instance of the FileStream class with the specified path, creation mode, read/write permission, and sharing permission.
        /// </summary>
        /// <param name="path">A relative or absolute path for the file that the current FileStream object will encapsulate.</param>
        /// <param name="mode">One of the enumeration values that determines how to open or create the file.</param>
        /// <param name="access">A bitwise combination of the enumeration values that determines how the file can be accessed by the <see cref="FileStream"/> object. This also determines the values returned by the <see cref="CanRead"/> and <see cref="CanWrite"/> properties of the <see cref="FileStream"/> object. <see cref="CanSeek"/> is <see langword="true"/> if path specifies a disk file.</param>
        /// <param name="share">A bitwise combination of the enumeration values that determines how the file will be shared by processes.</param>
        public FileStream(
            string path,
            FileMode mode,
            FileAccess access,
            FileShare share)
           : this(
                 path,
                 mode,
                 access,
                 share,
                 NativeFileStream.BufferSizeDefault)
        {
        }

        /// <summary>
        /// Initializes a new instance of the FileStream class with the specified path, creation mode, read/write permission, and sharing permission.
        /// </summary>
        /// <param name="path">A relative or absolute path for the file that the current FileStream object will encapsulate.</param>
        /// <param name="mode">One of the enumeration values that determines how to open or create the file.</param>
        /// <param name="access">A bitwise combination of the enumeration values that determines how the file can be accessed by the <see cref="FileStream"/> object. This also determines the values returned by the <see cref="CanRead"/> and <see cref="CanWrite"/> properties of the <see cref="FileStream"/> object. <see cref="CanSeek"/> is <see langword="true"/> if path specifies a disk file.</param>
        /// <param name="share">A bitwise combination of the enumeration values that determines how the file will be shared by processes.</param>
        /// <param name="bufferSize">A positive <see cref="Int32"/> value greater than 0 indicating the buffer size. The default buffer size is 2048.</param>
        public FileStream(
            string path,
            FileMode mode,
            FileAccess access,
            FileShare share,
            int bufferSize)
        {
            // path validation happening in the call
            _fileName = Path.GetFullPath(path);

            // make sure mode, access, and share are within range
            if (mode < FileMode.CreateNew
                || mode > FileMode.Append
                || access < FileAccess.Read
                || access > FileAccess.ReadWrite
                || share < FileShare.None
                || share > FileShare.ReadWrite)
            {
                throw new ArgumentOutOfRangeException();
            }

            // Get wantsRead and wantsWrite from access, note that they cannot both be false
            bool wantsRead = (access & FileAccess.Read) == FileAccess.Read;
            bool wantsWrite = (access & FileAccess.Write) == FileAccess.Write;

            // You can't open for readonly access (wantsWrite == false) when
            // mode is CreateNew, Create, Truncate or Append (when it's not Open or OpenOrCreate)
            if (mode != FileMode.Open
                && mode != FileMode.OpenOrCreate
                && !wantsWrite)
            {
                throw new ArgumentException();
            }

            RegisterShareInformation(access, share);

            try
            {
                uint attributes = NativeIO.GetAttributes(_fileName);
                bool exists = attributes != NativeIO.EmptyAttribute;
                bool isReadOnly = exists && (((FileAttributes)attributes) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;

                // if the path specified is an existing directory, fail
                if (exists && ((((FileAttributes)attributes)
                    & FileAttributes.Directory) == FileAttributes.Directory))
                {
                    throw new IOException(
                        string.Empty,
                        (int)IOException.IOExceptionErrorCode.UnauthorizedAccess);
                }

                // The seek limit is 0 (the beginning of the file) for all modes except Append
                _seekLimit = 0;

                switch (mode)
                {
                    case FileMode.CreateNew:
                        CreateNewFile(exists, bufferSize);
                        break;

                    case FileMode.Create:
                        CreateFile(exists, bufferSize);
                        break;

                    case FileMode.Open:
                        OpenFile(exists, bufferSize);
                        break;

                    case FileMode.OpenOrCreate:
                        OpenOrCreateFile(bufferSize);
                        break;

                    case FileMode.Truncate:
                        TruncateFile(exists, bufferSize);
                        break;

                    case FileMode.Append:
                        AppendToFile(access, bufferSize);
                        break;

                    default:
                        throw new ArgumentException("");
                }

                // Now that we have a valid NativeFileStream, we add it to the FileRecord, so it can get cleaned-up in case an eject or force format
                _fileRecord.NativeFileStream = _nativeFileStream;

                AdjustCapabilities(
                    wantsRead,
                    wantsWrite,
                    isReadOnly);
            }
            catch
            {
                // something went wrong, clean up and re-throw the exception
                _nativeFileStream?.Close();

                FileSystemManager.RemoveFromOpenList(_fileRecord);

                throw;
            }
        }

        /// <inheritdoc/>
        ~FileStream()
        {
            Dispose(false);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Closes the current stream and releases any resources associated with the current stream.
        /// </summary>
        public override void Close()
        {
            Dispose(true);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the FileStream and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        _canRead = false;
                        _canWrite = false;
                        _canSeek = false;
                    }

                    if (_nativeFileStream != null)
                    {
                        _nativeFileStream.Close();
                    }
                }
                finally
                {
                    if (_fileRecord != null)
                    {
                        FileSystemManager.RemoveFromOpenList(_fileRecord);
                        _fileRecord = null;
                    }

                    _nativeFileStream = null;
                    _disposed = true;
                }
            }
        }

        // This is for internal use to support proper atomic CopyAndDelete
        internal void DisposeAndDelete()
        {
            _nativeFileStream.Close();

            // need to null this so Dispose(true) won't close the stream again
            _nativeFileStream = null;

            NativeIO.Delete(
                _fileName,
                false);

            Dispose(true);
        }

        /// <summary>
        /// Clears buffers for this stream and causes any buffered data to be written to the file.
        /// </summary>
        public override void Flush()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException();
            }

            _nativeFileStream.Flush();
        }

        /// <summary>
        /// Reads a block of bytes from the stream and writes the data in a given buffer.
        /// </summary>
        /// <param name="buffer">When this method returns, contains the specified byte array with the values between <paramref name="offset"/> and (<paramref name="offset"/> + <paramref name="count"/> - 1) replaced by the bytes read from the current source.</param>
        /// <param name="offset">The byte offset in array at which the read bytes will be placed.</param>
        /// <param name="count">The maximum number of bytes to read.</param>
        /// <returns>The total number of bytes read into the buffer. This might be less than the number of bytes requested if that number of bytes are not currently available, or zero if the end of the stream is reached.</returns>
        public override int Read(
            byte[] buffer,
            int offset,
            int count)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException();
            }

            if (!_canRead)
            {
                throw new NotSupportedException();
            }

            lock (_nativeFileStream)
            {
                // argument validation in interop layer
                return _nativeFileStream.Read(
                    buffer,
                    offset,
                    count,
                    NativeFileStream.TimeoutDefault);
            }
        }

        /// <summary>
        /// Reads a byte from the file and advances the read position one byte.
        /// </summary>
        /// <returns>The byte, cast to an <see cref="int"/>, or -1 if the end of the stream has been reached.</returns>
        public override int ReadByte()
        {
            byte[] resByte = new byte[1];

            int count = Read(resByte, 0, 1);

            if (count != 1)
            {
                // End of File
                return -1;
            }

            return resByte[0];
        }

        /// <summary>
        /// Sets the current position of this stream to the given value.
        /// </summary>
        /// <param name="offset">The point relative to origin from which to begin seeking.</param>
        /// <param name="origin">Specifies the beginning, the end, or the current position as a reference point for <paramref name="offset"/>, using a value of type <see cref="SeekOrigin"/>.</param>
        /// <returns>The new position in the stream.</returns>
        public override long Seek(
            long offset,
            SeekOrigin origin)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException();
            }

            if (!_canSeek)
            {
                throw new NotSupportedException();
            }

            long oldPosition = Position;

            long newPosition = _nativeFileStream.Seek(
                offset,
                (uint)origin);

            if (newPosition < _seekLimit)
            {
                Position = oldPosition;

                throw new IOException();
            }

            return newPosition;
        }

        /// <summary>
        /// Sets the length of this stream to the given value.
        /// </summary>
        /// <param name="value">The new length of the stream.</param>
        public override void SetLength(long value)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException();
            }

            if (!_canWrite
                || !_canSeek)
            {
                throw new NotSupportedException();
            }

            // argument validation in interop layer
            _nativeFileStream.SetLength(value);
        }

        /// <summary>
        /// Writes a block of bytes to the file stream.
        /// </summary>
        /// <param name="buffer">The buffer containing data to write to the stream.</param>
        /// <param name="offset">The zero-based byte offset in array from which to begin copying bytes to the stream.</param>
        /// <param name="count">The maximum number of bytes to write.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException();
            }

            if (!_canWrite)
            {
                throw new NotSupportedException();
            }

            // argument validation in interop layer
            int bytesWritten;

            lock (_nativeFileStream)
            {
                // we check for count being != 0 because we want to handle negative cases as well in the interop layer
                while (count != 0)
                {
                    bytesWritten = _nativeFileStream.Write(
                        buffer,
                        offset,
                        count,
                        NativeFileStream.TimeoutDefault);

                    if (bytesWritten == 0)
                    {
                        throw new IOException();
                    }

                    offset += bytesWritten;
                    count -= bytesWritten;
                }
            }
        }

        /// <summary>
        /// Writes a byte to the current position in the file stream.
        /// </summary>
        /// <param name="value">A byte to write to the stream.</param>
        public override void WriteByte(byte value)
        {
            byte[] wrByte = new byte[] { value };

            Write(wrByte, 0, 1);
        }

        #endregion

        private void AdjustCapabilities(bool wantsRead, bool wantsWrite, bool isReadOnly)
        {
            // Retrive the filesystem capabilities
            _nativeFileStream.GetStreamProperties(
                out _canRead,
                out _canWrite,
                out _canSeek);

            // Ii the file is readonly, regardless of the filesystem capability, we'll turn off write
            if (isReadOnly)
            {
                _canWrite = false;
            }

            // Make sure the requests (wantsRead / wantsWrite) matches the filesystem capabilities (canRead / canWrite)
            if ((wantsRead
                 && !_canRead) || (wantsWrite
                                   && !_canWrite))
            {
                throw new IOException(
                    string.Empty,
                    (int)IOException.IOExceptionErrorCode.UnauthorizedAccess);
            }

            // finally, adjust the _canRead / _canWrite to match the requests
            if (!wantsWrite)
            {
                _canWrite = false;
            }
            else if (!wantsRead)
            {
                _canRead = false;
            }
        }

        private void RegisterShareInformation(
            FileAccess access,
            FileShare share)
        {
            _fileRecord = FileSystemManager.AddToOpenList(
                _fileName,
                (int)access,
                (int)share);
        }

        private void CreateNewFile(
            bool exists,
            int bufferSize)
        {
            // if the file exists, IOException is thrown
            if (exists)
            {
                throw new IOException(
                    string.Empty,
                    (int)IOException.IOExceptionErrorCode.PathAlreadyExists);
            }

            _nativeFileStream = new NativeFileStream(
                _fileName,
                bufferSize);
        }

        private void CreateFile(
            bool exists,
            int bufferSize)
        {
            // if the file exists, it should be overwritten
            _nativeFileStream = new NativeFileStream(
                _fileName,
                bufferSize);

            if (exists)
            {
                _nativeFileStream.SetLength(0);
            }
        }

        private void OpenFile(
            bool exists,
            int bufferSize)
        {
            // if the file does not exist, IOException/FileNotFound is thrown
            if (!exists)
            {
                throw new IOException(
                    string.Empty,
                    (int)IOException.IOExceptionErrorCode.FileNotFound);
            }

            _nativeFileStream = new NativeFileStream(
                _fileName,
                bufferSize);
        }

        private void OpenOrCreateFile(int bufferSize)
        {
            // if the file does not exist, it is created
            _nativeFileStream = new NativeFileStream(
                _fileName,
                bufferSize);
        }

        private void TruncateFile(
            bool exists,
            int bufferSize)
        {
            // the file would be overwritten. if the file does not exist, IOException/FileNotFound is thrown
            if (!exists)
            {
                throw new IOException(
                    string.Empty,
                    (int)IOException.IOExceptionErrorCode.FileNotFound);
            }

            _nativeFileStream = new NativeFileStream(
                _fileName,
                bufferSize);

            _nativeFileStream.SetLength(0);
        }

        private void AppendToFile(
            FileAccess access,
            int bufferSize)
        {
            // Opens the file if it exists and seeks to the end of the file. Append can only be used in conjunction with FileAccess.Write
            // Attempting to seek to a position before the end of the file will throw an IOException and any attempt to read fails and throws an NotSupportedException
            if (access != FileAccess.Write)
            {
                throw new ArgumentException("");
            }

            _nativeFileStream = new NativeFileStream(
                _fileName,
                bufferSize);

            _seekLimit = _nativeFileStream.Seek(
                0,
                (uint)SeekOrigin.End);
        }
    }
}
