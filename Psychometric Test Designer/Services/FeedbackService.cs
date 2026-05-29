using Microsoft.EntityFrameworkCore;
using Psychometric_Test_Designer.Data;
using Psychometric_Test_Designer.DTOs;
using Psychometric_Test_Designer.Models;

namespace Psychometric_Test_Designer.Services
{
    public class FeedbackService
    {
        private static readonly Dictionary<string, string[]> TopicKeywords = new(StringComparer.OrdinalIgnoreCase)
        {
            ["кризисное состояние"] = ["суицид", "самоуб", "самоповреж", "наложить на себя руки", "покончить с собой", "не хочу жить", "хочется умереть", "свести счеты", "резать себя", "выйти в окно"],
            ["тревожность"] = ["тревог", "страх", "паник", "пережива", "нерв", "дрож", "ужас", "боюсь", "anxiety", "panic", "fear"],
            ["депрессивное состояние"] = ["тлен", "безысход", "пустота", "апат", "депресс", "не вижу смысла", "бессмыс", "ничего не радует", "одинок", "отчаян", "hopeless"],
            ["усталость"] = ["устал", "усталость", "выгор", "выгорание", "сил нет", "нет сил", "истощ", "сонлив", "не высп", "изнур", "разбит", "tired", "burnout", "exhaust"],
            ["конфликт"] = ["конфликт", "ссора", "руга", "токсич", "давление", "агресс", "буллинг", "травля", "оскорб", "унижа", "угрож", "напряж", "conflict", "toxic", "pressure", "bullying"],
            ["учеба"] = ["сложно", "непонятно", "задания", "контрольная", "долги", "дедлайн", "экзамен", "зачет", "тема", "лаборатор", "не понимаю", "unclear", "task", "study", "deadline"],
            ["преподаватели"] = ["преподав", "учитель", "пара", "объясн", "оценк", "замечан", "teacher", "explain", "grade"],
            ["нагрузка"] = ["нагрузка", "перегруз", "много заданий", "много пар", "не успева", "давят сроки", "завал", "долги", "workload", "overload"],
            ["инфраструктура"] = ["кабинет", "столов", "интернет", "компьютер", "оборуд", "аудитор", "температур", "шум", "internet", "computer"],
            ["поддержка"] = ["поддерж", "помог", "вместе", "довер", "команд", "понимают", "не один", "support", "help"],
            ["мотивация"] = ["мотивац", "интерес", "не хочу", "не могу заставить", "цель", "будущее", "скучно", "motivation", "interest"]
        };

        private static readonly WeightedTerm[] CrisisMarkers =
        [
            new("наложить на себя руки", 11m),
            new("покончить с собой", 11m),
            new("свести счеты", 10m),
            new("не хочу жить", 10m),
            new("хочется умереть", 10m),
            new("лучше умереть", 10m),
            new("суицид", 10m),
            new("самоуб", 10m),
            new("самоповреж", 9m),
            new("резать себя", 9m),
            new("выйти в окно", 9m)
        ];

