using Microsoft.EntityFrameworkCore;

namespace Psychometric_Test_Designer.Data
{
    public static class DatabaseBootstrapper
    {
        public static async Task EnsureSchemaAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await db.Database.ExecuteSqlRawAsync("""
                ALTER TABLE users
                ADD COLUMN IF NOT EXISTS role VARCHAR(30) NOT NULL DEFAULT 'Student';

                ALTER TABLE users
                ADD COLUMN IF NOT EXISTS full_name VARCHAR(200) NOT NULL DEFAULT '';

                ALTER TABLE user_metric_snapshots
                ADD COLUMN IF NOT EXISTS source_test_id INT REFERENCES tests(test_id),
                ADD COLUMN IF NOT EXISTS created_at TIMESTAMP DEFAULT NOW();

                ALTER TABLE scales
                ADD COLUMN IF NOT EXISTS is_positive BOOLEAN NOT NULL DEFAULT FALSE;

                ALTER TABLE metrics
                ADD COLUMN IF NOT EXISTS is_positive BOOLEAN NOT NULL DEFAULT FALSE;

                UPDATE scales
                SET is_positive = TRUE
                WHERE name IN (
                    'Восстановление',
                    'Индекс благополучия',
                    'Поддержка группы',
                    'Психологическая безопасность',
                    'Учебная вовлеченность',
                    'Ясность учебных задач',
                    'Настроение',
                    'Эмоциональная стабильность'
                );

                UPDATE metrics
                SET is_positive = TRUE
                WHERE name IN (
                    'Индекс благополучия',
                    'Восстановление',
                    'Эмоциональная стабильность'
                );

                CREATE TABLE IF NOT EXISTS text_feedback (
                    feedback_id SERIAL PRIMARY KEY,
                    user_id INT REFERENCES users(user_id) ON DELETE SET NULL,
                    group_id INT REFERENCES groups(group_id) ON DELETE CASCADE,
                    text TEXT NOT NULL,
                    sentiment_score NUMERIC NOT NULL DEFAULT 0,
                    topics TEXT,
                    created_at TIMESTAMP DEFAULT NOW()
                );

                CREATE TABLE IF NOT EXISTS test_assignments (
                    assignment_id SERIAL PRIMARY KEY,
                    test_id INT NOT NULL REFERENCES tests(test_id) ON DELETE CASCADE,
                    group_id INT NOT NULL REFERENCES groups(group_id) ON DELETE CASCADE,
                    opens_at TIMESTAMP NOT NULL,
                    closes_at TIMESTAMP NOT NULL,
                    created_at TIMESTAMP DEFAULT NOW()
                );

                CREATE TABLE IF NOT EXISTS app_seed_state (
                    key TEXT PRIMARY KEY,
                    value TEXT NOT NULL
                );
                """);
        }
    }
}
