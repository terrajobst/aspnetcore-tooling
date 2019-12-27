using System.ComponentModel.Composition;
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.LanguageServerClient.Razor
{
    internal sealed class RazorLSPContentTypeDefinition
    {
        public const string Name = "RazorLSP";
        public const string CSHTMLFileExtension = ".ch";
        public const string RazorFileExtension = ".rz";

        /// <summary>
        /// Exports the Razor LSP content type
        /// </summary>
        [Export]
        [Name(Name)]
        [BaseDefinition(CodeRemoteContentDefinition.CodeRemoteContentTypeName)]
        public ContentTypeDefinition RazorLSPContentType { get; set; }

        /// <summary>
        /// Exports the Razor LSP cshtml file extension
        /// </summary>
        [Export]
        [ContentType(Name)]
        [FileExtension(CSHTMLFileExtension)]
        public FileExtensionToContentTypeDefinition RazorCSHTMLFileExtension { get; set; }

        /// <summary>
        /// Exports the Razor LSP razor file extension
        /// </summary>
        [Export]
        [ContentType(Name)]
        [FileExtension(RazorFileExtension)]
        public FileExtensionToContentTypeDefinition RazorRazorFileExtension { get; set; }
    }
}
