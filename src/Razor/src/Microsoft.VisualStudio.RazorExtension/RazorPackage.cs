// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;
using System.Threading;
//using Microsoft.VisualStudio.LanguageServerClient.Razor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.VisualStudio.RazorExtension
{
    //[AboutDialogInfo(PackageGuidString, "ASP.NET Core Razor Language Services", "#110", "#112", IconResourceID = "#400")]
    //[ProvideEditorFactory(typeof(RazorEditorFactory), 101)]
    //[ProvideEditorExtension(typeof(RazorEditorFactory), RazorLSPContentTypeDefinition.CSHTMLFileExtension, 32, NameResourceID = 101)]
    //[ProvideEditorExtension(typeof(RazorEditorFactory), RazorLSPContentTypeDefinition.RazorFileExtension, 32, NameResourceID = 101)]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    [Guid(PackageGuidString)]
    public sealed class RazorPackage : AsyncPackage
    {
        public const string PackageGuidString = "13b72f58-279e-49e0-a56d-296be02f0805";

        private const string CSharpPackageIdString = "13c3bbb4-f18f-4111-9f54-a0fb010d9194";

        //private RazorEditorFactory _editorFactory;

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            // We need to force the CSharp package to load. That's responsible for the initialization
            // of the remote host client.
            var shell = GetService(typeof(SVsShell)) as IVsShell;
            if (shell == null)
            {
                return;
            }

            IVsPackage package = null;
            var packageGuid = new Guid(CSharpPackageIdString);
            shell.LoadPackage(ref packageGuid, out package);

            //_editorFactory = new RazorEditorFactory(this);
            //RegisterEditorFactory(_editorFactory);
        }
    }
}
