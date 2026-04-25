using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Mnemo.Common;
using Mnemo.Contracts.Dtos.Repetition.Requests;
using Mnemo.Services;
using Mnemo.Services.Queries;

namespace Mnemo.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class StateController : ControllerBase
    {
        private readonly StateQueries _stateQueries;
        private readonly RepetitionStateService _stateService;

        public StateController(StateQueries stateQueries, RepetitionStateService stateService)
        {
            _stateQueries = stateQueries;
            _stateService = stateService;
        }

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));


        
        [HttpGet]
        public async Task<IActionResult> GetAllRepetitionStates()
        {
            var states = await _stateQueries.GetAllByUserIdAsync(UserId);

            var statesDto = Mapper.MapToDto(states);
            return Ok(statesDto);
        }

        
        [HttpPost("{id:int}/assess")]
        public async Task<IActionResult> SelfAssessmentRepetitionState(int id, [FromBody] QualityAssessmentRequest request)
        {
            var result = await _stateService.UpdateRepetitionStateAsync(UserId, id, request.Quality, shouldIncrementCounter: false);

            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    ErrorCode.TaskNotFound => NotFound(result.ErrorMessage),
                    ErrorCode.ActionNotAllowed => BadRequest(result.ErrorMessage),
                    _ => StatusCode(500, result.ErrorMessage)
                };
            }

            var stateDto = Mapper.MapToDto(result.Value);
            return Ok(stateDto);
        }
    }
}
