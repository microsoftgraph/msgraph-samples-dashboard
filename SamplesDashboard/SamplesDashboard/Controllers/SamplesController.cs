// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using GraphQL.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SamplesDashboard.Models;
using SamplesDashboard.Services;
namespace SamplesDashboard.Controllers
{
    [ApiController]
    public class SamplesController : Controller
    {
        private readonly GraphQLClient client;

        public SamplesController(GraphQLClient client)
        {
            this.client = client;
        }

        [Produces("application/json")]
        [Route("api/[controller]")]
        [HttpGet]
        public async Task<IActionResult> GetSamplesListAsync()
        {
            List<Repo> samples = await SampleService.GetSamples(this.client);
            return Ok(samples);
        }

        [Produces("application/json")]
        [Route("api/[controller]/{id}")]
        public async Task<IActionResult> GetDependenciesAsync(string id)
        {
            var dependencies = await SampleService.GetDependencies(this.client, id);
            return Ok(dependencies);
        }

        [Produces("application/json")]
        [Route("/feature/{id}")]
        public async Task<IActionResult> GetFeatureAreaAsync(string id)
        {
            List<string> ServicesList = await SampleService.GetFeatures(id);
            return Ok(ServicesList);
        }

        [Produces("application/json")]
        [Route("/language/{id}")]
        public async Task<IActionResult> GetLanguageAsync(string id)
        {
            List<string> languages = await SampleService.GetLanguages(id);
            return Ok(languages);
        }
    }
}