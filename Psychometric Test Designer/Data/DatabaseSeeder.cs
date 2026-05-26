using Microsoft.EntityFrameworkCore;
using Psychometric_Test_Designer.Core;
using Psychometric_Test_Designer.Models;
using Psychometric_Test_Designer.Services;

namespace Psychometric_Test_Designer.Data
{
    public static class DatabaseSeeder
    {
        private const string SeedVersion = "2026-05-25-mixed-scale-values";
        private const string SeedVersionKey = "demo_seed_version";

        private static readonly string[] TargetGroups =
        [
            "163", "165", "167", "169",
            "263", "265", "267", "269",
            "363", "365", "367", "369",
            "463", "465", "467", "469"
        ];

        private static readonly DateTime SeedNow = new(2026, 5, 15, 7, 0, 0, DateTimeKind.Utc);

        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var passwordService = scope.ServiceProvider.GetRequiredService<PasswordService>();

            if (!await ShouldReseed(db))
            {
                return;
            }

            await ClearDemoData(db);

            var random = new Random(20260515);

            var groups = CreateGroups();
            db.Groups.AddRange(groups);
            await db.SaveChangesAsync();

            var staff = CreateStaffUsers(groups[0], passwordService);
            var students = CreateStudents(groups, passwordService);
            db.Users.AddRange(staff);
            db.Users.AddRange(students);
            await db.SaveChangesAsync();

            var admin = staff.First(user => user.Role == UserRoles.Admin);
            var testSpecs = BuildTests();
            var scaleMap = new Dictionary<string, Scale>(StringComparer.OrdinalIgnoreCase);
            var metricMap = new Dictionary<string, Metric>(StringComparer.OrdinalIgnoreCase);
            var tests = CreateTests(testSpecs, admin.UserId, scaleMap, metricMap);

            db.Scales.AddRange(scaleMap.Values);
            db.Metrics.AddRange(metricMap.Values);
            db.Tests.AddRange(tests);
            await db.SaveChangesAsync();

            AddQuestionsAndLinks(db, testSpecs, tests, scaleMap, metricMap);
            await db.SaveChangesAsync();

            AddOpenAssignments(db, tests, groups);
            AddRegistrationTokens(db, groups);
            await db.SaveChangesAsync();

            AddPsychometricHistory(db, random, students, groups, tests, testSpecs, scaleMap, metricMap);
            AddTextFeedback(db, random, students, groups);
            await db.SaveChangesAsync();
            await SaveSeedVersion(db);
        }

        private static async Task<bool> ShouldReseed(AppDbContext db)
        {
            var currentSeedVersion = await db.SeedStates
                .AsNoTracking()
                .Where(state => state.Key == SeedVersionKey)
                .Select(state => state.Value)
                .FirstOrDefaultAsync();
            var groupNames = await db.Groups.AsNoTracking().Select(group => group.GroupName).ToListAsync();
            var hasWrongGroupSet = groupNames.Count != TargetGroups.Length || !TargetGroups.All(groupNames.Contains);
            var hasOldDemoGroups = groupNames.Any(group => group is "D375" or "G342" or "ADM" or "ПИ23" or "БИ21");
            var testsCount = await db.Tests.CountAsync();
            var studentsCount = await db.Users.CountAsync(user => user.Role == UserRoles.Student);
            var psychologistsCount = await db.Users.CountAsync(user => user.Role == UserRoles.Psychologist);
            var studentsWithoutFullName = await db.Users.CountAsync(user =>
                user.Role == UserRoles.Student && user.FullName == "");
            var snapshotsCount = await db.UserMetricSnapshots.CountAsync();
            var positiveScalesCount = await db.Scales.CountAsync(scale => scale.IsPositive);
            var positiveMetricsCount = await db.Metrics.CountAsync(metric => metric.IsPositive);
            var hasMojibake = await db.Tests.AnyAsync(test =>
                test.Title.Contains("РЎ") ||
                test.Title.Contains("Рџ") ||
                test.Title.Contains("Р‘") ||
                test.Title.Contains("СЊ"));

            return currentSeedVersion != SeedVersion
                || hasOldDemoGroups
                || hasWrongGroupSet
                || hasMojibake
                || testsCount < 5
                || studentsCount < 320
                || psychologistsCount < 1
                || studentsWithoutFullName > 0
                || snapshotsCount < 10_000
                || positiveScalesCount < 8
                || positiveMetricsCount < 3;
        }

