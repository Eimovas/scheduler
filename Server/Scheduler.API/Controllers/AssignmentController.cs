using System;
using Microsoft.AspNetCore.Mvc;
using Scheduler.Core.API;
using Scheduler.Core.Domain.DTO;

namespace Scheduler.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssignmentController : ControllerBase
    {
        private readonly IScheduleCalculator _scheduleCalculator;

        public AssignmentController(IScheduleCalculator scheduleCalculator)
        {
            _scheduleCalculator = scheduleCalculator ?? throw new ArgumentNullException(nameof(scheduleCalculator));
        }

        [HttpGet]
        public IActionResult Ping()
        {
            return Ok($"I'm alive at: {DateTime.Now:F}");
        }

        [HttpPost]
        public IActionResult Calculate([FromBody] SetupDTO setup)
        {
            return Ok(_scheduleCalculator.Calculate(setup));
        }
    }
}
