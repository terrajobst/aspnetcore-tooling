﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis.Razor;

namespace Microsoft.AspNetCore.Razor.LanguageServer.Common
{
    public class FilePathNormalizer
    {
        public string NormalizeDirectory(string directoryFilePath)
        {
            var normalized = Normalize(directoryFilePath);

            if (!normalized.EndsWith("/"))
            {
                normalized += '/';
            }

            return normalized;
        }

        public string Normalize(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return "/";
            }

            var decodedPath = WebUtility.UrlDecode(filePath);
            var normalized = decodedPath.Replace('\\', '/');

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) &&
                normalized[0] != '/')
            {
                normalized = '/' + normalized;
            }

            return normalized;
        }

        public string NormalizeForRead(string filePath)
        {
            filePath = Normalize(filePath);

            if (filePath[0] == '/')
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) &&
                    !filePath.StartsWith("//", StringComparison.OrdinalIgnoreCase)) // Network UNC paths
                {
                    // VSLS path, not understood by File.OpenRead so we need to strip the leading separator.
                    filePath = filePath.Substring(1);
                }
                else
                {
                    // Unix system, path starts with / which is allowed by File.OpenRead on non-windows.
                }
            }

            return filePath;
        }

        public string GetDirectory(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new InvalidOperationException(filePath);
            }

            var normalizedPath = Normalize(filePath);
            var lastSeparatorIndex = normalizedPath.LastIndexOf('/');

            var directory = normalizedPath.Substring(0, lastSeparatorIndex + 1);
            return directory;
        }

        public bool FilePathsEquivalent(string filePath1, string filePath2)
        {
            var normalizedFilePath1 = Normalize(filePath1);
            var normalizedFilePath2 = Normalize(filePath2);

            return FilePathComparer.Instance.Equals(normalizedFilePath1, normalizedFilePath2);
        }
    }
}
