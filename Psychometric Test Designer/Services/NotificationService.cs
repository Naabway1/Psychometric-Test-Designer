using Microsoft.EntityFrameworkCore;
using Psychometric_Test_Designer.Data;
using Psychometric_Test_Designer.DTOs;

namespace Psychometric_Test_Designer.Services
{
    public class NotificationService
    {
        private readonly AppDbContext _db;

        public NotificationService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<TriggerAlertDto>> GetActiveAlerts()
        {
            var alerts = new List<TriggerAlertDto>();

            var groups = await _db.Groups
                .AsNoTracking()
                .Where(group => (group.StudentCount ?? 0) > 0)
                .ToListAsync();

            foreach (var group in groups)
            {
                var metrics = await _db.UserMetrics
                    .AsNoTracking()
                    .Where(um => um.User.GroupId == group.GroupId && um.User.Role == "Student")
                    .GroupBy(um => new { um.MetricId, um.Metric.Name, um.Metric.IsPositive })
                    .Select(g => new { g.Key.Name, g.Key.IsPositive, AvgValue = g.Average(x => x.Value) })
                    .ToListAsync();

                foreach (var metric in metrics)
                {
                    var threshold = GetThreshold(metric.Name);
                    var riskValue = metric.IsPositive ? 100m - metric.AvgValue : metric.AvgValue;
                    if (threshold.HasValue && riskValue >= threshold.Value)
                    {
                        var severity = riskValue switch
                        {
                            >= 80m => "critical",
                            >= 70m => "high",
                            _ => "medium"
                        };

                        alerts.Add(new TriggerAlertDto
                        {
                            GroupId = group.GroupId,
                            GroupName = group.GroupName,
                            MetricName = metric.Name,
                            CurrentValue = Math.Round(metric.AvgValue, 1),
                            Threshold = threshold.Value,
                            Severity = severity,
                            Message = $"Группа {group.GroupName}: показатель «{metric.Name}» достиг {Math.Round(metric.AvgValue, 1)} (порог: {threshold.Value})."
                        });
                    }
                }
            }

            return alerts.OrderByDescending(a => a.Severity).ThenByDescending(a => a.CurrentValue).ToList();
        }

        private static int? GetThreshold(string metricName)
        {
            var name = metricName.ToLowerInvariant();

            if (name.Contains("стресс") || name.Contains("тревожн"))
                return 65;

            if (name.Contains("изоляц"))
                return 55;

            return null;
        }
    }
}
