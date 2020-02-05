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
    public static class SampleService
    {
        //Query to retrieve samples data
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

        public static async Task<List<Repo>> GetSamples(IConnection connection)
        {
            //run query 
            var samples = await connection.Run(samplesQuery);
            return samples.ToList();
        }

        //Get languages list from parsed yaml header
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

        //Get services list from the parsed yaml header
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

        //fetch yaml header from sample repo and parse it
        private static async Task<string> GetYamlHeader(string sampleName)
        {
            //run query 
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
