-- Таблица групп
CREATE TABLE groups (
	group_id VARCHAR(4) PRIMARY KEY,
	specialization VARCHAR(200) UNIQUE
);

-- Таблица пользователей
CREATE TABLE users (
    user_id SERIAL PRIMARY KEY,
    login VARCHAR(100) UNIQUE,
    password TEXT,
    group VARCHAR(4) REFERENCES groups(group_id) ON DELETE CASCADE,
    created_at TIMESTAMP DEFAULT NOW()
);

-- Таблица метрик (константные)
CREATE TABLE metrics (
    metric_id SERIAL PRIMARY KEY,
    name VARCHAR(100) UNIQUE,
    description TEXT
);

-- Метрики каждого пользователя
CREATE TABLE user_metrics (
    user_id INT REFERENCES users(user_id) ON DELETE CASCADE,
    metric_id INT REFERENCES metrics(metric_id) ON DELETE CASCADE,
    value NUMERIC DEFAULT 0,
    PRIMARY KEY(user_id, metric_id)
);

-- Таблица тестов
CREATE TABLE tests (
    test_id SERIAL PRIMARY KEY,
    title VARCHAR(255),
    created_by INT REFERENCES users(user_id),
    created_at TIMESTAMP DEFAULT NOW()
);

-- Таблица вопросов
CREATE TABLE questions (
    question_id SERIAL PRIMARY KEY,
    test_id INT REFERENCES tests(test_id) ON DELETE CASCADE,
    text TEXT - текст самого вопроса
);

-- Таблица шкал
CREATE TABLE scales (
    scale_id SERIAL PRIMARY KEY,
    name VARCHAR(100),
    description TEXT
);

-- Таблица ответов
CREATE TABLE answers (
    answer_id SERIAL PRIMARY KEY,
    question_id INT REFERENCES questions(question_id) ON DELETE CASCADE,
    text TEXT - текст самого ответа
);

-- Таблица ответов пользователей
CREATE TABLE user_answers (
    question_id INT REFERENCES questions(question_id) ON DELETE CASCADE,
    answer_id INT REFERENCES answers(answer_id) ON DELETE CASCADE,
    PRIMARY KEY(attempt_id, question_id)
);

-- Влияние ответа на шкалу
CREATE TABLE answer_scale_effects (
    answer_id INT REFERENCES answers(answer_id) ON DELETE CASCADE,
    scale_id INT REFERENCES scales(scale_id) ON DELETE CASCADE,
    effect_value NUMERIC,  -- положительное или отрицательное влияние
    PRIMARY KEY(answer_id, scale_id)
);

-- Связь шкал с метриками
CREATE TABLE scale_metric_mapping (
    scale_id INT REFERENCES scales(scale_id) ON DELETE CASCADE,
    metric_id INT REFERENCES metrics(metric_id) ON DELETE CASCADE,
    weight NUMERIC, -- коэффициент преобразования шкалы в метрику
    PRIMARY KEY(scale_id, metric_id)
);

-- Снапшоты метрик пользователей (раз в N дней)
CREATE TABLE user_metric_snapshots (
    id SERIAL PRIMARY KEY,
    user_id INT REFERENCES users(user_id) ON DELETE CASCADE,
    metric_id INT REFERENCES metrics(metric_id) ON DELETE CASCADE,
    value NUMERIC,
    source_test_id INT REFERENCES tests(test_id),
    created_at TIMESTAMP DEFAULT NOW()
);