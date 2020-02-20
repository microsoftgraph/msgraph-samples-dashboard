// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Client.Http;
using Microsoft.AspNetCore.Mvc;
using SamplesDashboard.Services;
namespace SamplesDashboard.Controllers
{
    [ApiController]
    public class SamplesController : Controller
    {
        private readonly SampleService _sampleService;

        public SamplesController(SampleService sampleService)
        {
            _sampleService = sampleService;
        }

        [Produces("application/json")]
        [Route("api/[controller]")]
        [HttpGet]
        public async Task<IActionResult> GetSamplesListAsync()
        {
            var samples = await _sampleService.GetSamples();
            return Ok(samples);
        }

        [Produces("application/json")]
        [Route("api/[controller]/{id}")]
        public async Task<IActionResult> GetDependenciesAsync(string id)
        {
            var dependencies = await _sampleService.GetDependencies(id);
            return Ok(dependencies);
        }

        [Produces("application/json")]
        [Route("/feature/{id}")]
        public async Task<IActionResult> GetFeatureAreaAsync(string id)
        {
            List<string> ServicesList = await _sampleService.GetFeatures(id);
            return Ok(ServicesList);
        }

        [Produces("application/json")]
        [Route("/language/{id}")]
        public async Task<IActionResult> GetLanguageAsync(string id)
        {
            List<string> languages = await _sampleService.GetLanguages(id);
            return Ok(languages);
        }
        //Gert all Samples and their dependencies in batch
        //[Produces("application/json")]
        //[Route("api/[controller]/{count}")]
        //public async Task<IActionResult> Get(int count)
        //{
        //    var samples = await _sampleService.GetSamples(count);
        //    var data = new List<Dto>();
        //    foreach (var sample in samples)
        //    {
        //        var dependencies = await _sampleService.GetDependencies(sample.Name);
        //        var languages = await _sampleService.GetLanguages(sample.Name);
        //        var featureArea = await _sampleService.GetFeatures(sample.Name);
        //        data.Add(new Dto()
        //        {
        //            Sample = sample,
        //            Dependencies = dependencies.ToList(),
        //            Languages = languages,
        //            FeatureArea = featureArea
        //        });
        //    }
        //    return Ok(data);
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