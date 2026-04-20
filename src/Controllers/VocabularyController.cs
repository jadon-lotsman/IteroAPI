using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Mnemo.Contracts.Dtos.Vocabulary;
using Mnemo.Services;
using Mnemo.Common;

namespace Mnemo.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class VocabularyController : ControllerBase
    {
        public VocabularyController(VocabularyManagementService service)
        {
            _vocabularyService = service;
        }

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        private readonly VocabularyManagementService _vocabularyService;


        [HttpGet]
        public async Task<IActionResult> GetAllEntries()
        {
            var entries = await _vocabularyService.GetAllEntriesAsync(UserId);

            var entriesDto = Mapper.MapToDto(entries);
            return Ok(entriesDto);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetEntryById(int id)
        {
            var entry = await _vocabularyService.GetEntryByIdAsync(UserId, id);

            if (entry == null)
                return NotFound();

            var entryDto = Mapper.MapToDto(entry);
            return Ok(entryDto);
        }

        [HttpGet("{key}")]
        public async Task<IActionResult> GetEntryByKey(string key)
        {
            var entry = await _vocabularyService.GetEntryByKeyAsync(UserId, key);

            if (entry == null)
                return NotFound();
            
            var entryDto = Mapper.MapToDto(entry);
            return Ok(entryDto);
        }


        [HttpPost]
        public async Task<IActionResult> CreateEntry(VocabularyEntryCreateDto dto)
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

        [HttpPut("{id:int}")]
        public async Task<IActionResult> PatchEntry(int id, VocabularyEntryPatchDto dto)
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