        private static async Task SaveSeedVersion(AppDbContext db)
        {
            var marker = await db.SeedStates.FirstOrDefaultAsync(state => state.Key == SeedVersionKey);
            if (marker == null)
            {
                db.SeedStates.Add(new SeedState
                {
                    Key = SeedVersionKey,
                    Value = SeedVersion
                });
            }
            else
            {
                marker.Value = SeedVersion;
            }

            await db.SaveChangesAsync();
        }

        private static async Task ClearDemoData(AppDbContext db)
        {
            await db.Database.ExecuteSqlRawAsync("""
                TRUNCATE TABLE
                    text_feedback,
                    test_assignments,
                    user_metric_snapshots,
                    user_scale_results,
                    user_metrics,
                    test_scale_metric,
                    question_scale,
                    answers,
                    questions,
                    tests,
                    scales,
                    metrics,
                    tokens_for_groups,
                    users,
                    groups
                RESTART IDENTITY CASCADE;
                """);
        }

        private static List<Group> CreateGroups()
        {
            return TargetGroups.Select(groupName => new Group
            {
                GroupName = groupName,
                Specialization = GetSpecialization(groupName),
                StudentCount = 20
            }).ToList();
        }

        private static string GetSpecialization(string groupName)
        {
            return groupName[^1] switch
            {
                '3' => "Сетевое и системное администрирование",
                '5' => "Компьютерные системы и комплексы",
                '7' => "Разработка веб- и мультимедийных приложений",
                '9' => "Информационная безопасность",
                _ => "Информационные системы и программирование"
            };
        }

        private static List<User> CreateStaffUsers(Group group, PasswordService passwordService)
        {
            return
            [
                new()
                {
                    Login = "admin",
                    FullName = "Администратор системы",
                    Password = passwordService.HashPassword("12345"),
                    Role = UserRoles.Admin,
                    GroupId = group.GroupId,
                    CreatedAt = SeedNow.AddDays(-90)
                },
                new()
                {
                    Login = "psychologist",
                    FullName = "Психолог колледжа",
                    Password = passwordService.HashPassword("12345"),
                    Role = UserRoles.Psychologist,
                    GroupId = group.GroupId,
                    CreatedAt = SeedNow.AddDays(-90)
                },
                new()
                {
                    Login = "teacher",
                    FullName = "Социальный педагог",
                    Password = passwordService.HashPassword("12345"),
                    Role = UserRoles.SocialTeacher,
                    GroupId = group.GroupId,
                    CreatedAt = SeedNow.AddDays(-90)
                }
            ];
        }

        private static List<User> CreateStudents(List<Group> groups, PasswordService passwordService)
        {
            var users = new List<User>();

            foreach (var group in groups)
            {
                for (var index = 1; index <= 20; index++)
                {
                    users.Add(new User
                    {
                        Login = $"s{group.GroupName}{index:00}",
                        FullName = BuildStudentFullName(group.GroupName, index),
                        Password = passwordService.HashPassword("12345"),
                        Role = UserRoles.Student,
                        GroupId = group.GroupId,
                        CreatedAt = SeedNow.AddDays(-60 + index)
                    });
                }
            }

            return users;
        }

