using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Itero.API.Services;
using Itero.API.Dtos;

namespace Itero.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class VocabularyController : ControllerBase
    {
        public VocabularyController(VocabularyService service)
        {
            _vocabularyService = service;
        }

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        private readonly VocabularyService _vocabularyService;


        [HttpGet]
        public async Task<IActionResult> GetAllEntries()
        {
            var entries = await _vocabularyService.GetAllEntriesAsync(UserId);

            return Ok(entries);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetEntryById(int id)
        {
            var entry = await _vocabularyService.GetEntryByIdAsync(UserId, id);

            if (entry == null)
                return NotFound();

            return Ok(entry);
        }

        [HttpGet("{key}")]
        public async Task<IActionResult> GetEntryByKey(string key)
        {
            var entry = await _vocabularyService.GetEntryByKeyAsync(UserId, key);

            if (entry == null)
                return NotFound();

            return Ok(entry);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEntry(VocabularyEntryDTO dto)
        {
            var result = await _vocabularyService.CreateEntryAsync(UserId, dto);

            if (result == null)
                return BadRequest();

            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateEntry(int id, VocabularyPatchDTO dto)
        {
            var result = await _vocabularyService.PatchEntryAsync(UserId, id, dto);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteEntry(int id)
        {
            var success = await _vocabularyService.RemoveEntryById(UserId, id);

            if(!success)
                return NotFound();

            return NoContent();
        }
    }
}
