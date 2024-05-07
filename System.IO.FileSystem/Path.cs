//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace System.IO
{
    /// <summary>
    /// Provides methods for processing file system strings in a cross-platform manner.
    /// Most of the methods don't do a complete parsing (such as examining a UNC hostname),
    /// but they will handle most string operations.
    /// </summary>
    public static class Path
    {
        // Public static readonly variant of the separators. The Path implementation itself is using
        // internal const variant of the separators for better performance.

        /// <summary>
        /// Provides a platform-specific character used to separate directory levels in a path string that reflects a hierarchical file system organization.
        /// </summary>
        public static readonly char DirectorySeparatorChar = PathInternal.DirectorySeparatorChar;

        /// <summary>
        /// Provides a platform-specific alternate character used to separate directory levels in a path string that reflects a hierarchical file system organization.
        /// </summary>
        public static readonly char AltDirectorySeparatorChar = PathInternal.AltDirectorySeparatorChar;

        /// <summary>
        /// Provides a platform-specific volume separator character.
        /// </summary>
        public static readonly char VolumeSeparatorChar = PathInternal.VolumeSeparatorChar;

        /// <summary>
        /// A platform-specific separator character used to separate path strings in environment variables.
        /// </summary>
        public static readonly char PathSeparator = PathInternal.PathSeparator;

        // TODO: This is not needed when <see cref="string.StartsWith"/> has an overload for <see cref="char"/>
        private const string ExtensionSeparatorString = ".";

        /// <summary>
        /// Changes the extension of a path string.
        /// </summary>
        /// <param name="path">The path information to modify.</param>
        /// <param name="extension">
        /// The new extension (with or without a leading period). Specify <see langword="null"/> to remove an existing extension from <paramref name="path"/>.
        /// </param>
        /// <returns>
        /// The modified path information.
        /// 
        /// If <paramref name="path"/> is <see langword="null"/> or an empty string (""), the path information is returned unmodified.
        /// If <paramref name="extension"/> is <see langword="null"/>, the returned string contains the specified path with its extension removed.
        /// If <paramref name="path"/> has no extension, and <paramref name="extension"/> is not <see langword="null"/>, the returned path string
        /// contains <paramref name="extension"/> appended to the end of <paramref name="path"/>.
        /// </returns>
        public static string ChangeExtension(string path, string extension)
        {
            if (path is null)
            {
                return null;
            }

            var subLength = path.Length;
            if (subLength == 0)
            {
                return string.Empty;
            }

            for (var i = path.Length - 1; i >= 0; i--)
            {
                var ch = path[i];

                if (ch == '.')
                {
                    subLength = i;
                    break;
                }

                if (PathInternal.IsDirectorySeparator(ch))
                {
                    break;
                }
            }

            if (extension is null)
            {
                return path.Substring(0, subLength);
            }

            var subPath = path.Substring(0, subLength);

            return extension.StartsWith(ExtensionSeparatorString) ?
                string.Concat(subPath, extension) :
                string.Concat(subPath, ExtensionSeparatorString, extension);
        }

        /// <summary>
        /// Combines two strings into a path.
        /// </summary>
        /// <param name="path1">The first path to combine.</param>
        /// <param name="path2">The second path to combine.</param>
        /// <returns>
        /// The combined paths. If one of the specified paths is a zero-length string, this method returns the other path.
        /// If <paramref name="path2"/> contains an absolute path, this method returns <paramref name="path2"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path1"/> or <paramref name="path2"/> is <see langword="null"/>.</exception>
        public static string Combine(string path1, string path2)
        {
            if (path1 is null || path2 is null)
            {
                throw new ArgumentNullException();
            }

            if (string.IsNullOrEmpty(path1))
            {
                return path2;
            }

            if (string.IsNullOrEmpty(path2))
            {
                return path1;
            }

            if (IsPathRooted(path2))
            {
                return path2;
            }

            return JoinInternal(path1, path2);
        }

        /// <summary>
        /// Returns the directory portion of a file path. This method effectively
        /// removes the last segment of the given file path, i.e. it returns a
        /// string consisting of all characters up to but not including the last
        /// backslash ("\") in the file path. The returned value is null if the
        /// specified path is null, empty, or a root (such as "\", "C:", or
        /// "\\server\share").
        /// </summary>
        /// <remarks>
        /// Directory separators are normalized in the returned string.
        /// </remarks>
        public static string GetDirectoryName(string path)
        {
            if (path is null || PathInternal.IsEffectivelyEmpty(path))
            {
                return null;
            }

            var end = GetDirectoryNameOffset(path);
            return end >= 0 ? PathInternal.NormalizeDirectorySeparators(path.Substring(0, end)) : null;
        }

        internal static int GetDirectoryNameOffset(string path)
        {
            var rootLength = PathInternal.GetRootLength(path);
            var end = path.Length;

            if (end <= rootLength)
            {
                return -1;
            }

            while (end > rootLength && !PathInternal.IsDirectorySeparator(path[--end]))
            {
            }

            // Trim off any remaining separators (to deal with C:\foo\\bar)
            while (end > rootLength && PathInternal.IsDirectorySeparator(path[end - 1]))
            {
                end--;
            }

            return end;
        }

        /// <summary>
        /// Returns the extension (including the period ".") of the specified path string.
        /// </summary>
        /// <param name="path">The path string from which to get the extension.</param>
        /// <returns>
        /// The extension of the specified path (including the period "."), or <see langword="null"/>, or <see cref="string.Empty"/>.
        /// If <paramref name="path"/> is <see langword="null"/>, <see cref="GetExtension(string)"/> returns <see langword="null"/>.
        /// If path does not have extension information, <see cref="GetExtension(string)"/> returns <see cref="string.Empty"/>.</returns>
        [return: NotNullIfNotNull("path")]
        public static string GetExtension(string path)
        {
            if (path is null)
            {
                return null;
            }

            var length = path.Length;

            for (var i = length - 1; i >= 0; i--)
            {
                var ch = path[i];

                if (ch == '.')
                {
                    if (i != length - 1)
                    {
                        return path.Substring(i, length - i);
                    }

                    return string.Empty;
                }

                if (PathInternal.IsDirectorySeparator(ch))
                {
                    break;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Returns the file name and extension of the specified path string.
        /// </summary>
        /// <param name="path">The path string from which to obtain the file name and extension.</param>
        /// <returns>
        /// The characters after the last directory separator character in <paramref name="path"/>.
        /// If the last character of <paramref name="path"/> is a directory or volume separator character, this method returns <see cref="string.Empty"/>.
        /// If <paramref name="path"/> is <see langword="null"/>, this method returns <see langword="null"/>.
        /// </returns>
        [return: NotNullIfNotNull("path")]
        public static string GetFileName(string path)
        {
            if (path is null)
            {
                return null;
            }

            var root = GetPathRoot(path).Length;

            // We don't want to cut off "C:\file.txt:stream" (i.e. should be "file.txt:stream")
            // but we *do* want "C:Foo" => "Foo". This necessitates checking for the root.

            for (var i = path.Length; --i >= 0;)
            {
                if (i < root || PathInternal.IsDirectorySeparator(path[i]))
                {
                    return path.Substring(i + 1);
                }
            }

            return path;
        }

        /// <summary>
        /// Returns the file name of the specified path string without the extension.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        /// <returns>The string returned by <see cref="GetFileName(string)"/>, minus the last period (.) and all characters following it.</returns>
        [return: NotNullIfNotNull("path")]
        public static string GetFileNameWithoutExtension(string path)
        {
            if (path is null)
            {
                return null;
            }

            var fileName = GetFileName(path);
            var lastPeriod = fileName.LastIndexOf('.');

            return lastPeriod < 0 ?
                fileName : // No extension was found
                fileName.Substring(0, lastPeriod);
        }

        /// <summary>
        /// Returns the absolute path for the specified path string.
        /// </summary>
        /// <param name="path">The file or directory for which to obtain absolute path information.</param>
        /// <returns>The fully qualified location of <paramref name="path"/>, such as "C:\MyFile.txt".</returns>
        public static string GetFullPath(string path)
        {
            ValidateNullOrEmpty(path);

            if (!IsPathRooted(path))
            {
                // TODO: will be implemented in next PR
                // string currDir = Directory.GetCurrentDirectory();
                // path = Combine(currDir, path);
            }

            return PathInternal.NormalizeDirectorySeparators(path);
        }

        /// <summary>
        /// Gets an array containing the characters that are not allowed in file names.
        /// </summary>
        /// <returns>An array containing the characters that are not allowed in file names.</returns>
        public static char[] GetInvalidFileNameChars() => new char[]
        {
            '\"', '<', '>', '|', '\0',
            (char)1, (char)2, (char)3, (char)4, (char)5, (char)6, (char)7, (char)8, (char)9, (char)10,
            (char)11, (char)12, (char)13, (char)14, (char)15, (char)16, (char)17, (char)18, (char)19, (char)20,
            (char)21, (char)22, (char)23, (char)24, (char)25, (char)26, (char)27, (char)28, (char)29, (char)30,
            (char)31, ':', '*', '?', '\\', '/'
        };

        /// <summary>
        /// Gets an array containing the characters that are not allowed in path names.
        /// </summary>
        /// <returns>An array containing the characters that are not allowed in path names.</returns>
        public static char[] GetInvalidPathChars() => new[]
        {
            '|', '\0',
            (char)1, (char)2, (char)3, (char)4, (char)5, (char)6, (char)7, (char)8, (char)9, (char)10,
            (char)11, (char)12, (char)13, (char)14, (char)15, (char)16, (char)17, (char)18, (char)19, (char)20,
            (char)21, (char)22, (char)23, (char)24, (char)25, (char)26, (char)27, (char)28, (char)29, (char)30,
            (char)31
        };

        /// <summary>
        /// Gets the root directory information from the path contained in the specified string.
        /// </summary>
        /// <param name="path">A string containing the path from which to obtain root directory information.</param>
        /// <returns>
        /// The root directory of <paramref name="path"/> if it is rooted.
        /// 
        /// -or-
        /// 
        /// <see cref="string.Empty"/> if <paramref name="path"/> does not contain root directory information.
        /// 
        /// -or-
        /// 
        /// <see langword="null"/> if <paramref name="path"/> is <see langword="null"/> or is effectively empty.
        /// </returns>
        public static string GetPathRoot(string path)
        {
            if (PathInternal.IsEffectivelyEmpty(path))
            {
                return null;
            }

            var pathRootLength = PathInternal.GetRootLength(path);
            var pathRoot = pathRootLength <= 0 ? string.Empty : path.Substring(0, pathRootLength);

            return PathInternal.NormalizeDirectorySeparators(pathRoot);
        }

        /// <summary>
        /// Determines whether a path includes a file name extension.
        /// </summary>
        /// <param name="path">The path to search for an extension.</param>
        /// <returns>
        /// <see langword="true"/> if the characters that follow the last directory separator (\ or /) or volume separator (:)
        /// in the path include a period (.) followed by one or more characters; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool HasExtension([NotNullWhen(true)] string path)
        {
            if (path is null)
            {
                return false;
            }

            for (var i = path.Length - 1; i >= 0; i--)
            {
                var ch = path[i];

                if (ch == '.')
                {
                    return i != path.Length - 1;
                }

                if (PathInternal.IsDirectorySeparator(ch))
                {
                    break;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns a value indicating whether the specified path string contains a root.
        /// </summary>
        /// <param name="path">The path to test.</param>
        /// <returns><see langword="true"/> if <paramref name="path"/> contains a root; otherwise, <see langword="false"/>.</returns>
        public static bool IsPathRooted([NotNullWhen(true)] string path)
        {
            if (path is null)
            {
                return false;
            }

            var length = path.Length;
            return (length >= 1 && PathInternal.IsDirectorySeparator(path[0]))
                   || (length >= 2 && PathInternal.IsValidDriveChar(path[0]) && path[1] == PathInternal.VolumeSeparatorChar);
        }

        private static string JoinInternal(string first, string second)
        {
            Debug.Assert(first.Length > 0 && second.Length > 0, "should have dealt with empty paths");

            var hasSeparator = PathInternal.IsDirectorySeparator(first[first.Length - 1])
                               || PathInternal.IsDirectorySeparator(second[0]);

            if (first.Equals(PathInternal.DirectorySeparatorCharAsString))
            {
                first = string.Empty;
            }

            return hasSeparator ?
                string.Concat(first, second) :
                string.Concat(first, PathInternal.DirectorySeparatorCharAsString, second);
        }

        private static void ValidateNullOrEmpty(string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException();
            }

            if (str.Length == 0)
            {
                throw new ArgumentException();
            }
        }
    }
}