        private static string BuildStudentFullName(string groupName, int index)
        {
            var lastNames = new[]
            {
                "Иванов", "Петров", "Смирнов", "Кузнецов", "Соколов",
                "Попов", "Лебедев", "Козлов", "Новиков", "Морозов",
                "Волков", "Соловьев", "Васильев", "Зайцев", "Павлов",
                "Семенов", "Голубев", "Виноградов", "Богданов", "Федоров"
            };
            var firstNames = new[]
            {
                "Алексей", "Дмитрий", "Илья", "Кирилл", "Максим",
                "Никита", "Артем", "Егор", "Михаил", "Даниил",
                "Анна", "Мария", "Дарья", "Софья", "Алина",
                "Екатерина", "Полина", "Виктория", "Ксения", "Елизавета"
            };
            var patronymics = new[]
            {
                "Андреевич", "Сергеевич", "Дмитриевич", "Игоревич", "Алексеевич",
                "Николаевич", "Павлович", "Романович", "Владимирович", "Олегович",
                "Андреевна", "Сергеевна", "Дмитриевна", "Игоревна", "Алексеевна",
                "Николаевна", "Павловна", "Романовна", "Владимировна", "Олеговна"
            };

            var offset = groupName.Sum(ch => ch) % lastNames.Length;
            var arrayIndex = (offset + index - 1) % lastNames.Length;
            return $"{lastNames[arrayIndex]} {firstNames[arrayIndex]} {patronymics[arrayIndex]}";
        }

        private static List<TestSpec> BuildTests()
        {
            return
            [
                new(
                    "Самооценка психических состояний: учебная версия",
                    [
                        RiskScale("Тревожность", "Ожидание неудачи, внутреннее напряжение и частые переживания"),
                        RiskScale("Фрустрация", "Переживание препятствий, бессилия и раздражения из-за неуспеха"),
                        RiskScale("Агрессивность", "Склонность к резким реакциям и конфликтному ответу"),
                        RiskScale("Ригидность", "Трудность переключения и принятия изменений")
                    ],
                    [
                        MetricSpec.Risk("Тревожность", "Тревожность", "Групповой уровень тревожности"),
                        MetricSpec.Risk("Фрустрация", "Фрустрация", "Групповой уровень фрустрации"),
                        MetricSpec.Risk("Агрессивность", "Агрессивность", "Риск напряженных реакций"),
                        MetricSpec.Risk("Ригидность", "Ригидность", "Сложность адаптации к изменениям")
                    ]),
                new(
                    "Стресс и учебная нагрузка",
                    [
                        RiskScale("Учебный стресс", "Субъективная напряженность из-за занятий, проверочных и сроков"),
                        RiskScale("Перегрузка", "Ощущение избытка заданий, дедлайнов и требований"),
                        PositiveScale("Восстановление", "Наличие сна, отдыха и личного ресурса"),
                        PositiveScale("Индекс благополучия", "Общее ощущение устойчивого и нормального состояния")
                    ],
                    [
                        MetricSpec.Risk("Индекс стресса", "Учебный стресс", "Уровень стрессовой нагрузки"),
                        MetricSpec.Risk("Перегрузка", "Перегрузка", "Давление учебных задач"),
                        MetricSpec.Positive("Восстановление", "Восстановление", "Способность восстанавливаться"),
                        MetricSpec.Positive("Индекс благополучия", "Индекс благополучия", "Позитивное состояние группы")
                    ]),
                new(
                    "Социальный климат и поддержка в группе",
                    [
                        PositiveScale("Поддержка группы", "Ощущение помощи, принятия и готовности взаимодействовать"),
                        PositiveScale("Психологическая безопасность", "Возможность спокойно говорить о трудностях и ошибках"),
                        RiskScale("Конфликтность", "Частота споров, давления и напряженного общения"),
                        RiskScale("Социальная изоляция", "Ощущение одиночества внутри учебной группы")
                    ],
                    [
                        MetricSpec.Positive("Индекс благополучия", "Поддержка группы", "Групповая поддержка"),
                        MetricSpec.Positive("Эмоциональная стабильность", "Психологическая безопасность", "Безопасность взаимодействия"),
                        MetricSpec.Risk("Конфликтность", "Конфликтность", "Риск конфликтов"),
                        MetricSpec.Risk("Социальная изоляция", "Социальная изоляция", "Риск выпадения из группы")
                    ]),
                new(
                    "Вовлеченность и учебная мотивация",
                    [
                        PositiveScale("Учебная вовлеченность", "Включенность в занятия, задания и жизнь группы"),
                        PositiveScale("Ясность учебных задач", "Понимание целей, требований и ближайших шагов"),
                        RiskScale("Дефицит вовлеченности", "Слабое участие в общих делах и учебном ритме"),
                        RiskScale("Риск пропусков", "Тенденция избегать занятий и групповых активностей")
                    ],
                    [
                        MetricSpec.Positive("Эмоциональная стабильность", "Учебная вовлеченность", "Стабильная учебная включенность"),
                        MetricSpec.Positive("Индекс благополучия", "Ясность учебных задач", "Ясность учебного процесса"),
                        MetricSpec.Risk("Дефицит вовлеченности", "Дефицит вовлеченности", "Риск слабого участия"),
                        MetricSpec.Risk("Риск пропусков", "Риск пропусков", "Риск выпадения из учебного ритма")
                    ]),
                new(
                    "Эмоциональная стабильность и настроение",
                    [
                        PositiveScale("Настроение", "Общий эмоциональный фон"),
                        PositiveScale("Эмоциональная стабильность", "Способность сохранять ровное состояние"),
                        RiskScale("Усталость", "Накопленная утомляемость и нехватка сил"),
                        RiskScale("Раздражительность", "Склонность быстро раздражаться и реагировать резко")
                    ],
                    [
                        MetricSpec.Positive("Индекс благополучия", "Настроение", "Позитивный эмоциональный фон"),
                        MetricSpec.Positive("Эмоциональная стабильность", "Эмоциональная стабильность", "Устойчивость состояния"),
                        MetricSpec.Risk("Усталость", "Усталость", "Риск утомления"),
                        MetricSpec.Risk("Раздражительность", "Раздражительность", "Эмоциональное напряжение")
                    ])
            ];
        }

