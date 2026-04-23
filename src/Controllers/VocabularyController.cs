using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Mnemo.Services;
using Mnemo.Common;
using Mnemo.Services.Queries;
using Mnemo.Contracts.Dtos.Vocabulary.Requests;

namespace Mnemo.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]/entries")]
    public class VocabularyController : ControllerBase
    {
        private readonly VocabularyQueries _vocabularyQueries;
        private readonly VocabularyManagementService _vocabularyService;


        public VocabularyController(VocabularyQueries vocabularyQueries, VocabularyManagementService vocabularyService)
        {
            _vocabularyQueries = vocabularyQueries;
            _vocabularyService = vocabularyService;
        }

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));



        [HttpGet]
        public async Task<IActionResult> GetAllEntries()
        {
            var entries = await _vocabularyQueries.GetAllByUserIdAsync(UserId);

            var entriesDto = Mapper.MapToDto(entries);
            return Ok(entriesDto);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetEntryById(int id)
        {
            var entry = await _vocabularyQueries.GetByIdAsync(UserId, id);

            if (entry == null)
                return NotFound();

            var entryDto = Mapper.MapToDto(entry);
            return Ok(entryDto);
        }

        [HttpGet("find")]
        public async Task<IActionResult> GetEntryByKey([FromQuery] string foreign)
        {
            var entry = await _vocabularyQueries.GetByForeignAsync(UserId, foreign);

            if (entry == null)
                return NotFound();
            
            var entryDto = Mapper.MapToDto(entry);
            return Ok(entryDto);
        }


        [HttpPost]
        public async Task<IActionResult> CreateEntry([FromBody] CreateVocabularyEntryRequest dto)
        {
            var result = await _vocabularyService.CreateEntryAsync(UserId, dto);

            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    "INVALID_DATA" => BadRequest(result.ErrorCode),
                    "USER_NOT_FOUND" => NotFound(result.ErrorCode),
                    "DUPLICATE_ENTRY" => Conflict(result.ErrorCode),
                    _ => StatusCode(500, result.ErrorCode)
                };
            }

            var entryDto = Mapper.MapToDto(result.Value);
            return Ok(entryDto);
        }

        [HttpPatch("{id:int}")]
        public async Task<IActionResult> PatchEntry(int id, [FromBody] PatchVocabularyEntryRequest dto)
        {
            var result = await _vocabularyService.PatchEntryAsync(UserId, id, dto);

            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    "INVALID_DATA" => BadRequest(result.ErrorCode),
                    "ENTRY_NOT_FOUND" => NotFound(result.ErrorCode),
                    _ => StatusCode(500, result.ErrorCode)
                };
            }

            var entryDto = Mapper.MapToDto(result.Value);
            return Ok(entryDto);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteEntry(int id)
        {
            var result = await _vocabularyService.RemoveEntryByIdAsync(UserId, id);

            if(!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    "ENTRY_NOT_FOUND" => NotFound(result.ErrorCode),
                    _ => StatusCode(500, result.ErrorCode)
                };
            }

            return NoContent();
        }
    }
}
