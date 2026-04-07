using Psychometric_Test_Designer.Data;
using Psychometric_Test_Designer.DTOs;
using Psychometric_Test_Designer.Models;
using Microsoft.EntityFrameworkCore;

namespace Psychometric_Test_Designer.Services
{
    public class TestProcessingService
    {
        private readonly AppDbContext _db;

        public TestProcessingService(AppDbContext db)
        {
            _db = db;
        }

        public async Task ProcessTest(SubmitTestDto dto)
        {
            Validate(dto);

            var answers = await GetAnswersData(dto);
            var scaleScores = CalculateScaleScores(answers);
            var normalized = Normalize(scaleScores);
            var metrics = await CalculateMetrics(dto.TestId, normalized);

            await ApplyEma(dto.UserId, metrics);
            SaveScaleResults(dto, scaleScores, normalized);

            await _db.SaveChangesAsync();
        }

        private void Validate(SubmitTestDto dto)
        {
            if (dto.Answers == null || !dto.Answers.Any())
                throw new Exception("Нет ответов");
        }

        private async Task<List<AnswerData>> GetAnswersData(SubmitTestDto dto)
        {
            var answerIds = dto.Answers.Select(a => a.AnswerId).ToList();

            return await _db.AnswerOptions
                .Where(a => answerIds.Contains(a.AnswerId))
                .Select(a => new AnswerData
                {
                    AnswerId = a.AnswerId,
                    Value = a.Value,
                    QuestionId = a.QuestionId,

                    Scales = _db.QuestionScales
                        .Where(qs => qs.QuestionId == a.QuestionId)
                        .Select(qs => new ScaleLink
                        {
                            ScaleId = qs.ScaleId,
                            Weight = (decimal)qs.Weight
                        })
                        .ToList()
                })
                .ToListAsync();
        }

        private Dictionary<int, decimal> CalculateScaleScores(List<AnswerData> answers)
        {
            var result = new Dictionary<int, decimal>();

            foreach (var answer in answers)
            {
                foreach (var scale in answer.Scales)
                {
                    if (!result.ContainsKey(scale.ScaleId))
                        result[scale.ScaleId] = 0;

                    result[scale.ScaleId] += answer.Value * scale.Weight;
                }
            }

            return result;
        }

        private Dictionary<int, decimal> Normalize(Dictionary<int, decimal> raw)
        {
            return raw.ToDictionary(
                x => x.Key,
                x => x.Value / 1m //временно
            );
        }

        private async Task<Dictionary<int, decimal>> CalculateMetrics(
            int testId,
            Dictionary<int, decimal> normalized)
        {
            var scaleIds = normalized.Keys.ToList();

            var links = await _db.TestScaleMetrics
                .Where(x => x.TestId == testId && scaleIds.Contains(x.ScaleId))
                .ToListAsync();

            var result = new Dictionary<int, decimal>();

            foreach (var link in links)
            {
                var value = normalized[link.ScaleId];

                if (!result.ContainsKey(link.MetricId))
                    result[link.MetricId] = 0;

                result[link.MetricId] += value * (decimal)link.Weight;
            }

            foreach (var key in result.Keys.ToList())
                result[key] *= 100;

            return result;
        }

        private async Task ApplyEma(int userId, Dictionary<int, decimal> metrics)
        {
            const decimal alpha = 0.7m;

            var existingMetrics = await _db.UserMetrics
                .Where(x => x.UserId == userId)
                .ToListAsync();

            foreach (var metric in metrics)
            {
                var existing = existingMetrics
                    .FirstOrDefault(x => x.MetricId == metric.Key);

                if (existing == null)
                {
                    _db.UserMetrics.Add(new UserMetric
                    {
                        UserId = userId,
                        MetricId = metric.Key,
                        Value = metric.Value
                    });
                }
                else
                {
                    existing.Value =
                        existing.Value * alpha +
                        metric.Value * (1 - alpha);
                }
            }
        }

        private void SaveScaleResults(SubmitTestDto dto, Dictionary<int, decimal> raw, Dictionary<int, decimal> normalized)
        {
            foreach (var scale in raw)
            {
                _db.UserScaleResults.Add(new UserScaleResult
                {
                    UserId = dto.UserId,
                    ScaleId = scale.Key,
                    RawScore = scale.Value,
                    NormalizedScore = normalized[scale.Key],
                    SourceTestId = dto.TestId,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        public class AnswerData
        {
            public int AnswerId { get; set; }
            public decimal Value { get; set; }
            public int QuestionId { get; set; }
            public List<ScaleLink> Scales { get; set; }
        }

        public class ScaleLink
        {
            public int ScaleId { get; set; }
            public decimal Weight { get; set; }
        }
    }
}
