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

            return StatusCode(500, result.ErrorMessage);
        }

        [HttpPost]
        public async Task<IActionResult> StartRepetitionSession()
        {
            var result = await _sessionService.StartRepetitionSessionAsync(UserId);

            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    ErrorCode.UserNotFound => NotFound(result.ErrorMessage),
                    ErrorCode.SessionNotFinished => BadRequest(result.ErrorMessage),
                    _ => StatusCode(500, result.ErrorMessage)
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
                    ErrorCode.SessionNotFound => NotFound(result.ErrorMessage),
                    _ => StatusCode(500, result.ErrorMessage)
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
                    ErrorCode.TaskNotFound => NotFound(result.ErrorMessage),
                    _ => StatusCode(500, result.ErrorMessage)
                };
            }

            var tasksDto = Mapper.MapToDto(result.Value);
            return Ok(tasksDto);
        }
    }
}
