using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace System.IO
{
    /// <summary>Contains internal path helpers that are shared between many projects.</summary>
    internal static class PathInternal
    {
        internal const char DirectorySeparatorChar = '\\';
        internal const char AltDirectorySeparatorChar = '/';
        internal const char VolumeSeparatorChar = ':';
        internal const char PathSeparator = ';';

        internal const string DirectorySeparatorCharAsString = "\\";

        internal const int MaxShortPath = 260;
        internal const int MaxShortDirectoryPath = 248;

#if PATH_SUPPORTS_UNC
        // \\?\, \\.\, \??\
        internal const int DevicePrefixLength = 4;
        // \\
        internal const int UncPrefixLength = 2;
        // \\?\UNC\, \\.\UNC\
        internal const int UncExtendedPrefixLength = 8;
#endif

        /// <summary>
        /// Gets the length of the root of the path (drive, share, etc.).
        /// </summary>
        internal static int GetRootLength(string path)
        {
            var pathLength = path.Length;
            var i = 0;

#if PATH_SUPPORTS_UNC
            var deviceSyntax = IsDevice(path);
            var deviceUnc = deviceSyntax && IsDeviceUNC(path);

            if ((!deviceSyntax || deviceUnc) && pathLength > 0 && IsDirectorySeparator(path[0]))
            {
                // UNC or simple rooted path (e.g. "\foo", NOT "\\?\C:\foo")
                if (deviceUnc || (pathLength > 1 && IsDirectorySeparator(path[1])))
                {
                    // UNC (\\?\UNC\ or \\), scan past server\share

                    // Start past the prefix ("\\" or "\\?\UNC\")
                    i = deviceUnc ? UncExtendedPrefixLength : UncPrefixLength;

                    // Skip two separators at most
                    var n = 2;
                    while (i < pathLength && (!IsDirectorySeparator(path[i]) || --n > 0))
                    {
                        i++;
                    }
                }
                else
                {
                    // Current drive rooted (e.g. "\foo")
                    i = 1;
                }
            }
            else if (deviceSyntax)
            {
                // Device path (e.g. "\\?\.", "\\.\")
                // Skip any characters following the prefix that aren't a separator
                i = DevicePrefixLength;
                while (i < pathLength && !IsDirectorySeparator(path[i]))
                {
                    i++;
                }

                // If there is another separator take it, as long as we have had at least one
                // non-separator after the prefix (e.g. don't take "\\?\\", but take "\\?\a\")
                if (i < pathLength && i > DevicePrefixLength && IsDirectorySeparator(path[i]))
                {
                    i++;
                }
            }
            else if (pathLength >= 2 && path[1] == VolumeSeparatorChar && IsValidDriveChar(path[0]))
            {
                // Valid drive specified path ("C:", "D:", etc.)
                i = 2;

                // If the colon is followed by a directory separator, move past it (e.g "C:\")
                if (pathLength > 2 && IsDirectorySeparator(path[2]))
                {
                    i++;
                }
            }
#else
            if (pathLength >= 2 && path[1] == VolumeSeparatorChar && IsValidDriveChar(path[0]))
            {
                // Valid drive specified path ("C:", "D:", etc.)
                i = 2;

                // If the colon is followed by a directory separator, move past it (e.g "C:\")
                if (pathLength > 2 && IsDirectorySeparator(path[2]))
                {
                    i++;
                }
            }
            else if (pathLength == 1 && IsDirectorySeparator(path[0]))
            {
                // Current drive rooted (e.g. "\foo")
                i = 1;
            }
#endif

            return i;
        }

#if PATH_SUPPORTS_UNC
        /// <summary>
        /// Returns true if the path uses any of the DOS device path syntaxes. ("\\.\", "\\?\", or "\??\")
        /// </summary>
        internal static bool IsDevice(string path)
        {
            // If the path begins with any two separators is will be recognized and normalized and prepped with
            // "\??\" for internal usage correctly. "\??\" is recognized and handled, "/??/" is not.
            return IsExtended(path)
                   ||
                   (
                       path.Length >= DevicePrefixLength
                       && IsDirectorySeparator(path[0])
                       && IsDirectorySeparator(path[1])
                       && (path[2] == '.' || path[2] == '?')
                       && IsDirectorySeparator(path[3])
                   );
        }

        /// <summary>
        /// Returns true if the path is a device UNC (\\?\UNC\, \\.\UNC\)
        /// </summary>
        internal static bool IsDeviceUNC(string path)
        {
            return path.Length >= UncExtendedPrefixLength
                   && IsDevice(path)
                   && IsDirectorySeparator(path[7])
                   && path[4] == 'U'
                   && path[5] == 'N'
                   && path[6] == 'C';
        }
#endif

        /// <summary>
        /// True if the given character is a directory separator.
        /// </summary>
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsDirectorySeparator(char c)
        {
            return c is DirectorySeparatorChar or AltDirectorySeparatorChar;
        }

        /// <summary>
        /// Returns true if the path is effectively empty for the current OS.
        /// For unix, this is empty or null. For Windows, this is empty, null, or
        /// just spaces ((char)32).
        /// </summary>
        internal static bool IsEffectivelyEmpty(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return true;
            }

            foreach (var c in path.ToCharArray())
            {
                if (c != ' ')
                {
                    return false;
                }
            }

            return true;
        }

#if PATH_SUPPORTS_UNC
        /// <summary>
        /// Returns true if the path uses the canonical form of extended syntax ("\\?\" or "\??\"). If the
        /// path matches exactly (cannot use alternate directory separators) Windows will skip normalization
        /// and path length checks.
        /// </summary>
        internal static bool IsExtended(string path)
        {
            // While paths like "//?/C:/" will work, they're treated the same as "\\.\" paths.
            // Skipping of normalization will *only* occur if back slashes ('\') are used.
            return path.Length >= DevicePrefixLength
                   && path[0] == '\\'
                   && (path[1] == '\\' || path[1] == '?')
                   && path[2] == '?'
                   && path[3] == '\\';
        }
#endif

        /// <summary>
        /// Returns true if the given character is a valid drive letter
        /// </summary>
        internal static bool IsValidDriveChar(char value)
        {
            return (uint)((value | 0x20) - 'a') <= (uint)('z' - 'a');
        }

        /// <summary>
        /// Normalize separators in the given path. Converts forward slashes into back slashes and compresses slash runs, keeping initial 2 if present.
        /// Also trims initial whitespace in front of "rooted" paths (see PathStartSkip).
        ///
        /// This effectively replicates the behavior of the legacy NormalizePath when it was called with fullCheck=false and expandShortpaths=false.
        /// The current NormalizePath gets directory separator normalization from Win32's GetFullPathName(), which will resolve relative paths and as
        /// such can't be used here (and is overkill for our uses).
        ///
        /// Like the current NormalizePath this will not try and analyze periods/spaces within directory segments.
        /// </summary>
        /// <remarks>
        /// The only callers that used to use Path.Normalize(fullCheck=false) were Path.GetDirectoryName() and Path.GetPathRoot(). Both usages do
        /// not need trimming of trailing whitespace here.
        ///
        /// GetPathRoot() could technically skip normalizing separators after the second segment- consider as a future optimization.
        ///
        /// For legacy .NET Framework behavior with ExpandShortPaths:
        ///  - It has no impact on GetPathRoot() so doesn't need consideration.
        ///  - It could impact GetDirectoryName(), but only if the path isn't relative (C:\ or \\Server\Share).
        ///
        /// In the case of GetDirectoryName() the ExpandShortPaths behavior was undocumented and provided inconsistent results if the path was
        /// fixed/relative. For example: "C:\PROGRA~1\A.TXT" would return "C:\Program Files" while ".\PROGRA~1\A.TXT" would return ".\PROGRA~1". If you
        /// ultimately call GetFullPath() this doesn't matter, but if you don't or have any intermediate string handling could easily be tripped up by
        /// this undocumented behavior.
        ///
        /// We won't match this old behavior because:
        ///
        ///   1. It was undocumented
        ///   2. It was costly (extremely so if it actually contained '~')
        ///   3. Doesn't play nice with string logic
        ///   4. Isn't a cross-plat friendly concept/behavior
        /// </remarks>
        [return: NotNullIfNotNull("path")]
        internal static string NormalizeDirectorySeparators(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            bool hasNavigationComponents = false;

            var components = path.Split(new[]
            {
                DirectorySeparatorChar,
                AltDirectorySeparatorChar
            });
            var resultComponents = new string[components.Length];
            var resultIndex = 0;

            for (int i = 0; i < components.Length; i++)
            {
                var component = components[i];

                if (component == "..")
                {
                    // We're navigating, so remember that
                    hasNavigationComponents = true;

                    // If the previous component is also "..", or if it's the start of the path, add ".." to the result
                    if (i == 0 && (resultIndex == 0
                                   || resultComponents[resultIndex - 1] == ".."))
                    {
                        resultComponents[resultIndex] = "..";
                        resultIndex++;
                    }
                    else
                    {
                        // Otherwise, go up one directory level, if we're not already at the root
                        if (resultIndex > 0)
                        {
                            resultIndex--;
                        }
                    }
                }
                else if (component != "."
                         && component != "")
                {
                    resultComponents[resultIndex] = component;
                    resultIndex++;
                }
            }

            var builder = new StringBuilder();

            // if the original path started with a directory separator, ensure the result does too
            // unless there were navigation components, in which case we can't start with a separator
            if (!hasNavigationComponents && (path.StartsWith(DirectorySeparatorCharAsString)
                                             || path.StartsWith(AltDirectorySeparatorChar.ToString())))
            {
                builder.Append(DirectorySeparatorChar);
            }

            for (var i = 0; i < resultIndex; i++)
            {
                if (i > 0
                    && builder.Length > 0)
                {
                    builder.Append(DirectorySeparatorChar);
                }

                builder.Append(resultComponents[i]);
            }

            // if the original path ended with a directory separator, ensure the result does too
            if (path.EndsWith(DirectorySeparatorCharAsString)
                || path.EndsWith(AltDirectorySeparatorChar.ToString()))
            {
                builder.Append(DirectorySeparatorChar);
            }

            return builder.ToString();
        }
    }
}
