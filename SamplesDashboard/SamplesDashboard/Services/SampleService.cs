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
            = new Query().Search("org:microsoftgraph training OR sample in:name", SearchType.Repository, 100)
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

            //iterate through each sample and set the language and feature area
            foreach (var sample in samples)
            {               
                //download yaml file and parsing it
                HttpClient httpClient = new HttpClient();
                HttpResponseMessage reponseMessage = null;
                reponseMessage = await httpClient.GetAsync(string.Concat("https://raw.githubusercontent.com/microsoftgraph/", sample.Name,"/master/Readme.md"));
                if(reponseMessage.StatusCode.ToString().Equals("NotFound"))
                    reponseMessage = await httpClient.GetAsync(string.Concat("https://raw.githubusercontent.com/microsoftgraph/", sample.Name, "/master/README.md"));

                if (reponseMessage.IsSuccessStatusCode)
                {
                    string fileContents = await reponseMessage.Content.ReadAsStringAsync();
                    string[] parts = fileContents.Split("---", StringSplitOptions.RemoveEmptyEntries);
                    string header = parts[0];
                    string[] lines = header.Split("\r\n");

                    sample.Language = SearchTerm("language", lines);
                    sample.FeatureArea = SearchTerm("services", lines);
                }
                else
                {
                    sample.FeatureArea = null;
                    sample.Language = null;
                }
            }
            return samples.ToList();
        }
        private static List<string> SearchTerm(string term, string[] lines)
        {
            bool foundHeader = false;
            List<string> myList = new List<string>();

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains(term))
                {
                    foundHeader = true;
                    continue;
                }
                if (foundHeader)
                {
                    if (lines[i].Contains(":"))
                        break;
                    myList.Add(lines[i].Split("-", StringSplitOptions.RemoveEmptyEntries).Last());
                }
            }
            return myList;
        }
    }
}
