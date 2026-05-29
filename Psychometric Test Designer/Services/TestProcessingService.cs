using Microsoft.EntityFrameworkCore;
using Psychometric_Test_Designer.Data;
using Psychometric_Test_Designer.DTOs;
using Psychometric_Test_Designer.Models;

namespace Psychometric_Test_Designer.Services
{
    public class TestProcessingService
    {
        private readonly AppDbContext _db;

        public TestProcessingService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<ProcessTestResultDto> ProcessTest(SubmitTestDto dto)
        {
            await Validate(dto);

            var answers = await GetAnswersData(dto);
            var scaleScores = CalculateScaleScores(answers);
            var ranges = CalculateScaleRanges(answers);
            var normalized = Normalize(scaleScores, ranges);
            var metrics = await CalculateMetrics(dto.TestId, normalized);

            var metricResults = await ApplyEma(dto.UserId, dto.TestId, metrics);
            var scaleResults = await SaveScaleResults(dto, scaleScores, normalized);

            await _db.SaveChangesAsync();

            return new ProcessTestResultDto
            {
                UserId = dto.UserId,
                TestId = dto.TestId,
                Scales = scaleResults,
                Metrics = metricResults
            };
        }

        private async Task Validate(SubmitTestDto dto)
        {
            if (dto.Answers == null || !dto.Answers.Any())
            {
                throw new Exception("Нет ответов");
            }

            var userExists = await _db.Users.AnyAsync(u => u.UserId == dto.UserId);
            if (!userExists)
            {
                throw new Exception("Пользователь не найден");
            }

            var testQuestionIds = await _db.Questions
                .Where(q => q.TestId == dto.TestId)
                .Select(q => q.QuestionId)
                .ToListAsync();

            if (testQuestionIds.Count == 0)
            {
                throw new Exception("Тест не найден или в нем нет вопросов");
            }

            var submittedQuestionIds = dto.Answers.Select(a => a.QuestionId).ToList();
            if (submittedQuestionIds.Distinct().Count() != submittedQuestionIds.Count)
            {
                throw new Exception("На один вопрос передано несколько ответов");
            }

            var missingQuestionIds = testQuestionIds.Except(submittedQuestionIds).ToList();
            if (missingQuestionIds.Count > 0)
            {
                throw new Exception("Переданы ответы не на все вопросы теста");
            }

            var foreignQuestionIds = submittedQuestionIds.Except(testQuestionIds).ToList();
            if (foreignQuestionIds.Count > 0)
            {
                throw new Exception("Переданы ответы на вопросы из другого теста");
            }

            var answerIds = dto.Answers.Select(a => a.AnswerId).ToList();
            if (answerIds.Distinct().Count() != answerIds.Count)
            {
                throw new Exception("Один вариант ответа передан несколько раз");
            }

            var answerQuestionMap = await _db.AnswerOptions
                .Where(a => answerIds.Contains(a.AnswerId))
                .Select(a => new { a.AnswerId, a.QuestionId })
                .ToListAsync();

            if (answerQuestionMap.Count != answerIds.Count)
            {
                throw new Exception("Один или несколько вариантов ответа не найдены");
            }

            foreach (var submitted in dto.Answers)
            {
                var answer = answerQuestionMap.First(a => a.AnswerId == submitted.AnswerId);
                if (answer.QuestionId != submitted.QuestionId)
                {
                    throw new Exception("Вариант ответа не принадлежит указанному вопросу");
                }
            }
        }

