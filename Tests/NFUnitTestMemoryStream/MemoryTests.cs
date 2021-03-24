using nanoFramework.TestFramework;
using System;
using System.IO;

namespace NFUnitTestMemoryStream
{
    [TestClass]
    public class MemoryTests
    {
        [TestMethod]
        public void EmptyMemoryStream()
        {
            // Arrange
            MemoryStream ms = new MemoryStream();
            // Act            
            // Assert
            Assert.True(ms.CanRead);
            Assert.True(ms.CanSeek);
            Assert.True(ms.CanWrite);
            Assert.Equal(0, ms.Length);
            Assert.Equal(0, ms.Position);
            Assert.Throws(typeof(ArgumentNullException), () => { ms.Read(null, 0, 0); });
            ms.WriteByte(42);
            ms.Position = 0;
            Assert.Equal(42, ms.ReadByte());
            Assert.Equal(-1, ms.ReadByte());
            ms.Dispose();
            Assert.Throws(typeof(ObjectDisposedException), () => { ms.ReadByte(); });
        }

        [TestMethod]

        public void MemoryStreamRead()
        {
            // Arrange
            byte[] array = new byte[10] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            byte[] subArray = new byte[5];
            MemoryStream ms = new MemoryStream(array);
            // Act
            ms.Position = 2;
            ms.Read(subArray, 0, subArray.Length);
            ms.Position = 0;
            // Assert
            Assert.True(ms.CanRead);
            Assert.True(ms.CanSeek);
            Assert.True(ms.CanWrite);
            Assert.Equal(array.Length, ms.Length);
            Assert.Equal(0, ms.Position);
            Assert.Equal(0, ms.ReadByte());
            Assert.Equal(1, ms.Position);
            Assert.Equal(1, ms.ReadByte());
            ms.Position = 9;
            Assert.Equal(9, ms.ReadByte());
            Assert.Equal(10, ms.Position);
            // It's a fixed buffer, we can't expand it
            Assert.Throws(typeof(NotSupportedException), () => { ms.WriteByte(10); });
            // Let's check the sub buffer
            Assert.Equal(array[2], subArray[0]);
            Assert.Equal(array[6], subArray[4]);
        }

        [TestMethod]

        public void MemoryStreamSeek()
        {
            // Arrange
            byte[] array = new byte[10] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            MemoryStream ms = new MemoryStream(array);
            // Act
            //Assert
            ms.Seek(2, SeekOrigin.Begin);
            Assert.Equal(2, ms.Position);
            ms.Seek(-2, SeekOrigin.End);
            Assert.Equal(8, ms.Position);
            ms.Seek(1, SeekOrigin.Current);
            Assert.Equal(9, ms.Position);
            Assert.Throws(typeof(IOException), () => { ms.Seek(1, SeekOrigin.End); });
            Assert.Throws(typeof(IOException), () => { ms.Seek(-11, SeekOrigin.End); });
            Assert.Throws(typeof(IOException), () => { ms.Seek(-1, SeekOrigin.Begin); });
            Assert.Throws(typeof(IOException), () => { ms.Seek(11, SeekOrigin.Begin); });
            ms.Position = 0;
            Assert.Throws(typeof(IOException), () => { ms.Seek(-1, SeekOrigin.Current); });
            Assert.Throws(typeof(IOException), () => { ms.Seek(11, SeekOrigin.Current); });
        }

        [TestMethod]
        public void MemoryStreamWrite()
        {
            // Arrange
            byte[] array = new byte[10] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            byte[] subArray = new byte[5] { 10, 11, 12, 13, 14 };
            MemoryStream ms = new MemoryStream(array);
            // Act
            ms.Position = 2;
            ms.Write(subArray, 0, 5);
            ms.Position = 2;
            //Assert
            Assert.Equal(10, ms.ReadByte());
            ms.Position = 6;
            Assert.Equal(14, ms.ReadByte());
            Assert.Throws(typeof(ArgumentNullException), () => { ms.Write(null, 0, 0); });
            Assert.Throws(typeof(ArgumentException), () => { ms.Write(new byte[0], 1, 0); });
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => { ms.Write(new byte[0], 0, -1); });
        }

        [TestMethod]
        public void MemoryStreamWriteExpandable()
        {
            // Arrange
            byte[] array = new byte[10] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            MemoryStream ms = new MemoryStream();
            // Act
            ms.Write(array, 0, 5);
            //Assert
            Assert.Equal(5, ms.Length);
            ms.Seek(-1, SeekOrigin.End);
            Assert.Equal(4, ms.ReadByte());
            ms.Write(array, 0, 10);
            Assert.Equal(15, ms.Length);
        }
    }
}
