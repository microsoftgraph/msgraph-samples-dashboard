// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System.Collections.Generic;
using SamplesDashboard.Extensions;

namespace SamplesDashboard
{
    public static class Constants
    {
        private static readonly IEnumerable<string> SdkList = new[]
        {
            "sdk", "microsoft-graph-explorer", "devx-api", "raptor", "\\\"msgraph-cli\\\"", "samples-dashboard"
        };
        private static readonly IEnumerable<string> SamplesList = new[] {"sample", "training"};
        public static readonly string Sdks = SdkList.BuildQueryString();
        public static readonly string Samples = SamplesList.BuildQueryString();
        public const string Timeout = "timeout";

        // Names of known identity libraries from Microsoft
        // as they appear in dependency graph or file.
        // MUST be converted to lowercase
        public static readonly string[] IdentityLibraries =
        {
            // .NET
            "microsoft.identity.client",
            "microsoft.identity.web",
            "microsoft.identitymodel.clients.activedirectory",
            "microsoft.authentication.webassembly.msal",
            // JavaScript
            "@azure/msal-node",
            "@azure/msal-browser",
            "@azure/msal-react",
            "@azure/msal-angular",
            "@azure/msal-angularjs",
            "msal", // Also Python, Obj-C
            "adal-angular",
            "passport-azure-ad",
            // Java
            "com.microsoft.identity.client:msal",
            "com.microsoft.azure:msal4j",
            "com.azure:azure-identity",
            "com.microsoft.aad:adal",
            // Ruby
            "adal"
        };

        // Names of known Graph SDKs from Microsoft
        // as they appear in dependency graph or file.
        // MUST be converted to lowercase
        public static readonly string[] GraphSdks = {
            // .NET
            "microsoft.graph",
            "microsoft.graph.beta",
            // JavaScript
            "@microsoft/microsoft-graph-client",
            // Java
            "com.microsoft.graph:microsoft-graph",
            "com.microsoft.graph:microsoft-graph-core",
            "com.microsoft.graph:microsoft-graph-beta",
            // PHP
            "microsoft/microsoft-graph",
            // Python
            "msgraphcore",
            // Ruby
            "microsoft_graph",
            // Obj-C
            "msgraphclientsdk"
        };

        // Config keys (appsettings.json, user secrets, etc.)
        public static readonly string GitHubToken = "githubToken";
        public static readonly string AzureClientId = "AzureClientId";
        public static readonly string AzureClientSecret = "AzureClientSecret";
        public static readonly string TenantId = "TenantId";
        public static readonly string KeyIdentifier = "KeyIdentifier";

        public static readonly string[] GradleDependencyTypes =
        {
            "implementation",
            "compileOnly",
            "compileClasspath",
            "annotationProcessor",
            "runtimeOnly",
            "runtimeClasspath",
            "testImplementation",
            "testCompileOnly",
            "testCompileClasspath",
            "testRuntimeOnly",
            "testRuntimeClasspath",
            "archives",
            "default"
        };
    }
}
