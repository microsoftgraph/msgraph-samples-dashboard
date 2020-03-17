// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SamplesDashboard;
using SamplesDashboard.Services;

namespace SamplesDashboard.Controllers
{
    [ApiController]
    public class SdksController : Controller
    {
        private readonly SampleService _sampleService;
        private IMemoryCache _cache;
        private readonly IConfiguration _config;

        public SdksController(SampleService sampleService, IMemoryCache memoryCache, IConfiguration config)
        {
            _sampleService = sampleService;
            _cache = memoryCache;
            _config = config;
        }

        [Produces("application/json")]
        [Route("api/[controller]")]
        [HttpGet]
        public async Task<IActionResult> GetSdksListAsync()
        {
            string name = " sdk";
            List<Node> sdkList;

            if (!_cache.TryGetValue("sdks", out sdkList))
            {
                sdkList = await _sampleService.GetSamples(name);

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
                repository = await _sampleService.GetRepository(id);
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(_config.GetValue<double>("timeout")));
                _cache.Set(id, repository, cacheEntryOptions);
            }

            return Ok(repository);
        }
    }
}