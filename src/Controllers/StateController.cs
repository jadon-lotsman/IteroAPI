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
    public class StateController : ControllerBase
    {
        public StateController(RepetitionStateService stateService)
        {
            _stateService = stateService;
        }

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        private readonly RepetitionStateService _stateService;


        
        [HttpPut("{id:int}")]
        public async Task<IActionResult> SelfAssessmentRepetitionState(int id, double quality)
        {
            var result = await _stateService.UpdateRepetitionStateAsync(UserId, id, quality, shouldIncrementCounter: false);

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
