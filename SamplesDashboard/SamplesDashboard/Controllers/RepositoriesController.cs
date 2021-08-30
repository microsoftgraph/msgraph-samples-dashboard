// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web.Resource;
using SamplesDashboard.Models;
using SamplesDashboard.Services;

namespace SamplesDashboard.Controllers
{
    [Authorize]
    [ApiController]
    [RequiredScope("Repos.Read")]
    [Route("[controller]")]
    public class RepositoriesController : ControllerBase
    {
        private readonly RepositoriesService _repositoriesService;
        private readonly ILogger<RepositoriesController> _logger;

        public RepositoriesController(
            RepositoriesService repositoriesService,
            ILogger<RepositoriesController> logger
        )
        {
            _repositoriesService = repositoriesService;
            _logger = logger;
        }

        [HttpGet("samples")]
        public async Task<ActionResult<IEnumerable<Repository>>> GetSamplesAsync()
        {
            var samples = await _repositoriesService.GetRepositoriesAsync(Constants.Samples);

            return Ok(samples);
        }

        [HttpGet("sdks")]
        public async Task<ActionResult<IEnumerable<Repository>>> GetSdks()
        {
            var sdks = await _repositoriesService.GetRepositoriesAsync(Constants.Sdks);

            return Ok(sdks);
        }

        [HttpGet("details/{repository}")]
        public async Task<ActionResult<Repository>> GetRepositoryDetails(string repository)
        {
            var repo = await _repositoriesService.GetRepositoryAsync(repository);

            return Ok(repo);
        }
    }
}
