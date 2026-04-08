using System.Security.Claims;
using Itero.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Itero.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class IterationController : ControllerBase
    {
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        private readonly IterationService _iterationService;

        public IterationController(IterationService service)
        {
            _iterationService = service;
        }


        [HttpGet]
        public async Task<IActionResult> GetIteration()
        {
            var iteration = await _iterationService.GetIterationAsync(UserId);

            if (iteration == null) 
                return NotFound();

            return Ok(iteration);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetIterationStepById(int stepId)
        {
            var step = await _iterationService.GetIterationStepByIdASync(UserId, stepId);

            if (step == null)
                return NotFound();

            return Ok(step);
        }

        [HttpPost]
        public async Task<IActionResult> CreateIteration()
        {
            var result = await _iterationService.CreateIterationAsync(UserId);

            if (result == null)
                return BadRequest("Already exist");

            return Ok(result);
        }

        [HttpPost("setStep")]
        public async Task<IActionResult> SetIterationStepValue(int stepId, string userValue)
        {
            var success = await _iterationService.SetStepValueAsync(UserId, stepId, userValue);

            if (!success)
                return NotFound();

            return Ok();
        }

        [HttpPost("result")]
        public async Task<IActionResult> ResultIteration()
        {
            

            return Ok();
        }
    }
}
