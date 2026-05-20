namespace Psychometric_Test_Designer.DTOs
{
    public class SubmitCurrentUserTestDto
    {
        public int TestId { get; set; }
        public List<SubmitAnswerDto> Answers { get; set; } = new();
    }
}
