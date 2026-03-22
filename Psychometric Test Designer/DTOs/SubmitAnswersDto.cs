namespace Psychometric_Test_Designer.DTOs
{
    public class SubmitAnswersDto
    {
        public int UserId { get; set; }
        public int TestId { get; set; }

        public List<AnswerDto> Answers { get; set; }
    }
}
