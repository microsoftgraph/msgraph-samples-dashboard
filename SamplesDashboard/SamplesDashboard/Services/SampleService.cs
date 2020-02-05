using System.Linq;
using System.Threading.Tasks;
using Octokit.GraphQL;
using System.Collections.Generic;
using SamplesDashboard.Models;
using Octokit.GraphQL.Model;
using System.Net.Http;
using System;

namespace SamplesDashboard.Services
{
    /// <summary>
    ///  This class contains samples query services and functions to be used by the samples API
    /// </summary>
    public static class SampleService
    {
        ///Query to retrieve samples data
        private static readonly ICompiledQuery<IEnumerable<Repo>> samplesQuery
            = new Query().Search("org:microsoftgraph archived:false training OR sample in:name", SearchType.Repository, 100)
            .Nodes
            .Select(node => node.Switch<Repo>(
               when => when.Repository(
                   repo => new Repo()
                   {
                       Name = repo.Name,
                       Owner = repo.Owner.Login,
                       Issues = repo.Issues(null, null, null, null, null, null, null, new List<IssueState> { IssueState.Open }).TotalCount,
                       Stars = repo.Stargazers(null, null, null, null, null).TotalCount,
                       PullRequests = repo.PullRequests(null, null, null, null, null, null, null, null, new List<PullRequestState> { PullRequestState.Open }).TotalCount,
                       })
               )
            ).Compile(); 
        
        /// <summary>
        /// Gets the connection object used to run the samples query and return the samples list 
        /// </summary>
        /// <param name="connection"></param>
        /// <returns> A list of samples.</returns>
        public static async Task<List<Repo>> GetSamples(IConnection connection)
        {
            //run query 
            var samples = await connection.Run(samplesQuery);
            return samples.ToList();
        }

        /// <summary>
        /// Gets languages list from parsed yaml header
        /// </summary>
        /// <param name="sampleName"></param>
        /// <returns> A new list of languages after parsing the yaml header.</returns>
        public static async Task<List<String>> GetLanguages(string sampleName)
        {
            string header = await GetYamlHeader(sampleName);
            if (!string.IsNullOrEmpty(header))
            {
                string[] lines = header.Split("\r\n");
                return SearchTerm("languages", lines);
            }
            return new List<string>();
        }

        /// <summary>
        /// Get services list from the parsed yaml header
        /// </summary>
        /// <param name="sampleName"></param>
        /// <returns> A new list of services in the yaml header after parsing it.</returns>
        public static async Task<List<string>> GetFeatures(string sampleName)
        {
            string header = await GetYamlHeader(sampleName);
            if (!string.IsNullOrEmpty(header))
            {
                string[] lines = header.Split("\r\n");
                return SearchTerm("services", lines);
            }
            return new List<string>();
        }

        /// <summary>
        /// fetch yaml header from sample repo and parse it
        /// </summary>
        /// <param name="sampleName"></param>
        /// <returns> The yaml header. </returns>
        private static async Task<string> GetYamlHeader(string sampleName)
        {
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage responseMessage  = await httpClient.GetAsync(string.Concat("https://raw.githubusercontent.com/microsoftgraph/", sampleName, "/master/Readme.md"));
            
            if (responseMessage.StatusCode.ToString().Equals("NotFound"))
                responseMessage = await httpClient.GetAsync(string.Concat("https://raw.githubusercontent.com/microsoftgraph/", sampleName, "/master/README.md"));

            if (responseMessage.IsSuccessStatusCode)
            {
                string fileContents = await responseMessage.Content.ReadAsStringAsync();
                string[] parts = fileContents.Split("---", StringSplitOptions.RemoveEmptyEntries);
                //we have a valid header between ---
                if (parts.Length > 1)
                {
                    return parts[0];
                }                
            }
            return "";
        }

        /// <summary>
        /// Gets the searchterm and gets related entries.
        /// </summary>
        /// <param name="term"></param>
        /// <param name="lines"></param>
        /// <returns>A list of the searchterm specified.</returns>
        private static List<string> SearchTerm(string term, string[] lines)
        {
            bool foundHeader = false;
            List<string> myList = new List<string>();

            foreach (var line in lines)
            {
                if (line.Contains(term))
                {
                    foundHeader = true;
                    continue;
                }
                if (foundHeader)
                {
                    if (line.Contains(":"))
                        break;
                    myList.Add(line.Split("-", StringSplitOptions.RemoveEmptyEntries).Last().Trim());
                }
            }
            return myList;
        }
    }
}
