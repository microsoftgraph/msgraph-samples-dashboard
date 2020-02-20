// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using GraphQL.Client.Http;
using Microsoft.AspNetCore.Mvc;
using SamplesDashboard.Services;
using System.Linq;

namespace SamplesDashboard.Controllers
{
    [ApiController]
    public class SamplesController : Controller
    {
        private readonly SampleService _service;

        public SamplesController(SampleService service)
        {
            _service = service;
        }

        //[Produces("application/json")]

        //[HttpGet]
        //public async Task<IActionResult> GetSamplesListAsync()
        //{
        //    var samples = await _service.GetSamples();
        //    return Ok(samples);
        //}

        [Produces("application/json")]
        [Route("api/[controller]/{id}")]
        public async Task<IActionResult> GetDependenciesAsync(string id)
        {
            var dependencies = await _service.GetDependencies(id);
            return Ok(dependencies);
        }

        [Produces("application/json")]
        [Route("/feature/{id}")]
        public async Task<IActionResult> GetFeatureAreaAsync(string id)
        {
            List<string> ServicesList = await _service.GetFeatures(id);
            return Ok(ServicesList);
        }

        [Produces("application/json")]
        [Route("/language/{id}")]
        public async Task<IActionResult> GetLanguageAsync(string id)
        {
            List<string> languages = await _service.GetLanguages(id);
            return Ok(languages);
        }

        [Route("api/[controller]")]
        public async Task<IActionResult> Get()
        {
            var samples = await _service.GetSamples();
            var data = new List<Dto>();
            foreach (var sample in samples)
            {
                var dependencies = await _service.GetDependencies(sample.Id);
                var languages = await _service.GetLanguages(sample.Id);
                var featureArea = await _service.GetFeatures(sample.Id);
                data.Add(new Dto()
                {
                    Sample = sample,
                    Dependencies = dependencies.ToList(),
                    Languages = languages,
                    FeatureArea = featureArea
                });
            }
            return Ok(data);

        }
        public class Dto
        {
            public Dto()
            {
                this.Dependencies = new List<DependenciesNode>();
                Languages = new List<string>();
                FeatureArea = new List<string>();
            }
            public Node Sample { get; set; }
            public List<DependenciesNode> Dependencies { get; set; }
            public List<string> Languages { get; set; }
            public List<string> FeatureArea { get; set; }
        }
    }
}