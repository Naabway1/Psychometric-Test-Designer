using Psychometric_Test_Designer.Data;
using Psychometric_Test_Designer.DTOs;

namespace Psychometric_Test_Designer.Services
{
    public class TestProcessingService
    {
        public async Task<Dictionary<int, decimal>> CalculateScaleScores (SubmitAnswersDto dto)
        {
            var result = new Dictionary<int, decimal> ();

            foreach (var answer in dto.Answers)
            {
                // var answerEntity = await 
            }

            return result;
        }
    }
}
