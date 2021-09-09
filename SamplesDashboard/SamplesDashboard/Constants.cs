// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using SamplesDashboard.Extensions;

namespace SamplesDashboard
{
    public static class Constants
    {
        // Configuration keys
        public const string Product = "Product";
        public const string ProductVersion = "ProductVersion";
        public const string GitHubOrg = "GitHubOrg";
        public const string GitHubAppId = "GitHubAppId";
        public const string GitHubAppKeyFile = "GitHubAppKeyFile";
        public const string GitHubAppKeyIdentifier = "GitHubAppKeyIdentifier";
        public const string GitHubConcurrency = "GitHubConcurrency";
        public const string KeyVaultUri = "KeyVaultUri";
        public const string KeyVaultAppId = "KeyVaultAppId";
        public const string KeyVaultSecret = "KeyVaultSecret";
        public const string AzureClientId = "AzureAd:ClientId";
        public const string AzureTenantId = "AzureAd:TenantId";
        public const string MSOSClientId = "MSOSClientId";
        public const string MSOSClientSecret = "MSOSClientSecret";

        // Cache keys
        public const string GitHubToken = "GitHubToken";
        public const string CacheLifetime = "CacheLifetime";

        // Key Vault
        public static readonly string[] KeyVaultScopes = { "https://vault.azure.net/.default" };

        // Repo queries
        private static readonly string[] SamplesList = { "sample", "training"};
        public static readonly string Samples = SamplesList.BuildQueryString();

        private static readonly string[] SdkList = { "sdk", "microsoft-graph-explorer", "devx-api", "raptor", "\\\"msgraph-cli\\\"", "samples-dashboard" };
        public static readonly string Sdks = SdkList.BuildQueryString();

        // Package managers
        public const string Nuget = "NUGET";
        public const string Npm = "NPM";
        public const string Gradle = "GRADLE";
        public const string Maven = "MAVEN";
        public const string CocoaPods = "COCOAPODS";

        // Names of known identity libraries from Microsoft
        // as they appear in dependency graph or file.
        // MUST be converted to lowercase
        public static readonly string[] IdentityLibraries =
        {
            // .NET
            "microsoft.identity.client",
            "microsoft.identity.web",
            "microsoft.identity.web.ui",
            "microsoft.identity.web.microsoftgraph",
            "microsoft.identity.web.microsoftgraphbeta",
            "microsoft.identitymodel.clients.activedirectory",
            "microsoft.authentication.webassembly.msal",
            // JavaScript
            "@azure/msal-common",
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

        // Gradle file dependency indicators
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