        private async Task<List<AnswerData>> GetAnswersData(SubmitTestDto dto)
        {
            var answerIds = dto.Answers.Select(a => a.AnswerId).ToList();

            var answers = await _db.AnswerOptions
                .Where(a => answerIds.Contains(a.AnswerId))
                .Select(a => new AnswerData
                {
                    AnswerId = a.AnswerId,
                    Value = a.Value,
                    QuestionId = a.QuestionId,
                    MinAnswerValue = _db.AnswerOptions
                        .Where(option => option.QuestionId == a.QuestionId)
                        .Min(option => option.Value),
                    MaxAnswerValue = _db.AnswerOptions
                        .Where(option => option.QuestionId == a.QuestionId)
                        .Max(option => option.Value),
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

            if (answers.Any(a => a.Scales.Count == 0))
            {
                throw new Exception("Один или несколько вопросов не привязаны к шкалам");
            }

            return answers;
        }

        private Dictionary<int, decimal> CalculateScaleScores(List<AnswerData> answers)
        {
            var result = new Dictionary<int, decimal>();

            foreach (var answer in answers)
            {
                foreach (var scale in answer.Scales)
                {
                    if (!result.ContainsKey(scale.ScaleId))
                    {
                        result[scale.ScaleId] = 0;
                    }

                    result[scale.ScaleId] += answer.Value * scale.Weight;
                }
            }

            return result;
        }

        private Dictionary<int, ScaleRange> CalculateScaleRanges(List<AnswerData> answers)
        {
            var result = new Dictionary<int, ScaleRange>();

            foreach (var answer in answers)
            {
                foreach (var scale in answer.Scales)
                {
                    var minContribution = answer.MinAnswerValue * scale.Weight;
                    var maxContribution = answer.MaxAnswerValue * scale.Weight;

                    if (minContribution > maxContribution)
                    {
                        (minContribution, maxContribution) = (maxContribution, minContribution);
                    }

                    if (!result.ContainsKey(scale.ScaleId))
                    {
                        result[scale.ScaleId] = new ScaleRange();
                    }

                    result[scale.ScaleId].Min += minContribution;
                    result[scale.ScaleId].Max += maxContribution;
                }
            }

            return result;
        }

        private Dictionary<int, decimal> Normalize(
            Dictionary<int, decimal> raw,
            Dictionary<int, ScaleRange> ranges)
        {
            return raw.ToDictionary(
                x => x.Key,
                x =>
                {
                    var range = ranges[x.Key].Max - ranges[x.Key].Min;
                    if (range == 0)
                    {
                        return 0m;
                    }

                    return Clamp((x.Value - ranges[x.Key].Min) / range, 0m, 1m);
                });
        }

        private async Task<Dictionary<int, decimal>> CalculateMetrics(
            int testId,
            Dictionary<int, decimal> normalized)
        {
            var scaleIds = normalized.Keys.ToList();

            var links = await _db.TestScaleMetrics
                .Where(x => x.TestId == testId && scaleIds.Contains(x.ScaleId))
                .ToListAsync();

            if (links.Count == 0)
            {
                throw new Exception("Для теста не настроены связи шкал с показателями мониторинга");
            }

            var result = new Dictionary<int, decimal>();

            foreach (var link in links)
            {
                var value = normalized[link.ScaleId];

                if (!result.ContainsKey(link.MetricId))
                {
                    result[link.MetricId] = 0;
                }

                result[link.MetricId] += value * (decimal)link.Weight;
            }

            foreach (var key in result.Keys.ToList())
            {
                result[key] = Clamp(result[key] * 100, 0m, 100m);
            }

            return result;
        }

        private async Task<List<ProcessedMetricResultDto>> ApplyEma(
            int userId,
            int testId,
            Dictionary<int, decimal> metrics)
        {
            const decimal alpha = 0.35m;
            var createdAt = DateTime.UtcNow;

            var metricIds = metrics.Keys.ToList();
            var metricMeta = await _db.Metrics
                .Where(m => metricIds.Contains(m.MetricId))
                .ToDictionaryAsync(m => m.MetricId, m => new { m.Name, m.IsPositive });

            var existingMetrics = await _db.UserMetrics
                .Where(x => x.UserId == userId)
                .ToListAsync();

            var result = new List<ProcessedMetricResultDto>();

            foreach (var metric in metrics)
            {
                var existing = existingMetrics.FirstOrDefault(x => x.MetricId == metric.Key);
                var emaValue = metric.Value;

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
                    existing.Value = metric.Value * alpha + existing.Value * (1 - alpha);
                    emaValue = existing.Value;
                }

                _db.UserMetricSnapshots.Add(new UserMetricSnapshot
                {
                    UserId = userId,
                    MetricId = metric.Key,
                    Value = metric.Value,
                    SourceTestId = testId,
                    CreatedAt = createdAt
                });

                result.Add(new ProcessedMetricResultDto
                {
                    MetricId = metric.Key,
                    MetricName = metricMeta.GetValueOrDefault(metric.Key)?.Name ?? $"Metric {metric.Key}",
                    IsPositive = metricMeta.GetValueOrDefault(metric.Key)?.IsPositive ?? false,
                    CurrentValue = metric.Value,
                    EmaValue = emaValue
                });
            }

            return result;
        }

        private async Task<List<ProcessedScaleResultDto>> SaveScaleResults(
            SubmitTestDto dto,
            Dictionary<int, decimal> raw,
            Dictionary<int, decimal> normalized)
        {
            var scaleIds = raw.Keys.ToList();
            var scaleMeta = await _db.Scales
                .Where(s => scaleIds.Contains(s.ScaleId))
                .ToDictionaryAsync(s => s.ScaleId, s => new { s.Name, s.IsPositive });

            var result = new List<ProcessedScaleResultDto>();
            var createdAt = DateTime.UtcNow;

            foreach (var scale in raw)
            {
                _db.UserScaleResults.Add(new UserScaleResult
                {
                    UserId = dto.UserId,
                    ScaleId = scale.Key,
                    RawScore = scale.Value,
                    NormalizedScore = normalized[scale.Key],
                    SourceTestId = dto.TestId,
                    CreatedAt = createdAt
                });

                result.Add(new ProcessedScaleResultDto
                {
                    ScaleId = scale.Key,
                    ScaleName = scaleMeta.GetValueOrDefault(scale.Key)?.Name ?? $"Scale {scale.Key}",
                    IsPositive = scaleMeta.GetValueOrDefault(scale.Key)?.IsPositive ?? false,
                    RawScore = scale.Value,
                    NormalizedScore = normalized[scale.Key]
                });
            }

            return result;
        }

        private static decimal Clamp(decimal value, decimal min, decimal max)
        {
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
        }

        private class AnswerData
        {
            public int AnswerId { get; set; }
            public decimal Value { get; set; }
            public int QuestionId { get; set; }
            public decimal MinAnswerValue { get; set; }
            public decimal MaxAnswerValue { get; set; }
            public List<ScaleLink> Scales { get; set; } = new();
        }

        private class ScaleLink
        {
            public int ScaleId { get; set; }
            public decimal Weight { get; set; }
        }

        private class ScaleRange
        {
            public decimal Min { get; set; }
            public decimal Max { get; set; }
        }
    }
}
