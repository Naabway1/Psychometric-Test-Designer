using Microsoft.EntityFrameworkCore;
using Psychometric_Test_Designer.Models;

namespace Psychometric_Test_Designer.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Таблицы
        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Test> Tests { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Scale> Scales { get; set; }
        public DbSet<Metric> Metrics { get; set; }
        public DbSet<QuestionScale> QuestionScales { get; set; }
        public DbSet<TestScaleMetric> TestScaleMetrics { get; set; }
        public DbSet<UserMetric> UserMetrics { get; set; }
        public DbSet<AnswerOption> AnswerOptions { get; set; }
        public DbSet<UserMetricSnapshot> UserMetricSnapshots { get; set; }
        public DbSet<UserScaleResult> UserScaleResults { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<QuestionScale>().HasKey(qs => new { qs.QuestionId, qs.ScaleId });
            modelBuilder.Entity<TestScaleMetric>().HasKey(tsm => new { tsm.TestId, tsm.ScaleId, tsm.MetricId });
            modelBuilder.Entity<UserMetric>().HasKey(um => new { um.UserId, um.MetricId });
        }
    }
}
