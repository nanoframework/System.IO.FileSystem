using System;
using System.Runtime.CompilerServices;

namespace System.IO
{
    /// <summary>
    /// Class for creating FileStream objects, and some basic file management
    /// routines such as Delete, etc.
    /// </summary>
    public static class File
    {
        #region Constants

        private const int _defaultCopyBufferSize = 2048;

        #endregion


        #region Static Methods

        /// <summary>
        /// Copies an existing file to a new file. Overwriting a file of the same name is not allowed.
        /// </summary>
        /// <param name="sourceFileName">The file to copy.</param>
        /// <param name="destFileName">The name of the destination file. This cannot be a directory or an existing file.</param>
        public static void Copy(String sourceFileName, String destFileName)
        {
            Copy(sourceFileName, destFileName, false);
        }

        /// <summary>
        /// Copies an existing file to a new file. Overwriting a file of the same name is allowed.
        /// </summary>
        /// <param name="sourceFileName">The file to copy.</param>
        /// <param name="destFileName">The name of the destination file. This cannot be a directory.</param>
        /// <param name="overwrite">true if the destination file can be overwritten; otherwise, false.</param>
        public static void Copy(String sourceFileName, String destFileName, bool overwrite)
        {
            // TODO: File Handling missing
            
            FileMode writerMode = (overwrite) ? FileMode.Create : FileMode.CreateNew;

            FileStream reader = new FileStream(sourceFileName, FileMode.Open, FileAccess.Read);

            try
            {
                using (FileStream writer = new FileStream(destFileName, writerMode, FileAccess.Write))
                {
                    long fileLength = reader.Length;
                    //writer.SetLength(fileLength);

                    byte[] buffer = new byte[_defaultCopyBufferSize];
                    for ( ; ; )
                    {
                        int readSize = reader.Read(buffer, 0, _defaultCopyBufferSize);
                        if (readSize <= 0)
                            break;

                        writer.Write(buffer, 0, readSize);
                    }

                    // Copy the attributes too
                    File.SetAttributes(destFileName, File.GetAttributes(sourceFileName));
                }
            }
            finally
            {
                    reader.Dispose();
            }
        }

        /// <summary>
        /// Creates or overwrites a file in the specified path.
        /// </summary>
        /// <param name="path">The path and name of the file to create.</param>
        /// <returns></returns>
        public static FileStream Create(String path)
        {
            return new FileStream(path, FileMode.Create, FileAccess.ReadWrite);
        }

        /// <summary>
        /// Deletes the specified file.
        /// </summary>
        /// <param name="path">The name of the file to be deleted. Wildcard characters are not supported.</param>
        public static void Delete(String path)
        {
            if (path == null)
                throw new ArgumentNullException("Path must be defined.");

            Path.CheckInvalidPathChars(path);

            try
            {
                byte attributes;
                string folderPath = Path.GetDirectoryName(path);

                // Only check folder if its not the Root
                if (folderPath != Path.GetPathRoot(path))
                {
                    attributes = (byte)NativeFile.GetAttributesNative(folderPath);

                    // Check if Directory existing
                    if (attributes == 0xFF)
                    {
                        throw new IOException("Directory not found.", (int)IOException.IOExceptionErrorCode.DirectoryNotFound);
                    }
                }

                // Folder exists, now verify whether the file itself exists.
                attributes = (byte)NativeFile.GetAttributesNative(path);
                if (attributes == 0xFF)
                {
                    // No-op on file not found
                    return;
                }

                // Check if file is ReadOnly or Directory (then not allowed to delete)
                if ((attributes & (byte)(FileAttributes.ReadOnly)) != 0)
                {
                    throw new IOException("Not allowed to delete ReadOnly Files.", (int)IOException.IOExceptionErrorCode.UnauthorizedAccess);
                }
                if ((attributes & (byte)(FileAttributes.Directory)) != 0)
                {
                    throw new IOException("Not allowed to delete Directories.", (int)IOException.IOExceptionErrorCode.UnauthorizedAccess);
                }

                NativeFile.DeleteNative(path);
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
        /// <returns></returns>
        public static bool Exists(String path)
        {
            return NativeFile.ExistsNative(Path.GetDirectoryName(path), Path.GetFileName(path));
        }

        /// <summary>
        /// Moves a specified file to a new location, providing the option to specify a new file name.
        /// </summary>
        /// <param name="sourceFileName">The name of the file to move. Absolute path.</param>
        /// <param name="destFileName">The new path and name for the file.</param>
        public static void Move(String sourceFileName, String destFileName)
        {
            // Src File must exists!
            if (!File.Exists(sourceFileName))
                throw new Exception("Source File not existing.");

            // Dest must not exist!
            if (File.Exists(destFileName))
                throw new Exception("Destination File already existing.");

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
                NativeFile.MoveNative(sourceFileName, destFileName);
            }
        }

        /// <summary>
        /// Gets the FileAttributes of the file on the path.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns>The FileAttributes of the file on the path.</returns>
        public static FileAttributes GetAttributes(string path)
        {
            byte attributes;

            attributes = NativeFile.GetAttributesNative(path);

            if (attributes == 0xFF)
            {
                throw new IOException("", (int)IOException.IOExceptionErrorCode.FileNotFound);
            }
            else
            {
                return (FileAttributes)attributes;
            }
        }

        /// <summary>
        /// Sets the specified FileAttributes of the file on the specified path.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <param name="fileAttributes">A bitwise combination of the enumeration values.</param>
        public static void SetAttributes(string path, FileAttributes fileAttributes)
        {
            NativeFile.SetAttributesNative(path, (byte) fileAttributes);
        }

        #endregion
    }

    #region Stubs (Native Calls)

    internal static class NativeFile
    {
        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerHidden]
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool ExistsNative(string path, string fileName);

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerHidden]
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void MoveNative(string pathSrc, string pathDest);

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerHidden]
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void DeleteNative(string path);

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerHidden]
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern byte GetAttributesNative(string path);

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerHidden]
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void SetAttributesNative(string path, byte attributes);
    }

    #endregion
}
