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
    public class RepositoriesController : Controller
    {
        private readonly RepositoriesService _repositoriesService;
        private IMemoryCache _cache;
        private readonly IConfiguration _config;

        public RepositoriesController(RepositoriesService repositoriesService, IMemoryCache memoryCache, IConfiguration config)
        {
            _repositoriesService = repositoriesService;
            _cache = memoryCache;
            _config = config;
        }

        [Produces("application/json")]
        [Route("api/samples")]
        [HttpGet]
        public async Task<IActionResult> GetSamplesListAsync()
        {
            string name = " sample OR training";
            List<Node> samples;

            if (!_cache.TryGetValue("samples", out samples))
            {
                samples = await _repositoriesService.GetRepositories(name);

                //Read timeout from config file 
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(_config.GetValue<double>("timeout")));

                // Save data in cache.
                _cache.Set("samples", samples, cacheEntryOptions);
            }              

            return Ok(samples);
        }

        [Produces("application/json")]
        [Route("api/sdks")]
        [HttpGet]
        public async Task<IActionResult> GetSdksListAsync()
        {
            string name = " sdk";
            List<Node> sdkList;

            if (!_cache.TryGetValue("sdks", out sdkList))
            {
                sdkList = await _repositoriesService.GetRepositories(name);

                //Read timeout from config file 
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(_config.GetValue<double>("timeout")));

                // Save data in cache.
                _cache.Set("sdks", sdkList, cacheEntryOptions);
            }

            return Ok(sdkList);
        }

        [Produces("application/json")]
        [Route("api/[controller]/{id}")]
        public async Task<IActionResult> GetRepositoriesAsync(string id)
        {
            Repository repository;
            if (!_cache.TryGetValue(id, out repository))
            {
                repository = await _repositoriesService.GetRepository(id);
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(_config.GetValue<double>("timeout")));
                _cache.Set(id, repository, cacheEntryOptions);
            }
            
            return Ok(repository);
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