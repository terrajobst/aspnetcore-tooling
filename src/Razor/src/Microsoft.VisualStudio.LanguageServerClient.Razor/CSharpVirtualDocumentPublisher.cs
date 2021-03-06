﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Razor.Workspaces;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;

namespace Microsoft.VisualStudio.LanguageServerClient.Razor
{
    [Shared]
    [Export(typeof(LSPDocumentManagerChangeTrigger))]
    internal class CSharpVirtualDocumentPublisher : LSPDocumentManagerChangeTrigger
    {
        private readonly RazorDynamicFileInfoProvider _dynamicFileInfoProvider;

        [ImportingConstructor]
        public CSharpVirtualDocumentPublisher(RazorDynamicFileInfoProvider dynamicFileInfoProvider)
        {
            if (dynamicFileInfoProvider is null)
            {
                throw new ArgumentNullException(nameof(dynamicFileInfoProvider));
            }

            _dynamicFileInfoProvider = dynamicFileInfoProvider;
        }

        public override void Initialize(LSPDocumentManager documentManager)
        {
            if (documentManager is null)
            {
                throw new ArgumentNullException(nameof(documentManager));
            }

            documentManager.Changed += DocumentManager_Changed;
        }

        // Internal for testing
        internal void DocumentManager_Changed(object sender, LSPDocumentChangeEventArgs args)
        {
            if (args.Kind != LSPDocumentChangeKind.VirtualDocumentChanged)
            {
                return;
            }

            if (args.VirtualNew is CSharpVirtualDocumentSnapshot)
            {
                var csharpContainer = new CSharpVirtualDocumentContainer(args.VirtualNew.Snapshot);
                _dynamicFileInfoProvider.UpdateLSPFileInfo(args.New.Uri, csharpContainer);
            }
        }

        private class CSharpVirtualDocumentContainer : DynamicDocumentContainer
        {
            private readonly ITextSnapshot _textSnapshot;

            public CSharpVirtualDocumentContainer(ITextSnapshot textSnapshot)
            {
                if (textSnapshot is null)
                {
                    throw new ArgumentNullException(nameof(textSnapshot));
                }

                _textSnapshot = textSnapshot;
            }

            public override string FilePath => throw new NotImplementedException();

            public override object GetExcerptService()
            {
                return null;
            }

            public override object GetMappingService()
            {
                return null;
            }

            public override TextLoader GetTextLoader(string filePath)
            {
                var sourceText = _textSnapshot.AsText();
                var textLoader = new SourceTextLoader(sourceText, filePath);
                return textLoader;
            }

            private sealed class SourceTextLoader : TextLoader
            {
                private readonly SourceText _sourceText;
                private readonly string _filePath;

                public SourceTextLoader(SourceText sourceText, string filePath)
                {
                    if (sourceText is null)
                    {
                        throw new ArgumentNullException(nameof(sourceText));
                    }

                    if (filePath is null)
                    {
                        throw new ArgumentNullException(nameof(filePath));
                    }

                    _sourceText = sourceText;
                    _filePath = filePath;
                }

                public override Task<TextAndVersion> LoadTextAndVersionAsync(Workspace workspace, DocumentId documentId, CancellationToken cancellationToken)
                {
                    return Task.FromResult(TextAndVersion.Create(_sourceText, VersionStamp.Default, _filePath));
                }
            }
        }
    }
}