        private static readonly WeightedTerm[] NegativeMarkers =
        [
            new("безысход", 5m), new("тлен", 5m), new("пустота", 4m), new("апат", 4m), new("депресс", 5m),
            new("отчаян", 4m), new("ненавиж", 4m), new("невыносим", 5m), new("не могу больше", 5m), new("ничего не радует", 4m),
            new("плохо", 3m), new("ужас", 4m), new("кошмар", 4m), new("тяжело", 3m), new("больно", 4m),
            new("страшно", 4m), new("боюсь", 3m), new("тревог", 4m), new("паник", 5m), new("нерв", 2m),
            new("стресс", 4m), new("срыв", 4m), new("плачу", 4m), new("слез", 3m), new("истер", 4m),
            new("устал", 3m), new("выгор", 4m), new("истощ", 4m), new("сил нет", 4m), new("нет сил", 4m),
            new("не высп", 2m), new("разбит", 3m), new("изнур", 3m), new("перегруз", 3m), new("завал", 3m),
            new("не успева", 3m), new("долг", 2m), new("дедлайн", 2m), new("сложно", 2m), new("непонятно", 2m),
            new("не понимаю", 2m), new("проблем", 2m), new("провал", 3m), new("неудач", 3m), new("стыд", 3m),
            new("вина", 3m), new("одинок", 4m), new("изол", 4m), new("никому не нужен", 5m), new("меня не слышат", 3m),
            new("конфликт", 3m), new("ссора", 3m), new("руга", 2m), new("агресс", 4m), new("травля", 5m),
            new("буллинг", 5m), new("унижа", 5m), new("оскорб", 4m), new("угрож", 5m), new("давление", 3m),
            new("токсич", 4m), new("раздраж", 3m), new("злюсь", 3m), new("бесит", 3m), new("тошнит", 3m),
            new("не хочу", 2m), new("бессмыс", 4m), new("скучно", 2m), new("нет мотивац", 3m), new("безразлич", 3m),
            new("hopeless", 5m), new("depress", 5m), new("panic", 5m), new("anxiety", 4m), new("stress", 4m),
            new("burnout", 4m), new("exhaust", 4m), new("lonely", 4m), new("bullying", 5m), new("toxic", 4m)
        ];

        private static readonly WeightedTerm[] PositiveMarkers =
        [
            new("спокой", 3m), new("хорошо", 3m), new("нормально", 2m), new("устойчив", 3m), new("стабиль", 3m),
            new("рад", 3m), new("легче", 3m), new("лучше", 3m), new("получилось", 3m), new("успева", 3m),
            new("понятно", 3m), new("ясно", 3m), new("интересно", 3m), new("мотивац", 3m), new("цель", 2m),
            new("поддерж", 4m), new("помог", 4m), new("вместе", 3m), new("довер", 4m), new("команд", 3m),
            new("комфорт", 4m), new("безопас", 4m), new("принят", 3m), new("слышат", 3m), new("уважа", 3m),
            new("справля", 3m), new("ресурс", 3m), new("восстанов", 4m), new("отдох", 3m), new("сон", 2m),
            new("good", 3m), new("calm", 3m), new("clear", 3m), new("support", 4m), new("comfortable", 4m), new("better", 3m)
        ];

        private static readonly string[] Intensifiers =
        [
            "очень", "сильно", "совсем", "крайне", "ужасно", "жутко", "невыносимо", "вообще", "прям", "постоянно"
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
            var lower = NormalizeText(text);
            var topics = TopicKeywords
                .Where(topic => topic.Value.Any(lower.Contains))
                .Select(topic => topic.Key)
                .Distinct()
                .ToList();

            if (CrisisMarkers.Any(marker => lower.Contains(marker.Pattern))
                && !topics.Contains("кризисное состояние"))
            {
                topics.Insert(0, "кризисное состояние");
            }

            if (topics.Count == 0 && NegativeMarkers.Any(marker => lower.Contains(marker.Pattern)))
            {
                topics.Add("эмоциональное напряжение");
            }

            if (topics.Count == 0)
            {
                topics.Add("общее");
            }

            return topics;
        }

        private static decimal CalculateSentiment(string text)
        {
            var lower = NormalizeText(text);
            var score = 0m;

            foreach (var marker in NegativeMarkers)
            {
                if (lower.Contains(marker.Pattern))
                {
                    score -= marker.Weight;
                }
            }

            foreach (var marker in PositiveMarkers)
            {
                if (lower.Contains(marker.Pattern))
                {
                    score += marker.Weight;
                }
            }

            foreach (var marker in CrisisMarkers)
            {
                if (lower.Contains(marker.Pattern))
                {
                    score -= marker.Weight;
                }
            }

            var intensifierMultiplier = 1m + Math.Min(0.45m, Intensifiers.Count(lower.Contains) * 0.12m);
            score *= intensifierMultiplier;

            return Math.Clamp(score * 14m, -100m, 100m);
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

        private static string NormalizeText(string text)
        {
            return string.Join(' ', text
                    .ToLowerInvariant()
                    .Replace('ё', 'е')
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                .Trim();
        }

        private sealed record WeightedTerm(string Pattern, decimal Weight);
    }
}
