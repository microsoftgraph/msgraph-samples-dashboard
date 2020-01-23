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
        private static readonly ICompiledQuery<IEnumerable<Repo>> samplesQuery
            = new Query().Search("org:microsoftgraph training OR sample in:name", SearchType.Repository, 100)
            .Nodes
            .Select(node => node.Switch<Repo>(
               when => when.Repository(
                   repo => new Repo()
                   {
                       Name = repo.Name,
                       Owner = repo.Owner.Login,
                       Language = repo.PrimaryLanguage.Select(lang => lang.Name).Single(),
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
    }
}
