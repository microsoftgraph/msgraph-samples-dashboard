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
                       //Language = repo.PrimaryLanguage.Select(lang => lang.Name).Single(),
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

            foreach (var sample in samples)
            {
                //links to sample names
                string str1 = "https://raw.githubusercontent.com/microsoftgraph/";
                string str2 = "/master/Readme.md";

                //download yaml file and parsing it
                HttpClient httpClient = new HttpClient();
                HttpResponseMessage reponseMessage = await httpClient.GetAsync(string.Concat(str1, sample.Name, str2));
                string fileContents = await reponseMessage.Content.ReadAsStringAsync();
                string[] parts = fileContents.Split("---", StringSplitOptions.RemoveEmptyEntries);
                string header = parts[0];

                string[] lines = header.Split("\r\n");

                //Getting language and storing it to a list
                bool foundLanguageHeader = false;
                bool foundServicesHeader = false;
                List<string> languageList = new List<string>();
                List<string> servicesList = new List<string>();

                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains("language"))
                    {
                        foundLanguageHeader = true;
                        continue;
                    }
                    if (foundLanguageHeader)
                    {
                        if (lines[i].Contains(":"))
                            break;
                        languageList.Add(lines[i].Split("-", StringSplitOptions.RemoveEmptyEntries).Last());                       
                    }
                    if (lines[i].Contains("services"))
                    {
                        foundServicesHeader = true;
                        continue;
                    }
                    if (foundServicesHeader)
                    {
                        if (lines[i].Contains(":"))
                            break;
                        servicesList.Add(lines[i].Split("-", StringSplitOptions.RemoveEmptyEntries).Last());
                    }
                }
                
                sample.FeatureArea = servicesList;
                sample.Language = languageList;
            }
            return samples.ToList();
        }
        //public static async Task<List<Repo>> GetYamlData(IConnection connection)
        //{
        //    var samples = await GetSamples(connection);

        //    List<string> languageList = new List<string>();
        //    List<string> servicesList = new List<string>();


        //    foreach (var sample in samples)
        //    {
        //        //links to sample names
        //        string str1 = "https://raw.githubusercontent.com/microsoftgraph/";
        //        string str2 = "/master/README.md";

        //        //download yaml file and parsing it
        //        HttpClient httpClient = new HttpClient();
        //        HttpResponseMessage reponseMessage = await httpClient.GetAsync(string.Concat(str1, sample.Name, str2));
        //        string fileContents = await reponseMessage.Content.ReadAsStringAsync();
        //        string[] parts = fileContents.Split("---", StringSplitOptions.RemoveEmptyEntries);
        //        string header = parts[0];

        //        string[] lines = header.Split("\r\n");

        //        //Getting language and storing it to a list
        //        bool foundLanguageHeader = false;

        //        for (int i = 0; i < lines.Length; i++)
        //        {
        //            if (lines[i].Contains("languages"))
        //            {
        //                foundLanguageHeader = true;
        //                continue;
        //            }
        //            if (foundLanguageHeader)
        //            {
        //                if (lines[i].Contains(":"))
        //                    break;
        //                languageList.Add(lines[i].Split("-", StringSplitOptions.RemoveEmptyEntries).Last());
        //            }
        //        }
        //        // getting services
        //        bool foundServicesHeader = false;
        //        for (int i = 0; i < lines.Length; i++)
        //        {
        //            if (lines[i].Contains("services"))
        //            {
        //                foundServicesHeader = true;
        //                continue;
        //            }
        //            if (foundServicesHeader)
        //            {
        //                if (lines[i].Contains(":"))
        //                    break;
        //                servicesList.Add(lines[i].Split("-", StringSplitOptions.RemoveEmptyEntries).Last());
        //            }
        //        }

        //        sample.FeatureArea = servicesList;
        //        sample.Language = languageList;
        //    }
        //    return samples.ToList();
        //}
        
    }
}
