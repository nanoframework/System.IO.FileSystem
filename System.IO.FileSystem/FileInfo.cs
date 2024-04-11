//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System.IO.System.IO;

namespace System.IO
{
    /// <summary>
    /// Provides properties and instance methods for the creation, copying, deletion, moving, and opening of files, and aids in the creation of <see cref="FileStream"/> objects. This class cannot be inherited.
    /// </summary>
    [Serializable]
    public sealed class FileInfo : FileSystemInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileInfo"/> class, which acts as a wrapper for a file path.
        /// </summary>
        /// <param name="fileName"></param>
        public FileInfo(string fileName)
        {
            // path validation in GetFullPath
            _fullPath = Path.GetFullPath(fileName);
        }

        /// <inheritdoc/>
        public override string Name
        {
            get
            {
                return Path.GetFileName(_fullPath);
            }
        }

        /// <summary>
        /// Gets the size, in bytes, of the current file.
        /// </summary>
        public long Length
        {
            get
            {
                RefreshIfNull();

                return (long)_nativeFileInfo.Size;
            }
        }

        /// <summary>
        /// Gets a string representing the directory's full path.
        /// </summary>
        public string DirectoryName
        {
            get
            {
                return Path.GetDirectoryName(_fullPath);
            }
        }

        /// <summary>
        /// Gets an instance of the parent directory.
        /// </summary>
        public DirectoryInfo Directory
        {
            get
            {
                string dirName = DirectoryName;

                return dirName == null ? null : new DirectoryInfo(dirName);
            }
        }

        /// <summary>
        /// Creates a file.
        /// </summary>
        /// <returns>A new file.</returns>
        public FileStream Create()
        {
            return File.Create(_fullPath);
        }

        ///<inheritdoc/>
        public override void Delete()
        {
            File.Delete(_fullPath);
        }

        ///<inheritdoc/>
        public override bool Exists
        {
            get
            {
                return File.Exists(_fullPath);
            }
        }

        /// <summary>
        /// Returns the original path that was passed to the FileInfo constructor. Use the <see cref="FileSystemInfo.FullName"/> or <see cref="Name"/> property for the full path or file name.
        /// </summary>
        /// <returns>A string representing the path.</returns>
        public override string ToString()
        {
            return _fullPath;
        }
    }
}
