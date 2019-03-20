using System;
using Microsoft.AspNetCore.Mvc;
using Scheduler.Console.API;
using Scheduler.Core.Domain.DTO;

namespace Scheduler.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DistributionController : ControllerBase
    {
        [HttpGet]
        public IActionResult Ping()
        {
            return Ok($"I'm alive at: {DateTime.Now:F}");
        }

        [HttpPost]
        public IActionResult Calculate([FromBody] SetupDTO setup)
        {
            return Ok(ScheduleProvider.CalculateDistribution(setup));
        }
    }

    public sealed class MyClass
    {
        public static void Some()
        {
            this.Equals("");
        }
    }
}