        private static ScaleSpec RiskScale(string name, string description) => new(name, description, true);

        private static ScaleSpec PositiveScale(string name, string description) => new(name, description, false);

        private static List<Test> CreateTests(
            List<TestSpec> specs,
            int adminUserId,
            Dictionary<string, Scale> scaleMap,
            Dictionary<string, Metric> metricMap)
        {
            var tests = new List<Test>();

            foreach (var spec in specs)
            {
                foreach (var scale in spec.Scales)
                {
                    scaleMap.TryAdd(scale.Name, new Scale
                    {
                        Name = scale.Name,
                        Description = scale.Description,
                        IsPositive = !scale.IsRisk
                    });
                }

                foreach (var metric in spec.Metrics)
                {
                    metricMap.TryAdd(metric.Name, new Metric
                    {
                        Name = metric.Name,
                        Description = metric.Description,
                        IsPositive = metric.IsPositive
                    });
                }

                tests.Add(new Test
                {
                    Title = spec.Title,
                    CreatedById = adminUserId,
                    CreatedAt = SeedNow.AddDays(-35)
                });
            }

            return tests;
        }

        private static void AddQuestionsAndLinks(
            AppDbContext db,
            List<TestSpec> specs,
            List<Test> tests,
            Dictionary<string, Scale> scaleMap,
            Dictionary<string, Metric> metricMap)
        {
            for (var testIndex = 0; testIndex < specs.Count; testIndex++)
            {
                var spec = specs[testIndex];
                var test = tests[testIndex];

                foreach (var scale in spec.Scales)
                {
                    foreach (var questionText in BuildQuestionTexts(scale))
                    {
                        var question = new Question
                        {
                            TestId = test.TestId,
                            Text = questionText
                        };

                        db.Questions.Add(question);
                        db.SaveChanges();

                        db.AnswerOptions.AddRange(CreateAnswerOptions(question.QuestionId));
                        db.QuestionScales.Add(new QuestionScale
                        {
                            QuestionId = question.QuestionId,
                            ScaleId = scaleMap[scale.Name].ScaleId,
                            Weight = 1.0
                        });
                    }
                }

                foreach (var metric in spec.Metrics)
                {
                    db.TestScaleMetrics.Add(new TestScaleMetric
                    {
                        TestId = test.TestId,
                        ScaleId = scaleMap[metric.ScaleName].ScaleId,
                        MetricId = metricMap[metric.Name].MetricId,
                        Weight = 1.0
                    });
                }
            }
        }

