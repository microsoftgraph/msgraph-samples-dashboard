// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SamplesDashboard.Models
{
    public class YamlHeaderExtensions
    {
        public string[] Services { get; set; }
    }
    public class YamlHeader
    {
        public string[] Languages { get; set; }
        public YamlHeaderExtensions Extensions { get; set; }
        public string DependencyFile { get; set; }
        public bool NoDependencies { get; set; }
        internal bool NoDependenciesWasSpecified { get; set; }

        public void MergeWith(YamlHeader mergeTarget)
        {
            if (mergeTarget == null) return;

            Languages = Languages ?? mergeTarget.Languages;
            Extensions = Extensions ?? mergeTarget.Extensions;
            DependencyFile = DependencyFile ?? mergeTarget.DependencyFile;
            NoDependencies = NoDependenciesWasSpecified ? NoDependencies : NoDependencies || mergeTarget.NoDependencies;
        }

        public static YamlHeader FromString(string headerContent)
        {
            var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .IgnoreUnmatchedProperties()
                    .Build();
            var header = deserializer.Deserialize<YamlHeader>(headerContent);

            if (headerContent.Contains("noDependencies"))
            {
                header.NoDependenciesWasSpecified = true;
            }

            return header;
        }

        public static async Task<YamlHeader> GetFromRepo(HttpClient httpClient,
                                                         string repoName,
                                                         string branch)
        {
            var currentHeader = await GetFromReadme(httpClient, repoName, branch);
            var repoNameFileHeader = await GetFromFile(httpClient, repoName, branch, $"{repoName}.yml");
            if (repoNameFileHeader != null)
            {
                repoNameFileHeader.MergeWith(currentHeader);
                currentHeader = repoNameFileHeader;
            }

            var devxFileHeader = await GetFromFile(httpClient, repoName, branch, "devx.yml");
            if (devxFileHeader != null)
            {
                devxFileHeader.MergeWith(currentHeader);
                currentHeader = devxFileHeader;
            }

            return currentHeader;

        }

        public string LanguagesString 
        {
            get
            {
                return Languages == null ? "" : string.Join(",", Languages);
            }
        }

        public string ServicesString
        {
            get
            {
                return (Extensions == null || Extensions.Services == null) ? 
                    "" : string.Join(",", Extensions.Services);
            }
        }

        private static async Task<YamlHeader> GetFromReadme(HttpClient httpClient,
                                                            string repoName,
                                                            string branch)
        {
            var responseMessage = await httpClient.GetAsync(
                $"https://raw.githubusercontent.com/microsoftgraph/{repoName}/{branch}/README.md");

            if (responseMessage.StatusCode.ToString().Equals("NotFound"))
                responseMessage = await httpClient.GetAsync(
                    $"https://raw.githubusercontent.com/microsoftgraph/{repoName}/{branch}/Readme.md");

            if (responseMessage.IsSuccessStatusCode)
            {
                var fileContents = await responseMessage.Content.ReadAsStringAsync();
                var stringSeparator = new string[] { "\r\n---\r\n", "\n---\n" };
                var parts = fileContents.Split(stringSeparator, StringSplitOptions.RemoveEmptyEntries);

                //we have a valid header between ---
                if (parts.Length > 1)
                {
                    var header = parts[0];
                    if (header.StartsWith("---"))
                    {
                        header = header.Substring(3);
                    }
                    return FromString(header);
                }
            }

            return null;
        }

        private static async Task<YamlHeader> GetFromFile(HttpClient httpClient,
                                                          string repoName,
                                                          string branch,
                                                          string filePath)
        {
            var responseMessage = await httpClient.GetAsync(
                $"https://raw.githubusercontent.com/microsoftgraph/{repoName}/{branch}/{filePath}");

            if (responseMessage.IsSuccessStatusCode)
            {
                var fileContents = await responseMessage.Content.ReadAsStringAsync();
                return FromString(fileContents);
            }

            return null;
        }
    }
}
