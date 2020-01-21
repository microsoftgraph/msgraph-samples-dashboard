using System;
//using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Octokit.GraphQL;
using System.Collections.Generic;
using SamplesDashboard.Models;
using Octokit.GraphQL.Model;

namespace SamplesDashboard.Services
{
    public static class SampleService
    {
        public static async Task<List<Repo>> GetSamples(Connection connection)
        {
            List<PullRequestState> pullRequestStates = new List<PullRequestState>
            {
                PullRequestState.Open
            };
            List<IssueState> issueStates = new List<IssueState> 
            {
                IssueState.Open 
            };

            var query = new Query()
                 .RepositoryOwner("microsoftgraph")
                 .Repositories(first: 50)
                 .Nodes
                 .Select(r => new Repo { 
                     Name = r.Name,
                     Owner = r.Owner.Login,
                     //Status
                     Language = r.PrimaryLanguage.Name,
                     PullRequests = r.PullRequests(null, null, null, null, null, null, null, null, pullRequestStates ).TotalCount,
                     Issues = r.Issues(null, null, null, null, null, null, null, issueStates).TotalCount,
                     Stars = r.Stargazers(null, null, null, null, null).TotalCount
                    //featurearea
                });;;
             
            //run query 
            var result = await connection.Run(query);

            return result.ToList();
        }
    }
}
