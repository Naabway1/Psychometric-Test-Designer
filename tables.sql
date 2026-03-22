-- Таблица групп
CREATE TABLE groups (
	group_id VARCHAR(4) PRIMARY KEY,
	specialization VARCHAR(200)
);

-- Таблица пользователей
CREATE TABLE users (
    user_id SERIAL PRIMARY KEY,
    login VARCHAR(100) UNIQUE,
    password TEXT,
    group_id VARCHAR(4) REFERENCES groups(group_id) ON DELETE CASCADE,
    created_at TIMESTAMP DEFAULT NOW()
);

-- Таблица метрик (константные)
CREATE TABLE metrics (
    metric_id SERIAL PRIMARY KEY,
    name VARCHAR(100) UNIQUE,
    description TEXT
);

-- Таблица шкал
CREATE TABLE scales (
    scale_id SERIAL PRIMARY KEY,
    name VARCHAR(100),
    description TEXT
);

-- Метрики каждого пользователя
CREATE TABLE user_metrics (
    user_id INT REFERENCES users(user_id) ON DELETE CASCADE,
    metric_id INT REFERENCES metrics(metric_id) ON DELETE CASCADE,
    user_metric_value NUMERIC NOT NULL DEFAULT 0,
    PRIMARY KEY(user_id, metric_id)
);

-- Таблица тестов
CREATE TABLE tests (
    test_id SERIAL PRIMARY KEY,
    title VARCHAR(255),
    created_by INT REFERENCES users(user_id) SET NULL,
    created_at TIMESTAMP DEFAULT NOW()
);

-- Таблица вопросов
CREATE TABLE questions (
    question_id SERIAL PRIMARY KEY,
    test_id INT REFERENCES tests(test_id) ON DELETE CASCADE,
    text TEXT NOT NULL
);

-- Таблица ответов
CREATE TABLE answers (
    answer_id SERIAL PRIMARY KEY,
    question_id INT REFERENCES questions(question_id) ON DELETE CASCADE,
    text TEXT NOT NULL,
    answer_value NUMERIC NOT NULL
);

-- Связь: вопрос -> шкала
CREATE TABLE question_scale (
    question_id INT REFERENCES questions(question_id) ON DELETE CASCADE,
    scale_id INT REFERENCES scales(scale_id) ON DELETE CASCADE,
    weight NUMERIC NOT NULL,
    PRIMARY KEY(question_id, scale_id)
);

-- Связь: шкала -> метрика (в контексте теста)
CREATE TABLE test_scale_metric (
    test_id INT REFERENCES tests(test_id) ON DELETE CASCADE,
    scale_id INT REFERENCES scales(scale_id) ON DELETE CASCADE,
    metric_id INT REFERENCES metrics(metric_id) ON DELETE CASCADE,
    weight NUMERIC NOT NULL, -- сумма по (test_id, metric_id) = 1 (и только лишь 1)
    PRIMARY KEY(test_id, scale_id, metric_id)
);

-- Таблица результат шкал 
CREATE TABLE user_scale_results (
    usr_id SERIAL PRIMARY KEY,
    user_id INT REFERENCES users(user_id) ON DELETE CASCADE,
    scale_id INT REFERENCES scales(scale_id) ON DELETE CASCADE,
    raw_score NUMERIC,
    normalized_score NUMERIC,
    source_test_id INT REFERENCES tests(test_id),
    created_at TIMESTAMP DEFAULT NOW()
);

-- Снапшоты метрик пользователей (раз в N дней)
CREATE TABLE user_metric_snapshots (
    ums_id SERIAL PRIMARY KEY,
    user_id INT REFERENCES users(user_id) ON DELETE CASCADE,
    metric_id INT REFERENCES metrics(metric_id) ON DELETE CASCADE,
    value NUMERIC,
    source_test_id INT REFERENCES tests(test_id),
    created_at TIMESTAMP DEFAULT NOW()
);
