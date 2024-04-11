//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

namespace System.IO
{
    /// <summary>
    /// Exposes instance methods for creating, moving, and enumerating through directories and subdirectories. This class cannot be inherited.
    /// </summary>
    public sealed class DirectoryInfo : FileSystemInfo
    {
        /// <summary>
        /// Gets the name of this <see cref="DirectoryInfo"/> instance.
        /// </summary>
        public override string Name
        {
            get
            {
                return Path.GetFileName(_fullPath);
            }
        }

        /// <inheritdoc/>
        public override bool Exists
        {
            get
            {
                return Directory.Exists(_fullPath);
            }
        }

        /// <summary>
        /// Gets the root portion of the directory.
        /// </summary>
        /// <value>An object that represents the root of the directory.</value>
        public DirectoryInfo Root
        {
            get
            {
                return new DirectoryInfo(Path.GetPathRoot(_fullPath));
            }
        }

        /// <summary>
        /// Gets the parent directory of a specified subdirectory.
        /// </summary>
        public DirectoryInfo Parent
        {
            get
            {
                string parentDirPath = Path.GetDirectoryName(_fullPath);

                if (parentDirPath == null)
                {
                    return null;
                }

                return new DirectoryInfo(parentDirPath);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryInfo"/> class on the specified path.
        /// </summary>
        /// <param name="path"></param>
        public DirectoryInfo(string path)
        {
            // path validation happening in the call
            _fullPath = Path.GetFullPath(path);
        }

        /// <summary>
        /// Creates a directory.
        /// </summary>
        public void Create()
        {
            Directory.CreateDirectory(_fullPath);
        }

        /// <summary>
        /// Creates a subdirectory or subdirectories on the specified path. The specified path can be relative to this instance of the <see cref="DirectoryInfo"/> class.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public DirectoryInfo CreateSubdirectory(string path)
        {
            // path validatation happening in the call
            string subDirPath = Path.Combine(
                _fullPath,
                path);

            // This will also ensure "path" is valid.
            subDirPath = Path.GetFullPath(subDirPath);

            return Directory.CreateDirectory(subDirPath);
        }

        /// <summary>
        /// Returns a file list from the current directory.
        /// </summary>
        /// <returns>
        /// An array of type <see cref="FileInfo"/>.
        /// </returns>
        /// <exception cref="IOException"> Logical drive or a directory under given path does not exist. </exception>
        public FileInfo[] GetFiles()
        {
            string[] fileNames = Directory.GetFiles(_fullPath);

            FileInfo[] files = new FileInfo[fileNames.Length];

            for (int i = 0; i < fileNames.Length; i++)
            {
                files[i] = new FileInfo(fileNames[i]);
            }

            return files;
        }

        /// <summary>
        /// Returns the subdirectories of the current directory.
        /// </summary>
        /// <returns>An array of <see cref="DirectoryInfo"/> objects.</returns>
        public DirectoryInfo[] GetDirectories()
        {
            // searchPattern validation happening in the call
            string[] dirNames = Directory.GetDirectories(_fullPath);

            DirectoryInfo[] dirs = new DirectoryInfo[dirNames.Length];

            for (int i = 0; i < dirNames.Length; i++)
            {
                dirs[i] = new DirectoryInfo(dirNames[i]);
            }

            return dirs;
        }

        /// <summary>
        /// Moves a <see cref="DirectoryInfo"/> instance and its contents to a new path.
        /// </summary>
        /// <param name="destDirName">The name and path to which to move this directory. The destination cannot be another disk volume or a directory with the identical name. It can be an existing directory to which you want to add this directory as a subdirectory.</param>
        /// <exception cref="IOException">An attempt was made to move a directory to a different volume. -or- <paramref name="destDirName"/> already exists. -or- The source directory does not exist. -or- The source or destination directory name is <see langword="null"/>.</exception>"
        /// <exception cref="ArgumentNullException"><paramref name="destDirName"/> is <see langword="null"/>.</exception>"
        public void MoveTo(string destDirName)
        {
            // destDirName validation happening in the call
            Directory.Move(
                _fullPath,
                destDirName);
        }

        /// <inheritdoc/>
        public override void Delete()
        {
            Directory.Delete(_fullPath);
        }

        /// <summary>
        /// Deletes this instance of a <see cref="DirectoryInfo"/>, specifying whether to delete subdirectories and files.
        /// </summary>
        /// <param name="recursive"><see langword="true"/> to delete this directory, its subdirectories, and all files; otherwise, <see langword="false"/>.</param>
        public void Delete(bool recursive)
        {
            Directory.Delete(
                _fullPath,
                recursive);
        }

        /// <summary>
        /// Returns the original path that was passed to the <see cref="DirectoryInfo"/> constructor. Use the <see cref="FileSystemInfo.FullName"/> or <see cref="Name"/> properties for the full path or file/directory name instead of this method.
        /// </summary>
        /// <returns>The original path that was passed by the user.</returns>
        public override string ToString()
        {
            return _fullPath;
        }
    }
}
