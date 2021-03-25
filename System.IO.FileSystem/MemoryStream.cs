//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System;

namespace System.IO
{
    /// <summary>
    /// MemoryStream class
    /// </summary>
    public class MemoryStream : Stream
    {
        private const int CapacityDefaultSize = 256;
        // Either allocated internally or externally.
        private byte[] _buffer;
        // read/write head.
        private int _position;
        // Number of bytes within the memory stream
        private int _length;
        // length of usable portion of buffer for stream
        private int _capacity;
        // User-provided buffers aren't expandable.
        private bool _expandable;
        // Is this stream open or closed?
        private bool _isOpen;

        private const int MemStreamMaxLength = 0xFFFF;

        /// <summary>
        /// Creates an empty memory stream.
        /// </summary>
        /// <remarks>Default capacity is 256.</remarks>
        public MemoryStream()
        {
            _buffer = new byte[CapacityDefaultSize];
            _capacity = CapacityDefaultSize;
            _expandable = true;
            _isOpen = true;
        }

        /// <summary>
        /// Creates a memory stream from a buffer.
        /// </summary>
        /// <param name="buffer"></param>
        /// <exception cref="ArgumentNullException">Buffer can't be null.</exception>
        public MemoryStream(byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            _buffer = buffer;
            _length = _capacity = buffer.Length;
            _expandable = false;
            _isOpen = true;
        }

        ///<inheritdoc/>
        public override bool CanRead => _isOpen;

        ///<inheritdoc/>
        public override bool CanSeek => _isOpen;

        ///<inheritdoc/>
        public override bool CanWrite => _isOpen;

        ///<inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _isOpen = false;
            }
        }

        // returns a bool saying whether we allocated a new array.
        private bool EnsureCapacity(int value)
        {
            if (value > _capacity)
            {
                int newCapacity = value;
                if (newCapacity < CapacityDefaultSize)
                {
                    newCapacity = CapacityDefaultSize;
                }

                if (newCapacity < _capacity * 2)
                {
                    newCapacity = _capacity * 2;
                }

                if (!_expandable && newCapacity > _capacity)
                {
                    throw new NotSupportedException();
                }

                if (newCapacity > 0)
                {
                    byte[] newBuffer = new byte[newCapacity];
                    if (_length > 0) Array.Copy(_buffer, 0, newBuffer, 0, _length);
                    _buffer = newBuffer;
                }
                else
                {
                    _buffer = null;
                }

                _capacity = newCapacity;

                return true;
            }

            return false;
        }

        ///<inheritdoc/>
        public override void Flush()
        { }

        ///<inheritdoc/>
        public override long Length
        {
            get
            {
                EnsureOpen();
                return _length;
            }
        }

        ///<inheritdoc/>
        ///<exception cref="IOException">Can't adjust position out of the buffer size for fixed size buffer</exception>
        ///<exception cref="ArgumentOutOfRangeException">Position can't be negative or higher than the stream allocated size.</exception>
        public override long Position
        {
            get
            {
                EnsureOpen();
                return _position;
            }

            set
            {
                EnsureOpen();
                if (value < 0 || value > MemStreamMaxLength)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                if ((!_expandable) && (value >= _length))
                {
                    throw new IOException();
                }

                _position = (int)value;
            }
        }

        ///<inheritdoc/>
        /// <exception cref="ArgumentException">Invalid length.</exception>
        /// <exception cref="ArgumentNullException">IBuffer is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Offset is negative or longer than count.</exception>
        public override int Read(byte[] buffer, int offset, int count)
        {
            EnsureOpen();
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (offset < 0 || count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            if (buffer.Length - offset < count)
            {
                throw new ArgumentException();
            }

            int n = (int)(_length - _position);
            if (n > count) n = count;
            if (n <= 0)
            {
                return 0;
            }

            Array.Copy(_buffer, _position, buffer, offset, n);
            _position += n;
            return n;
        }

        ///<inheritdoc/>
        public override int ReadByte()
        {
            EnsureOpen();
            if (_position >= _length)
            {
                return -1;
            }

            return _buffer[_position++];
        }

        private void EnsureOpen()
        {
            if (!_isOpen)
            {
                throw new ObjectDisposedException();
            }
        }

        ///<inheritdoc/>
        ///<exception cref="IOException">You cannot Seek before origin or you cannot Seek after a fixed size buffer.</exception>
        ///<exception cref="ArgumentException">"Invalid seek origin."</exception>
        public override long Seek(long offset, SeekOrigin origin)
        {
            EnsureOpen();
            if (offset > MemStreamMaxLength)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            switch (origin)
            {
                case SeekOrigin.Begin:
                    if (offset < 0)
                    {
                        throw new IOException();
                    }

                    if ((!_expandable) && (offset >= _length))
                    {
                        throw new IOException();
                    }

                    _position = (int)offset;
                    break;

                case SeekOrigin.Current:
                    if (offset + _position < 0)
                    {
                        throw new IOException();
                    }

                    if ((!_expandable) && (offset >= _length))
                    {
                        throw new IOException();
                    }

                    _position += (int)offset;
                    break;

                case SeekOrigin.End:
                    if (_length + offset < 0)
                    {
                        throw new IOException();
                    }

                    if ((!_expandable) && (_length + offset >= _length))
                    {
                        throw new IOException();
                    }

                    _position = _length + (int)offset;
                    break;

                default:
                    throw new ArgumentException();
            }

            return _position;
        }

        ///<inheritdoc/>
        public override void SetLength(long value)
        {
            EnsureOpen();

            if (value > MemStreamMaxLength || value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            int newLength = (int)value;
            bool allocatedNewArray = EnsureCapacity(newLength);
            if (!allocatedNewArray && newLength > _length)
            {
                Array.Clear(_buffer, _length, newLength - _length);
            }

            _length = newLength;
            if (_position > newLength)
            {
                _position = newLength;
            }
        }

        ///<inheritdoc/>
        public virtual byte[] ToArray()
        {
            byte[] copy = new byte[_length];
            Array.Copy(_buffer, 0, copy, 0, _length);
            return copy;
        }

        ///<inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            EnsureOpen();

            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (offset < 0 || count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            if (buffer.Length - offset < count)
            {
                throw new ArgumentException();
            }

            int i = _position + count;
            // Check for overflow
            if (i > _length)
            {
                if (i > _capacity)
                {
                    EnsureCapacity(i);
                }

                _length = i;
            }

            Array.Copy(buffer, offset, _buffer, _position, count);
            _position = i;
            return;
        }

        ///<inheritdoc/>
        public override void WriteByte(byte value)
        {
            EnsureOpen();

            if (_position >= _capacity)
            {
                EnsureCapacity(_position + 1);
            }

            _buffer[_position++] = value;

            if (_position > _length)
            {
                _length = _position;
            }
        }

        /// <summary>
        /// Writes this MemoryStream to another stream.
        /// </summary>
        /// <param name="stream">Stream to write into.</param>
        /// <exception cref="ArgumentNullException">if stream is null.</exception>
        public virtual void WriteTo(Stream stream)
        {
            EnsureOpen();
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            stream.Write(_buffer, 0, _length);
        }
    }
}