        private static List<string> BuildQuestionTexts(ScaleSpec scale)
        {
            return scale.Name switch
            {
                "Тревожность" =>
                [
                    "Перед важными занятиями или проверочными я заранее начинаю сильно переживать.",
                    "Даже небольшая учебная ошибка надолго выбивает меня из спокойного состояния.",
                    "Я часто думаю, что могу не справиться с учебными задачами группы."
                ],
                "Фрустрация" =>
                [
                    "Когда задание не получается, у меня быстро появляется ощущение бессилия.",
                    "Из-за неудач в учебе мне трудно продолжать работу в обычном темпе.",
                    "Если планы группы срываются, я долго раздражаюсь и теряю мотивацию."
                ],
                "Агрессивность" =>
                [
                    "В спорной ситуации я могу ответить резче, чем хотел(а).",
                    "Когда меня критикуют по учебе, мне трудно сохранять спокойный тон.",
                    "Напряжение в группе иногда заставляет меня вступать в конфликт."
                ],
                "Ригидность" =>
                [
                    "Мне сложно быстро перестроиться, если меняются требования к заданию.",
                    "Я тяжело принимаю новые правила или другой порядок работы в группе.",
                    "Неожиданные изменения расписания работы над задачами сильно мешают мне."
                ],
                "Учебный стресс" =>
                [
                    "В последнюю неделю учебные задачи держали меня в постоянном напряжении.",
                    "Я часто чувствовал(а), что не успеваю за темпом занятий.",
                    "Мысли о контрольных, дедлайнах или долгах мешали мне отдыхать."
                ],
                "Перегрузка" =>
                [
                    "Количество заданий казалось мне больше, чем реально можно выполнить спокойно.",
                    "Я часто выбирал(а), какое задание сделать, потому что на все не хватало сил.",
                    "Учебная нагрузка оставляла мало времени на восстановление."
                ],
                "Восстановление" =>
                [
                    "После учебного дня у меня оставались силы на отдых и обычные дела.",
                    "Сон и свободное время помогали мне восстановиться перед следующими занятиями.",
                    "Я успевал(а) переключаться с учебы на личные дела без сильного напряжения."
                ],
                "Индекс благополучия" =>
                [
                    "В целом я чувствовал(а), что мое состояние в норме.",
                    "Последняя неделя прошла для меня достаточно устойчиво и спокойно.",
                    "У меня было ощущение, что я контролирую учебную ситуацию."
                ],
                "Поддержка группы" =>
                [
                    "В группе есть люди, к которым можно обратиться за помощью.",
                    "Одногруппники обычно поддерживают друг друга в сложных учебных ситуациях.",
                    "Мне комфортно работать с группой над общими задачами."
                ],
                "Психологическая безопасность" =>
                [
                    "В группе можно спокойно сказать, что что-то непонятно.",
                    "Ошибки в учебе не становятся поводом для унижения или давления.",
                    "Я могу высказать свое мнение, не ожидая резкой реакции."
                ],
                "Конфликтность" =>
                [
                    "В группе часто возникают споры, которые мешают учебной работе.",
                    "Некоторые обсуждения быстро переходят в напряженное общение.",
                    "Разногласия между студентами заметно влияют на общий климат."
                ],
                "Социальная изоляция" =>
                [
                    "Мне бывает одиноко, даже когда я нахожусь рядом с группой.",
                    "Я редко чувствую себя частью общих учебных и групповых дел.",
                    "Мне сложно обратиться к одногруппникам, когда нужна поддержка."
                ],
                "Учебная вовлеченность" =>
                [
                    "Я регулярно включаюсь в работу на занятиях и в учебных заданиях.",
                    "Мне важно понимать общий результат, к которому идет группа.",
                    "Я стараюсь участвовать в делах группы, а не оставаться в стороне."
                ],
                "Ясность учебных задач" =>
                [
                    "Мне обычно понятно, что нужно сделать по основным учебным задачам.",
                    "Требования преподавателей и сроки работ выглядят для меня достаточно ясными.",
                    "Я понимаю, какие шаги помогут закрыть учебные долги или сложные темы."
                ],
                "Дефицит вовлеченности" =>
                [
                    "Мне трудно заставить себя участвовать в учебной работе группы.",
                    "Я часто откладываю задания, даже когда понимаю их важность.",
                    "Общие дела группы проходят мимо меня."
                ],
                "Риск пропусков" =>
                [
                    "У меня появлялось желание пропустить занятия без серьезной причины.",
                    "Мне сложно удерживать регулярный учебный ритм.",
                    "Когда накапливается усталость, я начинаю избегать занятий или активностей."
                ],
                "Настроение" =>
                [
                    "В последние дни у меня чаще было ровное или хорошее настроение.",
                    "Мне удавалось замечать хорошие моменты в учебном дне.",
                    "Общение в группе чаще поддерживало меня, чем утомляло."
                ],
                "Эмоциональная стабильность" =>
                [
                    "Даже при нагрузке мне удавалось сохранять устойчивое состояние.",
                    "Я быстро возвращался(лась) к нормальному настроению после неприятных ситуаций.",
                    "Мне было проще спокойно реагировать на учебные трудности."
                ],
                "Усталость" =>
                [
                    "Я часто чувствовал(а) физическую или эмоциональную усталость.",
                    "К концу учебного дня мне было трудно концентрироваться.",
                    "Даже после отдыха у меня оставалось ощущение утомления."
                ],
                "Раздражительность" =>
                [
                    "Мелкие ситуации в учебе раздражали меня сильнее обычного.",
                    "Мне было трудно спокойно реагировать на замечания или просьбы.",
                    "Я замечал(а), что быстрее устаю от общения в группе."
                ],
                _ when scale.IsRisk =>
                [
                    $"За последнюю неделю показатель «{scale.Name.ToLowerInvariant()}» заметно влиял на мое состояние.",
                    $"В учебной группе мне бывает трудно справляться с проявлениями по шкале «{scale.Name.ToLowerInvariant()}».",
                    $"Из-за учебных ситуаций у меня усиливается «{scale.Name.ToLowerInvariant()}»."
                ],
                _ =>
                [
                    $"За последнюю неделю показатель «{scale.Name.ToLowerInvariant()}» был у меня выражен достаточно хорошо.",
                    $"Обстановка в группе помогает поддерживать «{scale.Name.ToLowerInvariant()}».",
                    $"Даже при учебной нагрузке мне удается сохранять «{scale.Name.ToLowerInvariant()}»."
                ]
            };
        }

