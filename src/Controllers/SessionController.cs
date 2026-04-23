using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Mnemo.Common;
using Mnemo.Contracts.Dtos.Repetition;
using Mnemo.Contracts.Dtos.Repetition.Requests;
using Mnemo.Services;
using Mnemo.Services.Queries;

namespace Mnemo.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class SessionController : ControllerBase
    {
        private readonly SessionQueries _sessionQueries;
        private readonly RepetitionSessionService _sessionService;

        public SessionController(SessionQueries sessionQueries, RepetitionSessionService sessionService)
        {
            _sessionQueries = sessionQueries;
            _sessionService = sessionService;
        }

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));



        [HttpGet]
        public async Task<IActionResult> GetRepetitionSessionStatus()
        {
            var result = await _sessionService.GetRepetitionSessionStatusAsync(UserId);

            return StatusCode(500, result.ErrorCode);
        }

        [HttpPost]
        public async Task<IActionResult> StartRepetitionSession()
        {
            var result = await _sessionService.StartRepetitionSessionAsync(UserId);

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

        [HttpDelete]
        public async Task<IActionResult> FinishRepetitionSession()
        {
            var result = await _sessionService.FinishRepetitionSessionAsync(UserId);

            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    "SESSION_NOT_FOUND" => NotFound(result.ErrorCode),
                    "SESSION_HAS_NO_TASKS" => BadRequest(result.ErrorCode),
                    _ => StatusCode(500, result.ErrorCode)
                };
            }

            var resultDto = Mapper.MapToDto(result.Value);
            return Ok(resultDto);
        }



        [HttpGet("tasks")]
        public async Task<IActionResult> GetAllTasks()
        {
            var tasks = await _sessionQueries.GetTasksByUserIdAsync(UserId);

            var tasksDto = Mapper.MapToDto(tasks);
            return Ok(tasksDto);
        }

        [HttpGet("tasks/{id:int}")]
        public async Task<IActionResult> GetTaskById(int id)
        {
            var task = await _sessionQueries.GetTaskByIdAsync(UserId, id);

            if (task == null)
                return NotFound();

            var tasksDto = Mapper.MapToDto(task);
            return Ok(tasksDto);
        }

        [HttpPost("tasks/{id:int}/answer")]
        public async Task<IActionResult> SubmitTaskAnswer(int id, [FromBody] SubmitTaskAnswerRequest request)
        {
            var result = await _sessionService.SubmitRepetitionTaskAnswerAsync(UserId, id, request.Answer);

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
    }
}
