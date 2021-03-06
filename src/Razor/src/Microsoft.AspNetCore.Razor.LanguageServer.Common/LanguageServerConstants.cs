﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Razor.LanguageServer.Common
{
    public static class LanguageServerConstants
    {
        public const string ProjectConfigurationFile = "project.razor.json";

        public const string ExpectsCursorPlaceholderKey = "expectsCursorPlaceholder";

        public const string CursorPlaceholderString = "__placeholder__";

        public const string RazorSemanticTokensEndpoint = "razor/semanticTokens";

        public const string RazorSemanticTokenLegendEndpoint = "razor/semanticTokensLegend";

        public const string RazorRangeFormattingEndpoint = "razor/rangeFormatting";

        public const string RazorUpdateCSharpBufferEndpoint = "razor/updateCSharpBuffer";

        public const string RazorUpdateHtmlBufferEndpoint = "razor/updateHtmlBuffer";

        public const string RazorLanguageQueryEndpoint = "razor/languageQuery";

        public const string RazorMapToDocumentRangeEndpoint = "razor/mapToDocumentRange";
    }
}
