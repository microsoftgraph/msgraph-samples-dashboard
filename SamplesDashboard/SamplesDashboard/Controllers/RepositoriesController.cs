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
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace SamplesDashboard.Controllers
{
    [ApiController]
    [Authorize]
    public class RepositoriesController : Controller
    {
        private readonly RepositoriesService _repositoriesService;
        private IMemoryCache _cache;
        private readonly IConfiguration _config;
        private readonly ILogger<RepositoriesController> _logger;

        public RepositoriesController(RepositoriesService repositoriesService, IMemoryCache memoryCache, IConfiguration config, ILogger<RepositoriesController> logger)
        {
            _repositoriesService = repositoriesService;
            _cache = memoryCache;
            _config = config;
            _logger = logger;
        }

        [Produces("application/json")]
        [Route("api/samples")]
        [HttpGet]
        public async Task<IActionResult> GetSamplesListAsync()        
        {

            if (!_cache.TryGetValue(Constants.Samples, out var samples))
            {   

                samples = await _repositoriesService.GetRepositories(Constants.Samples);

                //Read timeout from config file 
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(_config.GetValue<double>(Constants.Timeout)));

                // Save data in cache.
                _cache.Set( Constants.Samples, samples, cacheEntryOptions);
                _logger.LogInformation($"{nameof(RepositoriesController)} :samples cache refreshed");
            }

            return Ok(samples);
        }

        [Produces("application/json")]
        [Route("api/sdks")]
        [HttpGet]
        public async Task<IActionResult> GetSdksListAsync()
        {     

            if (!_cache.TryGetValue(Constants.Sdks, out var sdkList))
            {
                sdkList = await _repositoriesService.GetRepositories(Constants.Sdks);

                //Read timeout from config file 
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(_config.GetValue<double>(Constants.Timeout)));

                // Save data in cache.
                _cache.Set(Constants.Sdks, sdkList, cacheEntryOptions);
                _logger.LogInformation($"{nameof(RepositoriesController)} :sdks cache refreshed");
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
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(_config.GetValue<double>(Constants.Timeout)));
                _cache.Set(id, repository, cacheEntryOptions);
                _logger.LogInformation($"{nameof(RepositoriesController)} : {id} : repositories cache refreshed");

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