-- Таблица групп
CREATE TABLE IF NOT EXISTS groups (
    group_id SERIAL PRIMARY KEY,
    group_name VARCHAR(4) UNIQUE,
    specialization VARCHAR(200),
    student_count int
);
-- Таблица токенов
CREATE TABLE IF NOT EXISTS tokens_for_groups (
    token_id VARCHAR(50) PRIMARY KEY,
    group_id INT REFERENCES groups(group_id) ON UPDATE CASCADE,
    number_of_uses INT
);
-- Таблица пользователей
CREATE TABLE IF NOT EXISTS users (
    user_id SERIAL PRIMARY KEY,
    login VARCHAR(100) UNIQUE,
    password TEXT,
    role VARCHAR(30) NOT NULL DEFAULT 'Student',
    group_id INT REFERENCES groups(group_id) ON DELETE CASCADE,
    created_at TIMESTAMP DEFAULT NOW()
);
-- Таблица метрик (константные)
CREATE TABLE IF NOT EXISTS metrics (
    metric_id SERIAL PRIMARY KEY,
    name VARCHAR(100) UNIQUE,
    description TEXT,
    is_positive BOOLEAN NOT NULL DEFAULT FALSE
);
-- Таблица шкал
CREATE TABLE IF NOT EXISTS scales (
    scale_id SERIAL PRIMARY KEY,
    name VARCHAR(100),
    description TEXT,
    is_positive BOOLEAN NOT NULL DEFAULT FALSE
);
-- Метрики каждого пользователя
CREATE TABLE IF NOT EXISTS user_metrics (
    user_id INT REFERENCES users(user_id) ON DELETE CASCADE,
    metric_id INT REFERENCES metrics(metric_id) ON DELETE CASCADE,
    user_metric_value NUMERIC NOT NULL DEFAULT 0,
    PRIMARY KEY(user_id, metric_id)
);
-- Таблица тестов
CREATE TABLE IF NOT EXISTS tests (
    test_id SERIAL PRIMARY KEY,
    title VARCHAR(255),
    created_by INT REFERENCES users(user_id) ON DELETE
    SET NULL,
        created_at TIMESTAMP DEFAULT NOW()
);
-- Таблица вопросов
CREATE TABLE IF NOT EXISTS questions (
    question_id SERIAL PRIMARY KEY,
    test_id INT REFERENCES tests(test_id) ON DELETE CASCADE,
    text TEXT NOT NULL
);
-- Таблица ответов
CREATE TABLE IF NOT EXISTS answers (
    answer_id SERIAL PRIMARY KEY,
    question_id INT REFERENCES questions(question_id) ON DELETE CASCADE,
    text TEXT NOT NULL,
    answer_value NUMERIC NOT NULL
);
-- Связь: вопрос -> шкала
CREATE TABLE IF NOT EXISTS question_scale (
    question_id INT REFERENCES questions(question_id) ON DELETE CASCADE,
    scale_id INT REFERENCES scales(scale_id) ON DELETE CASCADE,
    weight NUMERIC NOT NULL,
    PRIMARY KEY(question_id, scale_id)
);
-- Связь: шкала -> метрика (в контексте теста)
CREATE TABLE IF NOT EXISTS test_scale_metric (
    test_id INT REFERENCES tests(test_id) ON DELETE CASCADE,
    scale_id INT REFERENCES scales(scale_id) ON DELETE CASCADE,
    metric_id INT REFERENCES metrics(metric_id) ON DELETE CASCADE,
    weight NUMERIC NOT NULL,
    -- сумма по (test_id, metric_id) = 1 (и только лишь 1)
    PRIMARY KEY(test_id, scale_id, metric_id)
);
-- Таблица результат шкал 
CREATE TABLE IF NOT EXISTS user_scale_results (
    usr_id SERIAL PRIMARY KEY,
    user_id INT REFERENCES users(user_id) ON DELETE CASCADE,
    scale_id INT REFERENCES scales(scale_id) ON DELETE CASCADE,
    raw_score NUMERIC,
    normalized_score NUMERIC,
    source_test_id INT REFERENCES tests(test_id),
    created_at TIMESTAMP DEFAULT NOW()
);
-- Снапшоты метрик пользователей (раз в N дней)
CREATE TABLE IF NOT EXISTS user_metric_snapshots (
    ums_id SERIAL PRIMARY KEY,
    user_id INT REFERENCES users(user_id) ON DELETE CASCADE,
    metric_id INT REFERENCES metrics(metric_id) ON DELETE CASCADE,
    value NUMERIC,
    source_test_id INT REFERENCES tests(test_id),
    created_at TIMESTAMP DEFAULT NOW()
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
