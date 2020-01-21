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
        public IConfiguration Configuration { get; }

        public SamplesController(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        [Produces("application/json")]
        [HttpGet]
        public async Task<IActionResult> GetSamplesListAsync()
        {

            var productInformation = new ProductHeaderValue("Octokit.GraphQL", "0.1.4-beta");
            
            var token = Configuration.GetValue<string>("auth_token");
            var connection = new Connection(productInformation, token);

            var samples = await SampleService.GetSamples(connection); 
            
            return Ok(samples);
        }
    }
}