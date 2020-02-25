// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SamplesDashboard.Services;

namespace SamplesDashboard.Controllers
{
    [ApiController]
    public class SamplesController : Controller
    {
        private readonly SampleService _sampleService;
        private IMemoryCache _cache;
        private readonly IConfiguration _config;

        public SamplesController(SampleService sampleService, IMemoryCache memoryCache, IConfiguration config)
        {
            _sampleService = sampleService;
            _cache = memoryCache;
            _config = config;

        }

        [Produces("application/json")]
        [Route("api/[controller]")]
        [HttpGet]
        public async Task<IActionResult> GetSamplesListAsync()
        {
            List<Node> samples;            
            if (!_cache.TryGetValue("samples", out samples))
            {
                samples = await _sampleService.GetSamples();

                //Read timeout from config file 
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(_config.GetValue<double>("timeout")));

                // Save data in cache.
                _cache.Set("samples", samples, cacheEntryOptions);
            }
                
            return Ok(samples);
        }

        [Produces("application/json")]
        [Route("api/[controller]/{id}")]
        public async Task<IActionResult> GetDependenciesAsync(string id)
        {
            IEnumerable<DependenciesNode> dependencies;
            if (!_cache.TryGetValue(id, out dependencies))
            {
                dependencies = await _sampleService.GetDependencies(id);
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(_config.GetValue<double>("timeout")));
                _cache.Set(id, dependencies, cacheEntryOptions);
            }
            
            return Ok(dependencies);
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