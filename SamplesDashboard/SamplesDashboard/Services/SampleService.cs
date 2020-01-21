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
        //Query to retrieve samples data
        private static readonly ICompiledQuery<IEnumerable<Repo>> samplesQuery = new Query()
                 .RepositoryOwner("microsoftgraph")
                 .Repositories(first: 50)
                 .Nodes
                 .Select(r => new Repo { 
                     Name = r.Name,
                     Owner = r.Owner.Login,
                     Language = r.PrimaryLanguage.Select(lang => lang.Name).Single(),
                     PullRequests = r.PullRequests(null, null, null, null, null, null, null, null, new List<PullRequestState>{PullRequestState.Open} ).TotalCount,
                     Issues = r.Issues(null, null, null, null, null, null, null, new List<IssueState>{IssueState.Open}).TotalCount,
                     Stars = r.Stargazers(null, null, null, null, null).TotalCount
                 }).Compile();

        public static async Task<List<Repo>> GetSamples(IConnection connection)
        {
            //run query 
            var result = await connection.Run(samplesQuery);
            return result.ToList();
        }
    }
}