        private static List<AnswerOption> CreateAnswerOptions(int questionId)
        {
            return
            [
                new() { QuestionId = questionId, Text = "Совсем не согласен(на)", Value = 0m },
                new() { QuestionId = questionId, Text = "Скорее не согласен(на)", Value = 0.33m },
                new() { QuestionId = questionId, Text = "Скорее согласен(на)", Value = 0.66m },
                new() { QuestionId = questionId, Text = "Полностью согласен(на)", Value = 1m }
            ];
        }

        private static void AddOpenAssignments(AppDbContext db, List<Test> tests, List<Group> groups)
        {
            foreach (var test in tests)
            {
                foreach (var group in groups)
                {
                    db.TestAssignments.Add(new TestAssignment
                    {
                        TestId = test.TestId,
                        GroupId = group.GroupId,
                        OpensAt = SeedNow.AddDays(-28),
                        ClosesAt = SeedNow.AddDays(45),
                        CreatedAt = SeedNow.AddDays(-29)
                    });
                }
            }
        }

        private static void AddRegistrationTokens(AppDbContext db, List<Group> groups)
        {
            foreach (var group in groups)
            {
                db.Tokens.Add(new Token
                {
                    TokenId = $"KTK-{group.GroupName}-2026",
                    GroupId = group.GroupId,
                    NumberOfUses = 100
                });
            }
        }

