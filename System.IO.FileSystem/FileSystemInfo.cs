﻿//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.IO
{
    /// <summary>
    /// Provides the base class for both <see cref="FileInfo"/> and <see cref="DirectoryInfo"/> objects.
    /// </summary>
    public abstract class FileSystemInfo : MarshalByRefObject
    {
        internal NativeFileInfo _nativeFileInfo;

        // fully qualified path of the directory
        internal string _fullPath;

        /// <summary>
        /// Gets or sets the attributes for the current file or directory.
        /// </summary>
        public FileAttributes Attributes
        {
            get
            {
                RefreshIfNull();

                return (FileAttributes)_nativeFileInfo.Attributes;
            }
        }

        /// <summary>
        /// Gets the full path of the directory or file.
        /// </summary>
        public virtual string FullName
        {
            get
            {
                return _fullPath;
            }
        }

        /// <summary>
        /// Gets the extension part of the file name, including the leading dot <code>.</code> even if it is the entire file name, or an empty string if no extension is present.
        /// </summary>
        /// <value>A string containing the FileSystemInfo extension.</value>
        public string Extension
        {
            get
            {
                return Path.GetExtension(FullName);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the file or directory exists.
        /// </summary>
        public abstract bool Exists
        {
            get;
        }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        public abstract string Name
        {
            get;
        }

        /// <summary>
        /// Deletes a file or directory.
        /// </summary>
        public abstract void Delete();

        /// <summary>
        /// Refreshes the state of the object.
        /// </summary>
        /// <exception cref="IOException">A device such as a disk drive is not ready.</exception>
        public virtual void Refresh()
        {
            object record = FileSystemManager.AddToOpenListForRead(_fullPath);

            try
            {
                _nativeFileInfo = NativeFindFile.GetFileInfo(_fullPath);

                if (_nativeFileInfo == null)
                {
                    HandleRefreshError();
                }
            }
            finally
            {
                FileSystemManager.RemoveFromOpenList(record);
            }
        }

        /// <summary>
        /// Handler for the case when the file or directory does not exist.
        /// </summary>
        protected abstract void HandleRefreshError();

        internal void RefreshIfNull()
        {
            if (_nativeFileInfo == null)
            {
                Refresh();
            }
        }
    }
}
