namespace Mnemo.Contracts.Dtos.Memorization
{
    public class RepetitionTaskResponseDto
    {
        public int Id { get; set; }
        public string? Prompt { get; set; }
        public string? UserAnswer { get; set; }
    }
}
