namespace Psychometric_Test_Designer.DTOs
{
    public class SubmitTestDto
    {
        public int UserId { get; set; }
        public int TestId { get; set; }

        public List<SubmitAnswerDto> Answers { get; set; } 
    }
}
