using System.ComponentModel.DataAnnotations;

namespace Mnemo.Contracts.Dtos.Repetition.Requests
{
    public class SubmitTaskAnswerRequest
    {
        public string? Answer { get; set; }
    }
}
