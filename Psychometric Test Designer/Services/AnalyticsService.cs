using Microsoft.EntityFrameworkCore;
using Psychometric_Test_Designer.Data;
using Psychometric_Test_Designer.DTOs;

namespace Psychometric_Test_Designer.Services
{
    public class AnalyticsService
    {
        private readonly AppDbContext _db;

        public AnalyticsService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<AdminAnalyticsDto> GetAdminAnalytics()
        {
            var totalUsers = await _db.Users.CountAsync();
            var totalGroups = await _db.Groups.CountAsync(group => (group.StudentCount ?? 0) > 0);
            var totalTests = await _db.Tests.CountAsync();
            var totalSubmissions = await _db.UserScaleResults
                .Select(usr => usr.SourceTestId)
                .Distinct()
                .CountAsync();
            var totalFeedback = await _db.TextFeedback.CountAsync();
            var totalAnswers = await _db.AnswerOptions.CountAsync();
            var studentsCount = await _db.Users.CountAsync(u => u.Role == "Student");
            var testsTaken = await _db.UserScaleResults
                .GroupBy(usr => usr.UserId)
                .CountAsync();

            return new AdminAnalyticsDto
            {
                TotalUsers = totalUsers,
                TotalStudents = studentsCount,
                TotalGroups = totalGroups,
                TotalTests = totalTests,
                TotalTestsTaken = testsTaken,
                TotalFeedback = totalFeedback,
                TotalAnswers = totalAnswers,
                FillRate = studentsCount > 0 ? Math.Round((decimal)testsTaken / studentsCount * 100, 1) : 0
            };
        }

        public async Task<List<GroupFillRateDto>> GetGroupFillRates()
        {
            return await _db.Groups
                .AsNoTracking()
                .Where(g => (g.StudentCount ?? 0) > 0)
                .Select(g => new GroupFillRateDto
                {
                    GroupId = g.GroupId,
                    GroupName = g.GroupName,
                    StudentCount = g.StudentCount ?? 0,
                    ActiveStudents = _db.UserScaleResults
                        .Where(usr => usr.User.GroupId == g.GroupId)
                        .Select(usr => usr.UserId)
                        .Distinct()
                        .Count(),
                    FeedbackCount = _db.TextFeedback
                        .Count(f => f.GroupId == g.GroupId)
                })
                .ToListAsync();
        }

        public async Task<List<StudentRecommendationDto>> GetRecommendations(int userId)
        {
            var metrics = await _db.UserMetrics
                .AsNoTracking()
                .Where(um => um.UserId == userId)
                .Select(um => new { um.Metric.Name, um.Value })
                .ToListAsync();

            var result = new List<StudentRecommendationDto>();

            foreach (var metric in metrics)
            {
                var rec = InterpretMetric(metric.Name, metric.Value);
                if (rec != null)
                    result.Add(rec);
            }

            return result;
        }

        private static StudentRecommendationDto? InterpretMetric(string name, decimal value)
        {
            if (name.Contains("стресс", StringComparison.OrdinalIgnoreCase))
            {
                if (value >= 70)
                    return new() { MetricName = name, Level = "Высокий", Advice = "Рекомендуется обратиться к психологу. Попробуйте техники дыхания и отдых." };
                if (value >= 40)
                    return new() { MetricName = name, Level = "Средний", Advice = "Старайтесь чередовать учёбу и отдых. Обсуждайте нагрузку с одногруппниками." };
                return new() { MetricName = name, Level = "Низкий", Advice = "У вас всё хорошо! Продолжайте поддерживать текущий режим." };
            }

            if (name.Contains("благополуч", StringComparison.OrdinalIgnoreCase))
            {
                if (value <= 30)
                    return new() { MetricName = name, Level = "Низкий", Advice = "Обратите внимание на своё состояние. Поговорите с близкими или психологом." };
                if (value <= 60)
                    return new() { MetricName = name, Level = "Средний", Advice = "Есть куда расти. Попробуйте больше времени уделять хобби и общению." };
                return new() { MetricName = name, Level = "Высокий", Advice = "Отличный уровень! Делитесь позитивом с окружающими." };
            }

            if (name.Contains("тревожн", StringComparison.OrdinalIgnoreCase))
            {
                if (value >= 70)
                    return new() { MetricName = name, Level = "Высокий", Advice = "Уровень тревожности повышен. Рекомендуется консультация специалиста." };
                if (value >= 40)
                    return new() { MetricName = name, Level = "Средний", Advice = "Постарайтесь снизить тревогу: прогулки, спорт, разговоры с друзьями." };
                return new() { MetricName = name, Level = "Низкий", Advice = "Вы спокойны и уравновешены. Так держать!" };
            }

            if (name.Contains("изоляц", StringComparison.OrdinalIgnoreCase))
            {
                if (value >= 60)
                    return new() { MetricName = name, Level = "Высокий", Advice = "Постарайтесь больше общаться с группой. Одиночество усугубляет стресс." };
                return new() { MetricName = name, Level = "Нормальный", Advice = "Вы хорошо социализированы в группе." };
            }

            return null;
        }
    }
}
