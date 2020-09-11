using System;

namespace System.IO
{
    /// <summary>
    /// Contains constants for specifying how the OS should open a file.
    /// These will control whether you overwrite a file, open an existing
    /// file, or some combination thereof.
    ///
    /// To append to a file, use Append (which maps to OpenOrCreate then we seek
    /// to the end of the file).  To truncate a file or create it if it doesn't
    /// exist, use Create.
    /// </summary>
    [Serializable]
    public enum FileMode
    {
        /// <summary>
        /// Creates a new file. An exception is raised if the file already exists.
        /// </summary>
        CreateNew = 1,

        /// <summary>
        /// Creates a new file. If the file already exists, it is overwritten.
        /// </summary>
        Create = 2,

        /// <summary>
        /// Opens an existing file. An exception is raised if the file does not exist.
        /// </summary>
        Open = 3,

        /// <summary>
        /// Opens the file if it exists. Otherwise, creates a new file.
        /// </summary>
        OpenOrCreate = 4,

        /// <summary>
        /// Opens an existing file. Once opened, the file is truncated so that its
        /// size is zero bytes. The calling process must open the file with at least
        /// WRITE access. An exception is raised if the file does not exist.
        /// </summary>
        Truncate = 5,

        /// <summary>
        /// Opens the file if it exists and seeks to the end.  Otherwise,
        /// creates a new file.
        /// </summary>
        Append = 6,
    }
}
