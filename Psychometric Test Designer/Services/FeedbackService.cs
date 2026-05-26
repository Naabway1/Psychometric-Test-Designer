using Microsoft.EntityFrameworkCore;
using Psychometric_Test_Designer.Data;
using Psychometric_Test_Designer.DTOs;
using Psychometric_Test_Designer.Models;

namespace Psychometric_Test_Designer.Services
{
    public class FeedbackService
    {
        private static readonly Dictionary<string, string[]> TopicKeywords = new()
        {
            ["усталость"] = ["устал", "усталость", "выгор", "выгорание", "сил нет", "нет сил", "истощ", "сонлив", "не высп", "перегруз", "tired", "burnout", "exhaust"],
            ["конфликт"] = ["конфликт", "ссора", "руга", "токсич", "давление", "агресс", "буллинг", "травля", "оскорб", "напряж", "conflict", "toxic", "pressure", "bullying"],
            ["учеба"] = ["сложно", "непонятно", "задания", "контрольная", "долги", "дедлайн", "экзамен", "зачет", "тема", "лаборатор", "unclear", "task", "study", "deadline"],
            ["преподаватели"] = ["преподав", "учитель", "пара", "объясн", "оценк", "замечан", "teacher", "explain", "grade"],
            ["нагрузка"] = ["нагрузка", "перегруз", "много заданий", "много пар", "не успева", "давят сроки", "workload", "overload"],
            ["инфраструктура"] = ["кабинет", "столов", "интернет", "компьютер", "оборуд", "аудитор", "температур", "шум", "internet", "computer"],
            ["поддержка"] = ["поддерж", "помог", "вместе", "довер", "команд", "понимают", "support", "help"],
            ["мотивация"] = ["мотивац", "интерес", "не хочу", "бессмыс", "цель", "будущее", "motivation", "interest"],
            ["тревожность"] = ["тревог", "страх", "паник", "пережива", "нерв", "anxiety", "panic", "fear"]
        };

        private static readonly string[] NegativeMarkers =
        [
            "устал", "плохо", "тяжело", "стресс", "конфликт", "непонятно",
            "выгор", "страх", "тревог", "давление", "проблем",
            "паник", "боюсь", "не успева", "сложно", "перегруз", "одинок", "токсич",
            "tired", "stress", "conflict", "unclear", "workload", "overload", "problem", "anxiety"
        ];

        private static readonly string[] PositiveMarkers =
        [
            "хорошо", "спокой", "понятно", "нормально", "интересно",
            "поддерж", "комфорт", "получилось",
            "помог", "лучше", "довер", "вместе", "успева", "безопас",
            "good", "calm", "clear", "support", "comfortable", "better"
        ];

        private readonly AppDbContext _db;

        public FeedbackService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<FeedbackResponseDto> Submit(int userId, FeedbackSubmitDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Text))
            {
                throw new Exception("Текст обратной связи обязателен");
            }

            var user = await _db.Users
                .AsNoTracking()
                .Include(u => u.Group)
                .FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                throw new Exception("Пользователь не найден");
            }

            var topics = DetectTopics(dto.Text);
            var feedback = new TextFeedback
            {
                UserId = user.UserId,
                GroupId = user.GroupId,
                Text = dto.Text.Trim(),
                SentimentScore = CalculateSentiment(dto.Text),
                Topics = string.Join(',', topics),
                CreatedAt = DateTime.UtcNow
            };

            _db.TextFeedback.Add(feedback);
            await _db.SaveChangesAsync();
            feedback.User = user;
            feedback.Group = user.Group;

            return ToDto(feedback);
        }

        public async Task<GroupFeedbackTrendDto> GetGroupTrends(int groupId)
        {
            var group = await _db.Groups.AsNoTracking().FirstOrDefaultAsync(g => g.GroupId == groupId);
            if (group == null)
            {
                throw new Exception("Группа не найдена");
            }

            var feedback = await _db.TextFeedback
                .AsNoTracking()
                .Include(f => f.User)
                .Include(f => f.Group)
                .Where(f => f.GroupId == groupId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            var topics = feedback
                .SelectMany(f => SplitTopics(f.Topics))
                .GroupBy(t => t)
                .Select(g => new FeedbackTopicDto
                {
                    Topic = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(t => t.Count)
                .ThenBy(t => t.Topic)
                .ToList();

            return new GroupFeedbackTrendDto
            {
                GroupId = group.GroupId,
                GroupName = group.GroupName,
                TotalMessages = feedback.Count,
                AverageSentiment = feedback.Count == 0 ? 0 : feedback.Average(f => f.SentimentScore),
                Topics = topics,
                RecentMessages = feedback.Take(10).Select(ToDto).ToList()
            };
        }

        public async Task<GroupFeedbackTrendDto> GetAllTrends()
        {
            var feedback = await _db.TextFeedback
                .AsNoTracking()
                .Include(f => f.User)
                .Include(f => f.Group)
                .OrderByDescending(f => f.CreatedAt)
                .Take(50)
                .ToListAsync();

            var topics = feedback
                .SelectMany(f => SplitTopics(f.Topics))
                .GroupBy(t => t)
                .Select(g => new FeedbackTopicDto
                {
                    Topic = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(t => t.Count)
                .ThenBy(t => t.Topic)
                .ToList();

            return new GroupFeedbackTrendDto
            {
                GroupId = 0,
                GroupName = "Все группы",
                TotalMessages = feedback.Count,
                AverageSentiment = feedback.Count == 0 ? 0 : feedback.Average(f => f.SentimentScore),
                Topics = topics,
                RecentMessages = feedback.Select(ToDto).ToList()
            };
        }

        private static FeedbackResponseDto ToDto(TextFeedback feedback)
        {
            return new FeedbackResponseDto
            {
                FeedbackId = feedback.FeedbackId,
                GroupId = feedback.GroupId,
                GroupName = feedback.Group?.GroupName ?? string.Empty,
                UserId = feedback.UserId,
                StudentFullName = string.IsNullOrWhiteSpace(feedback.User?.FullName)
                    ? feedback.User?.Login ?? "Неизвестный студент"
                    : feedback.User.FullName,
                StudentLogin = feedback.User?.Login ?? string.Empty,
                Text = feedback.Text,
                SentimentScore = feedback.SentimentScore,
                Topics = SplitTopics(feedback.Topics),
                CreatedAt = feedback.CreatedAt
            };
        }

        private static List<string> DetectTopics(string text)
        {
            var lower = text.ToLowerInvariant();
            var topics = TopicKeywords
                .Where(topic => topic.Value.Any(lower.Contains))
                .Select(topic => topic.Key)
                .Distinct()
                .ToList();

            if (topics.Count == 0)
            {
                topics.Add("общее");
            }

            return topics;
        }

        private static decimal CalculateSentiment(string text)
        {
            var lower = text.ToLowerInvariant();
            var negative = NegativeMarkers.Count(lower.Contains);
            var positive = PositiveMarkers.Count(lower.Contains);

            return Math.Clamp((positive - negative) * 25m, -100m, 100m);
        }

        private static List<string> SplitTopics(string topics)
        {
            if (string.IsNullOrWhiteSpace(topics))
            {
                return new List<string>();
            }

            return topics
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();
        }
    }
}
