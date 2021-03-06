﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Razor.LanguageServer.Common;
using Microsoft.CodeAnalysis.Razor.ProjectSystem;

namespace Microsoft.AspNetCore.Razor.LanguageServer
{
    internal abstract class DocumentVersionCache : ProjectSnapshotChangeTrigger, IRazorFileChangeListener
    {
        public abstract bool TryGetDocumentVersion(DocumentSnapshot documentSnapshot, out long version);

        public abstract void TrackDocumentVersion(DocumentSnapshot documentSnapshot, long version);

        public abstract void RazorFileChanged(string filePath, RazorFileChangeKind kind);
    }
}