        private static void AddPsychometricHistory(
            AppDbContext db,
            Random random,
            List<User> students,
            List<Group> groups,
            List<Test> tests,
            List<TestSpec> specs,
            Dictionary<string, Scale> scaleMap,
            Dictionary<string, Metric> metricMap)
        {
            var waveDates = new[]
            {
                SeedNow.AddDays(-21),
                SeedNow.AddDays(-14),
                SeedNow.AddDays(-7)
            };

            var latestMetrics = new Dictionary<(int UserId, int MetricId), decimal>();

            foreach (var student in students)
            {
                var group = groups.First(group => group.GroupId == student.GroupId);
                var profile = BuildStudentProfile(random, group.GroupName, student.UserId);

                for (var wave = 0; wave < waveDates.Length; wave++)
                {
                    for (var testIndex = 0; testIndex < tests.Count; testIndex++)
                    {
                        var test = tests[testIndex];
                        var spec = specs[testIndex];
                        var createdAt = waveDates[wave].AddMinutes(random.Next(0, 720));
                        var scaleValues = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

                        foreach (var scale in spec.Scales)
                        {
                            var normalized = CalculateScaleValue(random, scale, profile, wave);
                            scaleValues[scale.Name] = normalized;

                            db.UserScaleResults.Add(new UserScaleResult
                            {
                                UserId = student.UserId,
                                ScaleId = scaleMap[scale.Name].ScaleId,
                                RawScore = Math.Round(normalized * 12m, 2),
                                NormalizedScore = normalized,
                                SourceTestId = test.TestId,
                                CreatedAt = createdAt
                            });
                        }

                        foreach (var metric in spec.Metrics)
                        {
                            var metricId = metricMap[metric.Name].MetricId;
                            var value = scaleValues[metric.ScaleName] * 100m;

                            db.UserMetricSnapshots.Add(new UserMetricSnapshot
                            {
                                UserId = student.UserId,
                                MetricId = metricId,
                                Value = Math.Round(value, 1),
                                SourceTestId = test.TestId,
                                CreatedAt = createdAt
                            });

                            latestMetrics[(student.UserId, metricId)] = Math.Round(value, 1);
                        }
                    }
                }
            }

            foreach (var ((userId, metricId), value) in latestMetrics)
            {
                db.UserMetrics.Add(new UserMetric
                {
                    UserId = userId,
                    MetricId = metricId,
                    Value = value
                });
            }
        }

        private static StudentProfile BuildStudentProfile(Random random, string groupName, int userId)
        {
            var personalShift = (decimal)(random.NextDouble() * 0.20 - 0.10);
            var responseStyle = (HashUnit($"{groupName}:{userId}:style") - 0.5m) * 0.16m;

            return new StudentProfile(groupName, userId, personalShift, responseStyle);
        }

        private static decimal CalculateScaleValue(Random random, ScaleSpec scale, StudentProfile profile, int wave)
        {
            var groupScaleBase = 0.14m + HashUnit($"{profile.GroupName}:{scale.Name}:group-scale") * 0.72m;
            var studentScaleShift = (HashUnit($"{profile.UserId}:{scale.Name}:student-scale") - 0.5m) * 0.58m;
            var trend = (HashUnit($"{profile.GroupName}:{profile.UserId}:{scale.Name}:trend") - 0.5m) * 0.22m * wave;
            var waveImpulse = (HashUnit($"{profile.UserId}:{scale.Name}:wave:{wave}") - 0.5m) * 0.18m;
            var noise = (decimal)(random.NextDouble() * 0.14 - 0.07);

            var value = groupScaleBase
                + studentScaleShift
                + profile.PersonalShift
                + profile.ResponseStyle
                + trend
                + waveImpulse
                + noise;

            return Math.Round(Clamp(value, 0.03m, 0.97m), 3);
        }

