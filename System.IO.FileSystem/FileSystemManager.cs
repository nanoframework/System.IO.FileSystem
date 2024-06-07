//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System.Collections;

namespace System.IO
{
    internal class FileSystemManager
    {
        private static readonly ArrayList _openFiles = new();
        private static readonly ArrayList _lockedDirs = new();
        private static object _currentDirectoryRecord = null;

        public static string CurrentDirectory = NativeIO.FSRoot;

        internal class FileRecord
        {
            public string FullName;
            public NativeFileStream NativeFileStream;
            public FileShare Share;

            public FileRecord(
                string fullName,
                FileShare share)
            {
                FullName = fullName;
                Share = share;
            }
        }

        public static object AddToOpenList(string fullName)
        {
            return AddToOpenList(
                fullName,
                FileAccess.ReadWrite,
                FileShare.None);
        }

        public static object AddToOpenListForRead(string fullName)
        {
            return AddToOpenList(
                fullName,
                FileAccess.Read,
                FileShare.ReadWrite);
        }

        public static FileRecord AddToOpenList(
            string fullName,
            FileAccess access,
            FileShare share)
        {
            fullName = fullName.ToUpper();

            FileRecord record = new FileRecord(
                fullName,
                share);

            lock (_openFiles)
            {
                int count = _lockedDirs.Count;

                for (int i = 0; i < count; i++)
                {
                    if (IsInDirectory(fullName, (string)_lockedDirs[i]))
                    {
                        throw new IOException(
                            string.Empty,
                            (int)IOException.IOExceptionErrorCode.UnauthorizedAccess);
                    }
                }

                FileRecord current;
                count = _openFiles.Count;

                for (int i = 0; i < count; ++i)
                {
                    current = (FileRecord)_openFiles[i];

                    if (current.FullName == fullName)
                    {
                        // Given the previous fileshare info and the requested fileaccess and fileshare
                        // the following is the ONLY combinations that we should allow -- All others
                        // should failed with IOException
                        // (Behavior verified on desktop .NET)
                        //
                        // Previous FileShare   Requested FileAccess    Requested FileShare
                        // Read                 Read                    ReadWrite
                        // Write                Write                   ReadWrite
                        // ReadWrite            Read                    ReadWrite
                        // ReadWrite            Write                   ReadWrite
                        // ReadWrite            ReadWrite               ReadWrite
                        //
                        // The following check take advantage of the fact that the value for
                        // Read, Write, and ReadWrite in FileAccess enum and FileShare enum are
                        // identical.

                        if ((share != FileShare.ReadWrite)
                            || (((int)current.Share & (int)access) != (int)access))
                        {
                            throw new IOException(
                                string.Empty,
                                (int)IOException.IOExceptionErrorCode.UnauthorizedAccess);
                        }
                    }
                }

                _openFiles.Add(record);
            }

            return record;
        }

        public static void RemoveFromOpenList(object record)
        {
            lock (_openFiles)
            {
                _openFiles.Remove(record);
            }
        }

        public static object LockDirectory(string directory)
        {
            directory = directory.ToUpper();

            lock (_openFiles)
            {
                if (_lockedDirs.Contains(directory) || AnyFileOpenInDirectory(directory))
                {
                    throw new IOException(
                        string.Empty,
                        (int)IOException.IOExceptionErrorCode.UnauthorizedAccess);
                }

                _lockedDirs.Add(directory);
            }

            return (object)directory;
        }

        private static bool AnyFileOpenInDirectory(string directory)
        {
            foreach (FileRecord record in _openFiles)
            {
                if (IsInDirectory(record.FullName, directory))
                {
                    return true;
                }
            }

            return false;
        }

        public static void UnlockDirectory(object record)
        {
            lock (_openFiles)
            {
                _lockedDirs.Remove(record);
            }
        }

        public static void UnlockDirectory(string directory)
        {
            directory = directory.ToUpper();

            lock (_openFiles)
            {
                _lockedDirs.Remove(directory);
            }
        }

        public static void ForceRemoveRootname(string rootNamme)
        {
            string root = rootNamme.ToUpper();
            ArrayList recordsToRemove = new();

            lock (_openFiles)
            {
                foreach (FileRecord record in _openFiles)
                {
                    if (IsInDirectory(record.FullName, root))
                    {
                        record.NativeFileStream?.Close();
                        recordsToRemove.Add(record);
                    }
                }

                foreach (FileRecord record in recordsToRemove)
                {
                    _openFiles.Remove(record);
                }
            }
        }

        public static bool IsInDirectory(
            string path,
            string directory)
        {
            if (path.IndexOf(directory) == 0)
            {
                int directoryLength = directory.Length;

                if (path.Length > directoryLength)
                {
                    return path[directoryLength] == '\\';
                }
                else
                {
                    return true;
                }
            }

            return false;
        }

        internal static void SetCurrentDirectory(string path)
        {
            if (_currentDirectoryRecord != null)
            {
                RemoveFromOpenList(_currentDirectoryRecord);
            }

            if (path != NativeIO.FSRoot)
            {
                _currentDirectoryRecord = AddToOpenListForRead(path);
            }
            else
            {
                _currentDirectoryRecord = null;
            }

            CurrentDirectory = path;
        }
    }
}
