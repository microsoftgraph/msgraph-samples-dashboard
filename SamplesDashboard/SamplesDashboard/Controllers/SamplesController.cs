using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Octokit.GraphQL;
using SamplesDashboard.Services;
namespace SamplesDashboard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SamplesController : Controller
    {
        private readonly IConnection connection;
        public SamplesController(IConnection connection)
        {
            this.connection = connection;
        }

        [Produces("application/json")]
        [HttpGet]
        public async Task<IActionResult> GetSamplesListAsync()
        {         
            var samples = await SampleService.GetSamples(this.connection);             
            return Ok(samples);
        }
    }
}