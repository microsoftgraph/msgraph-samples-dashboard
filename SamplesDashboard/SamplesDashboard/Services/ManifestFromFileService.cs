// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SamplesDashboard.Models;

namespace SamplesDashboard.Services
{
    /// <summary>
    /// This class builds dependency manifests from a file in a repo
    /// </summary>
    public class ManifestFromFileService
    {
        private readonly IHttpClientFactory _clientFactory;

        public ManifestFromFileService(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        /// <summary>
        /// Builds a dependency graph manifest from a dependency file in the repo
        /// </summary>
        /// <param name="repoName">The name of the repo</param>
        /// <param name="defaultBranch">The default branch of the repo</param>
        /// <param name="dependencyFile">The relative path to the dependency file</param>
        /// <returns> The dependency graph manifest </returns>
        public async Task<GitHubGraphQLDependencyManifest> BuildDependencyManifestFromFile(
          string repoName,
          string defaultBranch,
          string dependencyFile)
        {
            //downloading the dependency file
            var fileType = GetSupportedFileType(dependencyFile);
            if (fileType != SupportedDependencyFileType.Unsupported)
            {
                var httpClient = _clientFactory.CreateClient();
                var responseMessage = await httpClient.GetAsync(
                    $"https://raw.githubusercontent.com/microsoftgraph/{repoName}/{defaultBranch}/{dependencyFile}");

                if (responseMessage.IsSuccessStatusCode)
                {
                    var fileContents = await responseMessage.Content.ReadAsStringAsync();
                    var stringSeparator = new string[] { "\r\n", "\n" };
                    var lines = fileContents.Split(stringSeparator, StringSplitOptions.RemoveEmptyEntries);

                    // Handle file type
                    switch (fileType)
                    {
                        case SupportedDependencyFileType.Gradle:
                            return BuildGradleDependencies(dependencyFile, lines);
                        case SupportedDependencyFileType.PodFile:
                            return BuildPodfileDependencies(dependencyFile, lines);
                        default:
                            break;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Determines the dependency manager type from the file name and extension
        /// </summary>
        /// <param name="dependencyFile">The relative path to the dependency file</param>
        /// <returns> The SupportedDependencyFileType value mapped to the file type </returns>
        internal SupportedDependencyFileType GetSupportedFileType(string dependencyFile)
        {
            var fileExtension = Path.GetExtension(dependencyFile).ToLower();
            var fileName = Path.GetFileNameWithoutExtension(dependencyFile).ToLower();

            var lowered = fileExtension.ToLower();

            if (fileExtension == ".gradle") return SupportedDependencyFileType.Gradle;
            if (fileName == "podfile") return SupportedDependencyFileType.PodFile;

            return SupportedDependencyFileType.Unsupported;
        }

        /// <summary>
        /// Generates a dependency graph manifest from a Gradle file
        /// </summary>
        /// <param name="dependencyFile">The relative path to the Gradle file</param>
        /// <param name="lines">The contents of the Gradle file</param>
        /// <returns> The dependency graph manifest </returns>
        internal GitHubGraphQLDependencyManifest BuildGradleDependencies(string dependencyFile, string[] lines)
        {
            var manifest = GitHubGraphQLDependencyManifest.InitializeForFile(dependencyFile);

            foreach(var line in lines)
            {
                if (Constants.GradleDependencyTypes.Any(type => line.Trim().StartsWith(type)))
                {
                    // Check for string notation
                    // runtimeOnly 'org.springframework:spring-core:2.5'
                    var match = Regex.Match(line, "'(.*):(.*)'");

                    if (match.Success && match.Groups.Count == 3)
                    {
                        manifest.Dependencies.Values.Add(new GitHubGraphQLDependency
                        {
                            PackageManager = Constants.Gradle,
                            PackageName = match.Groups[1].Value,
                            Requirements = $"= {match.Groups[2].Value}"
                        });
                    }
                }
            }

            return manifest;
        }

        /// <summary>
        /// Generates a dependency graph manifest from a Podfile
        /// </summary>
        /// <param name="dependencyFile">The relative path to the Podfile</param>
        /// <param name="lines">The contents of the Podfile</param>
        /// <returns> The dependency graph manifest </returns>
        internal GitHubGraphQLDependencyManifest BuildPodfileDependencies(string dependencyFile, string[] lines)
        {
            var manifest = GitHubGraphQLDependencyManifest.InitializeForFile(dependencyFile);

            foreach(var line in lines)
            {
                if (line.Trim().ToLower().StartsWith("pod"))
                {
                    var match = Regex.Match(line.ToLower(), @"pod\s*'(\w*)',\s*'\D*([\d\.]*)'");

                    if (match.Success && match.Groups.Count == 3)
                    {
                        manifest.Dependencies.Values.Add(new GitHubGraphQLDependency
                        {
                            PackageManager = Constants.CocoaPods,
                            PackageName = match.Groups[1].Value,
                            Requirements = $"=={match.Groups[2].Value}"
                        });
                    }
                }
            }

            return manifest;
        }
    }
}
