﻿// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
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
        [Route("/features/{id}")]
        public async Task<IActionResult> GetLanguagesAndFeatureAreaAsync(string id)
        {
            Dictionary<string,string> ServicesList = await _sampleService.GetHeaderDetails(id);
            return Ok(ServicesList);
        }      
    }

    public class Dto
    {
        public Dto()
        {
            this.Dependencies = new List<DependenciesNode>();
            Features = new List<string>();
           
        }
        public Node Sample { get; set; }
        public List<DependenciesNode> Dependencies { get; set; }
        public List<string> Features { get; set; }
    }
}