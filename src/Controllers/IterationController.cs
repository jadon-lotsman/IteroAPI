using System.Security.Claims;
using Itereta.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Mnemo.Common;
using Mnemo.Services;

namespace Mnemo.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class IterationController : ControllerBase
    {
        public IterationController(VocabularyIterationService service)
        {
            _iterationService = service;
        }

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        private readonly VocabularyIterationService _iterationService;


        [HttpGet("status")]
        public async Task<IActionResult> GetIterationStatus()
        {
            var result = await _iterationService.GetIterationStatusAsync(UserId);

            return StatusCode(500, result.ErrorCode);
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartIteration()
        {
            var result = await _iterationService.StartIterationAsync(UserId);

            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    "USER_NOT_FOUND" => NotFound(result.ErrorCode),
                    "ITERATION_NOT_FINISHED" => Conflict(result.ErrorCode),
                    _ => StatusCode(500, result.ErrorCode)
                };
            }

            return Ok();
        }

        [HttpPost("finish")]
        public async Task<IActionResult> FinishIteration()
        {
            var result = await _iterationService.FinishIterationAsync(UserId);

            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    "ITERATION_NOT_FOUND" => NotFound(result.ErrorCode),
                    "ITERATION_HAS_NO_ITERETTES" => BadRequest(result.ErrorCode),
                    _ => StatusCode(500, result.ErrorCode)
                };
            }

            return Ok(result.Value);
        }


        [HttpGet("iterettes")]
        public async Task<IActionResult> GetAllIterettes()
        {
            var iterettes = await _iterationService.GetAllIterettesAsync(UserId);

            var iterettesDto = Mapper.MapToDto(iterettes);
            return Ok(iterettesDto);
        }

        [HttpGet("iterettes/{id:int}")]
        public async Task<IActionResult> GetIteretteById(int id)
        {
            var iterette = await _iterationService.GetIteretteByIdAsync(UserId, id);

            if (iterette == null)
                return NotFound();

            var iterettesDto = Mapper.MapToDto(iterette);
            return Ok(iterettesDto);
        }

        [HttpPut("iterettes/answer/{id:int}")]
        public async Task<IActionResult> SubmitIteretteAnswer(int id, string answer)
        {
            var result = await _iterationService.SubmitIteretteAnswerAsync(UserId, id, answer);

            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    "ITERETTE_NOT_FOUND" => NotFound(result.ErrorCode),
                    "ITERATION_WAS_FINISHED" => BadRequest(result.ErrorCode),
                    _ => StatusCode(500, result.ErrorCode)
                };
            }

            var iteretteDto = Mapper.MapToDto(result.Value);
            return Ok(iteretteDto);
        }


        [HttpPut("repetitions/{id:int}")]
        public async Task<IActionResult> SelfAssessmentRepetitionState(int id, double quality)
        {
            var result = await _iterationService.SelfAssessmentAsync(UserId, id, quality);

            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    "REPETITION_STATE_NOT_FOUND" => NotFound(result.ErrorCode),
                    "REPETITION_STATE_ASSESS_NOT_ALLOWED" => BadRequest(result.ErrorCode),
                    _ => StatusCode(500, result.ErrorCode)
                };
            }

            var stateDto = Mapper.MapToDto(result.Value);
            return Ok(stateDto);
        }
    }
}