        private static decimal HashUnit(string value)
        {
            unchecked
            {
                var hash = 2166136261u;
                foreach (var ch in value)
                {
                    hash ^= ch;
                    hash *= 16777619u;
                }

                return (decimal)(hash % 10_000u) / 9_999m;
            }
        }

        private static void AddTextFeedback(AppDbContext db, Random random, List<User> students, List<Group> groups)
        {
            var feedbackByTopic = new (string Topic, decimal Sentiment, string[] Texts)[]
            {
                ("усталость,нагрузка", -50m, [
                    "Много дедлайнов подряд, группа устала и сложнее держать темп.",
                    "Не всегда хватает времени восстановиться после пар и заданий.",
                    "Нагрузка ощущается высокой, хочется понятнее распределять задания."
                ]),
                ("конфликт,поддержка", -35m, [
                    "Иногда в группе спорят резко, после этого сложнее работать вместе.",
                    "Не всем комфортно просить помощи, есть страх выглядеть слабым.",
                    "Хочется больше спокойного общения и поддержки внутри группы."
                ]),
                ("учеба,ясность", -15m, [
                    "По нескольким предметам не хватает ясности, что именно готовить.",
                    "Когда требования объясняют заранее, учиться намного спокойнее.",
                    "Есть темы, которые группе хотелось бы разобрать еще раз."
                ]),
                ("общее,улучшение", 35m, [
                    "В группе стало спокойнее, ребята чаще помогают друг другу.",
                    "Последняя неделя прошла нормально, задачи стали понятнее.",
                    "Есть ощущение, что группа постепенно собирается и работает лучше."
                ]),
                ("вовлеченность,группа", 15m, [
                    "Когда задания делают вместе, становится проще включаться.",
                    "Нужны небольшие общие цели, чтобы группа не распадалась на отдельных людей.",
                    "Общие учебные активности помогают почувствовать себя частью группы."
                ])
            };

            foreach (var group in groups)
            {
                var groupStudents = students.Where(student => student.GroupId == group.GroupId).ToList();

                for (var week = 0; week < 3; week++)
                {
                    for (var index = 0; index < 10; index++)
                    {
                        var student = groupStudents[(week * 10 + index) % groupStudents.Count];
                        var topic = feedbackByTopic[random.Next(feedbackByTopic.Length)];
                        var text = topic.Texts[random.Next(topic.Texts.Length)];

                        db.TextFeedback.Add(new TextFeedback
                        {
                            UserId = student.UserId,
                            GroupId = group.GroupId,
                            Text = text,
                            SentimentScore = Clamp(topic.Sentiment + random.Next(-10, 11), -100m, 100m),
                            Topics = topic.Topic,
                            CreatedAt = SeedNow.AddDays(-21 + week * 7).AddHours(random.Next(8, 20))
                        });
                    }
                }
            }
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

        private sealed record TestSpec(string Title, List<ScaleSpec> Scales, List<MetricSpec> Metrics);

        private sealed record ScaleSpec(string Name, string Description, bool IsRisk);

        private sealed record MetricSpec(string Name, string ScaleName, string Description, bool IsPositive)
        {
            public static MetricSpec Risk(string name, string scaleName, string description) =>
                new(name, scaleName, description, false);

            public static MetricSpec Positive(string name, string scaleName, string description) =>
                new(name, scaleName, description, true);
        }

        private sealed record StudentProfile(string GroupName, int UserId, decimal PersonalShift, decimal ResponseStyle);
    }
}
