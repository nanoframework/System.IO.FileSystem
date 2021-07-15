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
        #region Variables

        private bool _canRead;
        private bool _canWrite;
        private bool _canSeek;

        private readonly long _seekLimit;
        private long _position;

        private bool _disposed;

        private readonly string _name;
        private readonly string _path;

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value that indicates whether the current stream supports reading.
        /// </summary>
        public override bool CanRead
        {
            get { return _canRead; }
        }

        /// <summary>
        /// Gets a value that indicates whether the current stream supports seeking.
        /// </summary>
        public override bool CanSeek
        {
            get { return _canSeek; }
        }

        /// <summary>
        /// Gets a value that indicates whether the current stream supports writing.
        /// </summary>
        public override bool CanWrite
        {
            get { return _canWrite; }
        }

        /// <summary>
        /// Gets the length in bytes of the stream.
        /// </summary>
        public override long Length
        {
            get
            {               
                if (_disposed)
                {
                    throw new ObjectDisposedException();
                }

                return GetLengthNative(FilePath, Name);
            }
        }

        /// <summary>
        /// Gets or sets the current position of this stream.
        /// </summary>
        public override long Position
        {
            get
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException();
                }

                return !_canSeek ? throw new NotSupportedException() : _position;
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
                    throw new ArgumentException("Can't set Position below SeekLimit.");
                }

                if (value > Length)
                {
                    throw new ArgumentException("Can't set Position beyond end of File.");
                }

                _position = value;
            }
        }

        /// <summary>
        /// Gets the name of the file including the file name extension.
        /// </summary>
        /// <value>
        /// The name of the file including the file name extension.
        /// </value>
        public string Name => _name;

        /// <summary>
        /// Gets the full file-system path of the current file, if the file has a path.
        /// </summary>
        public string FilePath => _path;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the FileStream class with the specified path and creation mode.
        /// </summary>
        /// <param name="path">A relative or absolute path for the file that the current FileStream object will encapsulate.</param>
        /// <param name="mode">One of the enumeration values that determines how to open or create the file.</param>
        public FileStream(
            string path,
            FileMode mode)
            : this(
                  path,
                  mode,
                  (mode == FileMode.Append ? FileAccess.Write : FileAccess.ReadWrite))
        {
        }

        /// <summary>
        /// Initializes a new instance of the FileStream class with the specified path, creation mode, and read/write permission.
        /// </summary>
        /// <param name="path">A relative or absolute path for the file that the current FileStream object will encapsulate.</param>
        /// <param name="mode">One of the enumeration values that determines how to open or create the file.</param>
        /// <param name="access">A bitwise combination of the enumeration values that determines how the file can be accessed by the FileStream object. This also determines the values returned by the CanRead and CanWrite properties of the FileStream object.</param>
        public FileStream(
            string path,
            FileMode mode,
            FileAccess access)
        {
            // Get wantsRead and wantsWrite from access, note that they cannot both be false
            bool wantsRead = (access & FileAccess.Read) == FileAccess.Read;
            bool wantsWrite = (access & FileAccess.Write) == FileAccess.Write;

            // You can't open for readonly access (wantsWrite == false) when
            // mode is CreateNew, Create, Truncate or Append (when it's not Open or OpenOrCreate)
            if (mode != FileMode.Open
                && mode != FileMode.OpenOrCreate
                && !wantsWrite)
            {
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one 
                throw new ArgumentException();
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one 
            }

            // Set File Name & Path
            _name = Path.GetFileName(path);
            _path = Path.GetDirectoryName(path);

            // TODO: Get Readonly status from File? Necessary?
            bool exists = File.Exists(path);
            bool isReadOnly = exists && ((File.GetAttributes(path)) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;

            // The seek limit is 0 (the beginning of the file) for all modes except Append
            _seekLimit = 0;

            // Set actual position to Start of File
            _position = 0;

            switch (mode)
            {
                case FileMode.CreateNew:
                    // if the file exists, IOException is thrown
                    if (exists)
                    {
                        throw new IOException("File already exists.", (int)IOException.IOExceptionErrorCode.PathAlreadyExists);
                    }

                    OpenFileNative(FilePath, Name, (int) mode);
                    break;

                case FileMode.Create:
                    // if the file exists, it should be overwritten
                    OpenFileNative(FilePath, Name, (int)mode);
                    
                    break;

                case FileMode.Open: 
                    // if the file does not exist, IOException/FileNotFound is thrown
                    if (!exists)
                    {
                        throw new IOException("File not found/exists.", (int)IOException.IOExceptionErrorCode.FileNotFound);
                    }
                    
                    OpenFileNative(FilePath, Name, (int)mode);
                    
                    break;

                case FileMode.OpenOrCreate: 
                    // if the file does not exist, it is created
                    OpenFileNative(FilePath, Name, (int)mode);
                    
                    break;

                case FileMode.Truncate:
                    // the file would be overwritten. if the file does not exist, IOException/FileNotFound is thrown
                    if (!exists)
                    {
                        throw new IOException("File not found/existing.", (int)IOException.IOExceptionErrorCode.FileNotFound);
                    }

                    OpenFileNative(FilePath, Name, (int)mode);
                    
                    break;

                case FileMode.Append:
                    // Opens the file if it exists and seeks to the end of the file. Append can only be used in conjunction with FileAccess.Write
                    // Attempting to seek to a position before the end of the file will throw an IOException and any attempt to read fails and throws an NotSupportedException
                    if (access != FileAccess.Write)
                    {
                        throw new ArgumentException("No Write Access to file.");
                    }

                    OpenFileNative(FilePath, Name, (int)mode);
                   
                    _position = Length;
                    _seekLimit = Length;
                    
                    break;

                default:
                    throw new ArgumentException("FileMode not known.");
            }

            switch(access)
            {
                case FileAccess.Read:
                    _canRead = true;
                    _canSeek = true;
                    break;

                case FileAccess.Write:
                    _canWrite = true;
                    _canSeek = true;
                    break;

                case FileAccess.ReadWrite:
                    _canRead = true;
                    _canWrite = true;
                    _canSeek = true;
                    break;

                default:
#pragma warning disable S112 // General exceptions should never be thrown
                    throw new Exception("FileAccess not known.");
#pragma warning restore S112 // General exceptions should never be thrown
            }

            // If the file is ReadOnly, regardless of the file system capability, we'll turn off write
            if (isReadOnly)
            {
                _canWrite = false;
            }

            // Make sure the requests (wantsRead / wantsWrite) matches the file system capabilities (canRead / canWrite)
            if ((wantsRead && !_canRead)
                || (wantsWrite && !_canWrite))
            {
                throw new IOException("", (int)IOException.IOExceptionErrorCode.UnauthorizedAccess);
            }

            // Finally, adjust the _canRead / _canWrite to match the requests
            if (!wantsWrite)
            {
                _canWrite = false;
            }
            else if (!wantsRead)
            {
                _canRead = false;
            }
        }
        
        /// <summary>
        /// Destructor
        /// </summary>
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
                }
                finally
                {
                    _disposed = true;
                }
            }
        }

        /// <summary>
        /// Clears buffers for this stream and causes any buffered data to be written to the file.
        /// </summary>
        public override void Flush()
        {
            // Already everything flushed/sync after every Read/Write operation, nothing to do here.
        }

        /// <summary>
        /// Reads a block of bytes from the stream and writes the data in a given buffer.
        /// </summary>
        /// <param name="buffer">When this method returns, contains the specified byte array with the values between offset and (offset + count - 1) replaced by the bytes read from the current source.</param>
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

            //Checks
            if (offset > buffer.Length)
            {
                throw new IndexOutOfRangeException("Offset is outside of buffer size.");
            }

            if (buffer.Length < offset + count)
            {
                throw new IndexOutOfRangeException("Buffer size is smaller then offset + byteCount.");
            }

            // Create buffer for read Data
            byte[] readedBuffer = new byte[count];

            int readedCount = ReadNative(
                FilePath,
                Name,
                Position,
                readedBuffer,
                count);

            // Copy Data into source Buffer
            Array.Copy(
                readedBuffer,
                0,
                buffer,
                offset,
                count);

            _position += readedCount;     // Adapt new actual position

            return readedCount;
        }

        /// <summary>
        /// Reads a byte from the file and advances the read position one byte.
        /// </summary>
        /// <returns></returns>
        public override int ReadByte()
        {
            byte[] resByte = new byte[1];

            int count = Read(resByte, 0, 1);

            if (count != 1)
                return -1;  // End of File

            return resByte[0];
        }

        /// <summary>
        /// Sets the current position of this stream to the given value.
        /// </summary>
        /// <param name="offset">The point relative to origin from which to begin seeking.</param>
        /// <param name="origin">Specifies the beginning, the end, or the current position as a reference point for offset, using a value of type SeekOrigin.</param>
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

            long newPosition;

            switch(origin)
            {
                case SeekOrigin.Begin:
                    newPosition = 0 + offset;
                    break;

                case SeekOrigin.Current:
                    newPosition = Position + offset;
                    break;

                case SeekOrigin.End:
                    newPosition = Length + offset;
                    break;

                default:
                    throw new NotSupportedException("SeekOrigin (" + origin.ToString() + ") not supported.");
            }

            // Try to set new Position
            Position = newPosition;

            return newPosition;
        }

        /// <summary>
        /// Sets the length of this stream to the given value.
        /// </summary>
        /// <param name="value">The new length of the stream.</param>
        public override void SetLength(long value)
        {
            throw new NotImplementedException();
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

            //Checks
            if (offset > buffer.Length)
            {
                throw new IndexOutOfRangeException("Offset is outside of buffer size.");
            }

            if (buffer.Length < offset + count)
            {
                throw new IndexOutOfRangeException("Buffer size is smaller then offset + byteCount.");
            }

            byte[] bufferToWrite = new byte[count];

            Array.Copy(
                buffer,
                offset,
                bufferToWrite,
                0,
                count);

            WriteNative(
                FilePath,
                Name,
                Position,
                bufferToWrite,
                count);

            _position += count;     // Adapt new actual position
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

        #region Stubs (Native Calls)

        [Diagnostics.DebuggerStepThrough]
        [Diagnostics.DebuggerHidden]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void OpenFileNative(string path, string fileName, int fileMode);

        [Diagnostics.DebuggerStepThrough]
        [Diagnostics.DebuggerHidden]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern int ReadNative(string path, string fileName, long actualPosition, byte[] buffer, int length);

        [Diagnostics.DebuggerStepThrough]
        [Diagnostics.DebuggerHidden]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void WriteNative(string path, string fileName, long actualPosition, byte[] buffer, int length);

        [Diagnostics.DebuggerStepThrough]
        [Diagnostics.DebuggerHidden]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern long GetLengthNative(string path, string fileName);

        #endregion

    }
}
