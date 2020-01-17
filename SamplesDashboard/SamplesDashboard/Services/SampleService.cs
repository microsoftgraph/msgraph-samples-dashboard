using System;
//using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Octokit.GraphQL;
using System.Collections.Generic;
using SamplesDashboard.Models;

namespace SamplesDashboard.Services
{
    public class SampleServices
    {
        public static async Task<List<Repo>> GetSamples()
        {
            var productInformation = new ProductHeaderValue("Octokit.GraphQL", "0.1.4-beta");
            var connection = new Connection(productInformation, "305098146e3c47ef05c78244e3c1953516bd26d9");

            var query = new Query()
                .RepositoryOwner("microsoftgraph")
                .Repositories(first: 50)
                .Nodes
                .Select(r => new Repo {
                    Name = r.Name,
                    Owner = r.Owner,


                });
             
            //run query 
            var result = await connection.Run(query);

            return result.ToList();
        }
    }
}
