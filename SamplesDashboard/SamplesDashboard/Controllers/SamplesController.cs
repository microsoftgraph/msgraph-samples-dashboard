using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SamplesDashboard.Services;
namespace SamplesDashboard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SamplesController : Controller
    {
        //public SamplesController(IConfiguration config)
        //{
        //    this.config = config;
        //}
        [Produces("application/json")]
        [HttpGet]
        public async Task<IActionResult> GetSamplesListAsync()
        {
            var samples = await SampleServices.GetSamples(); 
            
            return Ok(samples);
        }
    }
}