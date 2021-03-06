﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Composition;
using Microsoft.CodeAnalysis.Razor;
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.LanguageServerClient.Razor
{
    [Shared]
    [Export(typeof(RazorLSPTextDocumentCreatedListener))]
    internal class RazorLSPTextDocumentCreatedListener
    {
        private static readonly Guid HtmlLanguageServiceGuid = new Guid("9BBFD173-9770-47DC-B191-651B7FF493CD");

        private readonly TrackingLSPDocumentManager _lspDocumentManager;
        private readonly ITextDocumentFactoryService _textDocumentFactory;
        private readonly LSPEditorFeatureDetector _lspEditorFeatureDetector;
        private readonly SVsServiceProvider _serviceProvider;
        private readonly IEditorOptionsFactoryService _editorOptionsFactory;
        private readonly IContentType _razorLSPContentType;

        [ImportingConstructor]
        public RazorLSPTextDocumentCreatedListener(
            ITextDocumentFactoryService textDocumentFactory,
            IContentTypeRegistryService contentTypeRegistry,
            LSPDocumentManager lspDocumentManager,
            LSPEditorFeatureDetector lspEditorFeatureDetector,
            SVsServiceProvider serviceProvider,
            IEditorOptionsFactoryService editorOptionsFactory)
        {
            if (textDocumentFactory is null)
            {
                throw new ArgumentNullException(nameof(textDocumentFactory));
            }

            if (contentTypeRegistry is null)
            {
                throw new ArgumentNullException(nameof(contentTypeRegistry));
            }

            if (lspDocumentManager is null)
            {
                throw new ArgumentNullException(nameof(lspDocumentManager));
            }

            if (lspEditorFeatureDetector is null)
            {
                throw new ArgumentNullException(nameof(lspEditorFeatureDetector));
            }

            if (serviceProvider is null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            if (editorOptionsFactory is null)
            {
                throw new ArgumentNullException(nameof(editorOptionsFactory));
            }

            _lspDocumentManager = lspDocumentManager as TrackingLSPDocumentManager;

            if (_lspDocumentManager is null)
            {
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
                throw new ArgumentException("The LSP document manager should be of type " + typeof(TrackingLSPDocumentManager).FullName, nameof(_lspDocumentManager));
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
            }

            _textDocumentFactory = textDocumentFactory;
            _lspEditorFeatureDetector = lspEditorFeatureDetector;
            _serviceProvider = serviceProvider;
            _editorOptionsFactory = editorOptionsFactory;

            _textDocumentFactory.TextDocumentCreated += TextDocumentFactory_TextDocumentCreated;
            _textDocumentFactory.TextDocumentDisposed += TextDocumentFactory_TextDocumentDisposed;
            _razorLSPContentType = contentTypeRegistry.GetContentType(RazorLSPContentTypeDefinition.Name);
        }

        // Internal for testing
        internal void TextDocumentFactory_TextDocumentCreated(object sender, TextDocumentEventArgs args)
        {
            if (args is null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            if (!IsRazorLSPTextDocument(args.TextDocument))
            {
                return;
            }

            var textBuffer = args.TextDocument.TextBuffer;
            if (!textBuffer.ContentType.IsOfType(RazorLSPContentTypeDefinition.Name))
            {
                // This Razor text buffer has yet to be initialized.

                InitializeRazorLSPTextBuffer(textBuffer);
            }
        }

        // Internal for testing
        internal void TextDocumentFactory_TextDocumentDisposed(object sender, TextDocumentEventArgs args)
        {
            if (args is null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            // Do a lighter check to see if we care about this document.
            if (IsRazorFilePath(args.TextDocument.FilePath))
            {
                // If we don't know about this document we'll no-op
                _lspDocumentManager.UntrackDocument(args.TextDocument.TextBuffer);
            }
        }

        // Internal for testing
        internal bool IsRazorLSPTextDocument(ITextDocument textDocument)
        {
            var filePath = textDocument.FilePath;
            if (filePath == null)
            {
                return false;
            }

            if (!IsRazorFilePath(filePath))
            {
                return false;
            }

            // We pass a `null` hierarchy so we don't eagerly lookup hierarchy information before it's needed.
            if (!_lspEditorFeatureDetector.IsLSPEditorAvailable(textDocument.FilePath, hierarchy: null))
            {
                return false;
            }

            return true;
        }

        private static bool IsRazorFilePath(string filePath)
        {
            if (filePath == null)
            {
                return false;
            }

            if (!filePath.EndsWith(RazorLSPContentTypeDefinition.CSHTMLFileExtension, FilePathComparison.Instance) &&
                !filePath.EndsWith(RazorLSPContentTypeDefinition.RazorFileExtension, FilePathComparison.Instance))
            {
                // Not a Razor file
                return false;
            }

            return true;
        }

        private void InitializeRazorLSPTextBuffer(ITextBuffer textBuffer)
        {
            if (_lspEditorFeatureDetector.IsRemoteClient())
            {
                // We purposefully do not set ClientName's in remote client scenarios because we don't want to boot 2 langauge servers (one for both host and client).
                // The ClientName controls whether or not an ILanguageClient instantiates.

                // We still change the content type for remote scenarios in order to enable our TextMate grammar to light up the Razor editor properly.
                textBuffer.ChangeContentType(_razorLSPContentType, editTag: null);
            }
            else
            {
                // ClientName controls if the LSP infrastructure in VS will boot when it detects our Razor LSP contennt type. If the property exists then it will; otherwise
                // the text buffer will be ignored by the LSP 
                textBuffer.Properties.AddProperty(LanguageClientConstants.ClientNamePropertyKey, RazorLanguageServerClient.ClientName);

                // Initialize the buffer with editor options.
                InitializeOptions(textBuffer);

                textBuffer.ChangeContentType(_razorLSPContentType, editTag: null);

                // Must track the document after changing the content type so any LSPDocuments created understand they're being created for a Razor LSP document.
                _lspDocumentManager.TrackDocument(textBuffer);
            }
        }

        private void InitializeOptions(ITextBuffer textBuffer)
        {
            // Ideally we would initialize options based on Razor specific options in the context menu.
            // But since we don't have support for that yet, we will temporarily use the settings from Html.

            var textManager = _serviceProvider.GetService(typeof(SVsTextManager)) as IVsTextManager2;
            Assumes.Present(textManager);

            var langPrefs2 = new LANGPREFERENCES2[] { new LANGPREFERENCES2() { guidLang = HtmlLanguageServiceGuid } };
            if (VSConstants.S_OK == textManager.GetUserPreferences2(null, null, langPrefs2, null))
            {
                var insertSpaces = langPrefs2[0].fInsertTabs == 0;
                var tabSize = langPrefs2[0].uTabSize;

                var razorOptions = _editorOptionsFactory.GetOptions(textBuffer);
                razorOptions.SetOptionValue(DefaultOptions.ConvertTabsToSpacesOptionId, insertSpaces);
                razorOptions.SetOptionValue(DefaultOptions.TabSizeOptionId, (int)tabSize);
            }
        }
    }
}
