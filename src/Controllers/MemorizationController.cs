using System.Security.Claims;
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
    public class MemorizationController : ControllerBase
    {
        public MemorizationController(VocabularyMemorizationService service)
        {
            _memorizationService = service;
        }

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        private readonly VocabularyMemorizationService _memorizationService;


        [HttpGet("status")]
        public async Task<IActionResult> GetRepetitionSessionStatus()
        {
            var result = await _memorizationService.GetRepetitionSessionStatusAsync(UserId);

            return StatusCode(500, result.ErrorCode);
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartRepetitionSession()
        {
            var result = await _memorizationService.StartRepetitionSessionAsync(UserId);

            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    "USER_NOT_FOUND" => NotFound(result.ErrorCode),
                    "SESSION_NOT_FINISHED" => Conflict(result.ErrorCode),
                    _ => StatusCode(500, result.ErrorCode)
                };
            }

            return Ok();
        }

        [HttpPost("finish")]
        public async Task<IActionResult> FinishRepetitionSession()
        {
            var result = await _memorizationService.FinishRepetitionSessionAsync(UserId);

            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    "SESSION_NOT_FOUND" => NotFound(result.ErrorCode),
                    "SESSION_HAS_NO_TASKS" => BadRequest(result.ErrorCode),
                    _ => StatusCode(500, result.ErrorCode)
                };
            }

            return Ok(result.Value);
        }


        [HttpGet("tasks")]
        public async Task<IActionResult> GetAllTasks()
        {
            var tasks = await _memorizationService.GetAllRepetitionTasksAsync(UserId);

            var tasksDto = Mapper.MapToDto(tasks);
            return Ok(tasksDto);
        }

        [HttpGet("tasks/{id:int}")]
        public async Task<IActionResult> GetTaskById(int id)
        {
            var task = await _memorizationService.GetRepetitionTaskByIdAsync(UserId, id);

            if (task == null)
                return NotFound();

            var tasksDto = Mapper.MapToDto(task);
            return Ok(tasksDto);
        }

        [HttpPut("tasks/answer/{id:int}")]
        public async Task<IActionResult> SubmitTaskAnswer(int id, string answer)
        {
            var result = await _memorizationService.SubmitRepetitionTaskAnswerAsync(UserId, id, answer);

            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    "TASK_NOT_FOUND" => NotFound(result.ErrorCode),
                    "SESSION_WAS_FINISHED" => BadRequest(result.ErrorCode),
                    _ => StatusCode(500, result.ErrorCode)
                };
            }

            var tasksDto = Mapper.MapToDto(result.Value);
            return Ok(tasksDto);
        }


        [HttpPut("repetitions/{id:int}")]
        public async Task<IActionResult> SelfAssessmentRepetitionState(int id, double quality)
        {
            var result = await _memorizationService.SelfAssessmentRepetitionStateAsync(UserId, id, quality);

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
