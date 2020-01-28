using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Octokit.GraphQL;
using SamplesDashboard.Models;
using SamplesDashboard.Services;
namespace SamplesDashboard.Controllers
{
    [ApiController]
    public class SamplesController : Controller
    {
        private readonly IConnection connection;
        public SamplesController(IConnection connection)
        {
            this.connection = connection;
        }

        [Produces("application/json")]
        [Route("api/[controller]")]
        [HttpGet]
        public async Task<IActionResult> GetSamplesListAsync()
        {
            List<Repo> samples = await SampleService.GetSamples(this.connection);
            return Ok(samples);
        }

        [Produces("application/json")]
        [Route("/feature/{id}")]
        public async Task<IActionResult> GetFeatureAreaAsync(string id)
        {
            List<string> LanguageList = await SampleService.GetFeatures(id);
            return Ok(LanguageList);
        }

        [Produces("application/json")]
        [Route("/language/{id}")]
        public async Task<IActionResult> GetLanguageAsync(string id)
        {
            var language = await SampleService.GetLanguages(id);
            return Ok(language);
        }
    }
}