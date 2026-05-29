using Microsoft.EntityFrameworkCore;
using Psychometric_Test_Designer.Data;
using Psychometric_Test_Designer.DTOs;
using Psychometric_Test_Designer.Models;

namespace Psychometric_Test_Designer.Services
{
    public class TestService
    {
        private readonly AppDbContext _db;

        public TestService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<Test>> GetAllTests()
        {
            var tests = await _db.Tests.Select(t => new Test
            {
                TestId = t.TestId,
                Title = t.Title,
                CreatedById = t.CreatedById,
                CreatedAt = t.CreatedAt
            }).ToListAsync();

            return tests;
        }

        public async Task<List<TestAssignmentDto>> GetAssignments()
        {
            var now = DateTime.UtcNow;

            return await _db.TestAssignments
                .AsNoTracking()
                .OrderByDescending(a => a.OpensAt)
                .Select(a => new TestAssignmentDto
                {
                    AssignmentId = a.AssignmentId,
                    TestId = a.TestId,
                    TestTitle = a.Test.Title,
                    GroupId = a.GroupId,
                    GroupName = a.Group.GroupName,
                    OpensAt = a.OpensAt,
                    ClosesAt = a.ClosesAt,
                    IsActive = a.OpensAt <= now && a.ClosesAt >= now
                })
                .ToListAsync();
        }

        public async Task<List<ScaleResponseDto>> GetScales()
        {
            return await _db.Scales
                .AsNoTracking()
                .OrderBy(s => s.Name)
                .Select(s => new ScaleResponseDto
                {
                    ScaleId = s.ScaleId,
                    Name = s.Name,
                    Description = s.Description,
                    IsPositive = s.IsPositive
                })
                .ToListAsync();
        }

        public async Task<List<MetricResponseDto>> GetMetrics()
        {
            return await _db.Metrics
                .AsNoTracking()
                .OrderBy(m => m.Name)
                .Select(m => new MetricResponseDto
                {
                    MetricId = m.MetricId,
                    Name = m.Name,
                    Description = m.Description,
                    IsPositive = m.IsPositive
                })
                .ToListAsync();
        }

        public async Task<TestAssignmentDto> CreateAssignment(CreateTestAssignmentDto dto)
        {
            if (dto.TestId <= 0)
            {
                throw new Exception("Выберите тест");
            }

            if (dto.GroupId <= 0)
            {
                throw new Exception("Выберите группу");
            }

            var opensAt = ToUtc(dto.OpensAt);
            var closesAt = ToUtc(dto.ClosesAt);

            if (closesAt <= opensAt)
            {
                throw new Exception("Дата закрытия должна быть позже даты открытия");
            }

            var testExists = await _db.Tests.AnyAsync(t => t.TestId == dto.TestId);
            if (!testExists)
            {
                throw new Exception("Тест не найден");
            }

            var groupExists = await _db.Groups.AnyAsync(g => g.GroupId == dto.GroupId);
            if (!groupExists)
            {
                throw new Exception("Группа не найдена");
            }

            var assignment = new TestAssignment
            {
                TestId = dto.TestId,
                GroupId = dto.GroupId,
                OpensAt = opensAt,
                ClosesAt = closesAt,
                CreatedAt = DateTime.UtcNow
            };

            _db.TestAssignments.Add(assignment);
            await _db.SaveChangesAsync();

            return (await GetAssignments()).First(a => a.AssignmentId == assignment.AssignmentId);
        }

        public async Task<List<TestAssignmentDto>> CreateAssignments(CreateTestAssignmentsDto dto)
        {
            if (dto.TestId <= 0)
            {
                throw new Exception("Выберите тест");
            }

            var groupIds = dto.GroupIds
                .Where(groupId => groupId > 0)
                .Distinct()
                .ToList();

            if (groupIds.Count == 0)
            {
                throw new Exception("Выберите хотя бы одну группу");
            }

            var opensAt = ToUtc(dto.OpensAt);
            var closesAt = ToUtc(dto.ClosesAt);

            if (closesAt <= opensAt)
            {
                throw new Exception("Дата закрытия должна быть позже даты открытия");
            }

            var testExists = await _db.Tests.AnyAsync(t => t.TestId == dto.TestId);
            if (!testExists)
            {
                throw new Exception("Тест не найден");
            }

            var existingGroupIds = await _db.Groups
                .Where(g => groupIds.Contains(g.GroupId))
                .Select(g => g.GroupId)
                .ToListAsync();

            var missingGroups = groupIds.Except(existingGroupIds).ToList();
            if (missingGroups.Count > 0)
            {
                throw new Exception("Одна или несколько выбранных групп не найдены");
            }

            var assignments = groupIds.Select(groupId => new TestAssignment
            {
                TestId = dto.TestId,
                GroupId = groupId,
                OpensAt = opensAt,
                ClosesAt = closesAt,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            _db.TestAssignments.AddRange(assignments);
            await _db.SaveChangesAsync();

            var assignmentIds = assignments.Select(a => a.AssignmentId).ToList();
            return (await GetAssignments())
                .Where(a => assignmentIds.Contains(a.AssignmentId))
                .OrderBy(a => a.GroupName)
                .ToList();
        }

        public async Task<List<TestAssignmentDto>> GetAvailableTestsForUser(int userId)
        {
            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                throw new Exception("Пользователь не найден");
            }

            var now = DateTime.UtcNow;

            var assignments = await _db.TestAssignments
                .AsNoTracking()
                .Where(a => a.GroupId == user.GroupId && a.OpensAt <= now && a.ClosesAt >= now)
                .OrderBy(a => a.ClosesAt)
                .Select(a => new TestAssignmentDto
                {
                    AssignmentId = a.AssignmentId,
                    TestId = a.TestId,
                    TestTitle = a.Test.Title,
                    GroupId = a.GroupId,
                    GroupName = a.Group.GroupName,
                    OpensAt = a.OpensAt,
                    ClosesAt = a.ClosesAt,
                    IsActive = true
                })
                .ToListAsync();

            var available = new List<TestAssignmentDto>();
            foreach (var assignment in assignments)
            {
                if (!await HasUserCompletedAssignment(userId, assignment.TestId, assignment.OpensAt, assignment.ClosesAt))
                {
                    available.Add(assignment);
                }
            }

            return available;
        }

        public async Task<bool> IsTestAvailableForUser(int userId, int testId)
        {
            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                return false;
            }

            var now = DateTime.UtcNow;
            var assignments = await _db.TestAssignments
                .AsNoTracking()
                .Where(a =>
                    a.TestId == testId
                    && a.GroupId == user.GroupId
                    && a.OpensAt <= now
                    && a.ClosesAt >= now)
                .Select(a => new { a.TestId, a.OpensAt, a.ClosesAt })
                .ToListAsync();

            foreach (var assignment in assignments)
            {
                if (!await HasUserCompletedAssignment(userId, assignment.TestId, assignment.OpensAt, assignment.ClosesAt))
                {
                    return true;
                }
            }

            return false;
        }

        public async Task<bool> IsAssignmentAvailableForUser(int userId, int assignmentId, int testId)
        {
            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                return false;
            }

            var now = DateTime.UtcNow;
            var assignment = await _db.TestAssignments
                .AsNoTracking()
                .Where(a =>
                    a.AssignmentId == assignmentId
                    && a.TestId == testId
                    && a.GroupId == user.GroupId
                    && a.OpensAt <= now
                    && a.ClosesAt >= now)
                .Select(a => new { a.TestId, a.OpensAt, a.ClosesAt })
                .FirstOrDefaultAsync();

            return assignment != null
                && !await HasUserCompletedAssignment(userId, assignment.TestId, assignment.OpensAt, assignment.ClosesAt);
        }

        public async Task<Test> GetTestById(int testId)
        {
            var test = await _db.Tests.FirstOrDefaultAsync(t => t.TestId == testId);
            if (test == null)
            {
                throw new Exception("Тест не найден");
            }
            return test;
        }

        public async Task<List<Test>> GetTestsByCreatorId(int creatorId)
        {
            var tests = await _db.Tests.Where(t => t.CreatedById == creatorId).ToListAsync();
            if (tests == null || tests.Count == 0)
            {
                throw new Exception("Тесты не найдены");
            }
            return tests;
        }

        public async Task<Test?> CreateTest(TestDto dto)
        {
            var test = new Test
            {
                Title = dto.Title,
                CreatedById = dto.CreatedById,
                CreatedAt = DateTime.UtcNow
            };

            _db.Tests.Add(test);
            int result = await _db.SaveChangesAsync();
            return result > 0 ? test : null;
        }

        public async Task<Test?> DeleteTest(int testId)
        {
            var test = await _db.Tests.FirstOrDefaultAsync(t => t.TestId == testId);
            if (test == null)
            {
                return null;
            }

            await using var transaction = await _db.Database.BeginTransactionAsync();

            var deleted = new Test
            {
                TestId = test.TestId,
                Title = test.Title,
                CreatedById = test.CreatedById,
                CreatedAt = test.CreatedAt
            };

            await RemoveTestStructure(testId, removeAssignments: true, removeResults: true);
            _db.Tests.Remove(test);
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return deleted;
        }

        public async Task<FullTestResponseDto> CreateFullTest(CreateFullTestDto dto)
        {
            ValidateFullTest(dto);

            await using var transaction = await _db.Database.BeginTransactionAsync();

            var creatorExists = await _db.Users.AnyAsync(u => u.UserId == dto.CreatedById);
            if (!creatorExists)
            {
                throw new Exception("Автор теста не найден");
            }

            var scaleMap = new Dictionary<string, Scale>(StringComparer.OrdinalIgnoreCase);
            foreach (var scaleDto in dto.Scales)
            {
                var scale = await GetOrCreateScale(scaleDto);
                scaleMap[NormalizeName(scale.Name)] = scale;
            }

            var metricMap = new Dictionary<string, Metric>(StringComparer.OrdinalIgnoreCase);
            foreach (var metricDto in dto.Metrics)
            {
                var metric = await GetOrCreateMetric(metricDto);
                metricMap[NormalizeName(metric.Name)] = metric;
            }

            var test = new Test
            {
                Title = dto.Title.Trim(),
                CreatedById = dto.CreatedById,
                CreatedAt = DateTime.UtcNow
            };

            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            foreach (var questionDto in dto.Questions)
            {
                var question = new Question
                {
                    TestId = test.TestId,
                    Text = questionDto.Text.Trim()
                };

                _db.Questions.Add(question);
                await _db.SaveChangesAsync();

                foreach (var answerDto in questionDto.AnswerOptions)
                {
                    _db.AnswerOptions.Add(new AnswerOption
                    {
                        QuestionId = question.QuestionId,
                        Text = answerDto.Text.Trim(),
                        Value = answerDto.Value
                    });
                }

                var questionScaleKeys = new HashSet<int>();
                foreach (var linkDto in questionDto.ScaleLinks)
                {
                    var scale = scaleMap[NormalizeName(linkDto.ScaleName)];
                    if (!questionScaleKeys.Add(scale.ScaleId))
                    {
                        throw new Exception($"Шкала '{scale.Name}' уже привязана к вопросу '{question.Text}'");
                    }

                    _db.QuestionScales.Add(new QuestionScale
                    {
                        QuestionId = question.QuestionId,
                        ScaleId = scale.ScaleId,
                        Weight = linkDto.Weight
                    });
                }
            }

            var metricLinkKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var linkDto in dto.MetricLinks)
            {
                var scale = scaleMap[NormalizeName(linkDto.ScaleName)];
                var metric = metricMap[NormalizeName(linkDto.MetricName)];
                var key = $"{scale.ScaleId}:{metric.MetricId}";

                if (!metricLinkKeys.Add(key))
                {
                        throw new Exception($"Связь шкалы '{scale.Name}' и показателя '{metric.Name}' уже задана");
                }

                _db.TestScaleMetrics.Add(new TestScaleMetric
                {
                    TestId = test.TestId,
                    ScaleId = scale.ScaleId,
                    MetricId = metric.MetricId,
                    Weight = linkDto.Weight
                });
            }

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return await GetFullTestById(test.TestId);
        }

        public async Task<FullTestResponseDto> UpdateFullTest(int testId, CreateFullTestDto dto)
        {
            ValidateFullTest(dto);

            await using var transaction = await _db.Database.BeginTransactionAsync();

            var test = await _db.Tests.FirstOrDefaultAsync(t => t.TestId == testId);
            if (test == null)
            {
                throw new Exception("Тест не найден");
            }

            var scaleMap = new Dictionary<string, Scale>(StringComparer.OrdinalIgnoreCase);
            foreach (var scaleDto in dto.Scales)
            {
                var scale = await GetOrCreateScale(scaleDto);
                scaleMap[NormalizeName(scale.Name)] = scale;
            }

            var metricMap = new Dictionary<string, Metric>(StringComparer.OrdinalIgnoreCase);
            foreach (var metricDto in dto.Metrics)
            {
                var metric = await GetOrCreateMetric(metricDto);
                metricMap[NormalizeName(metric.Name)] = metric;
            }

            await RemoveTestStructure(testId, removeAssignments: false, removeResults: false);

            test.Title = dto.Title.Trim();
            await _db.SaveChangesAsync();

            await AddFullTestStructure(test.TestId, dto, scaleMap, metricMap);

            await transaction.CommitAsync();
            return await GetFullTestById(test.TestId);
        }

        public async Task<FullTestResponseDto> GetFullTestById(int testId)
        {
            var test = await _db.Tests.AsNoTracking().FirstOrDefaultAsync(t => t.TestId == testId);
            if (test == null)
            {
                throw new Exception("Тест не найден");
            }

            var questions = await _db.Questions
                .AsNoTracking()
                .Where(q => q.TestId == testId)
                .OrderBy(q => q.QuestionId)
                .Select(q => new FullQuestionResponseDto
                {
                    QuestionId = q.QuestionId,
                    Text = q.Text,
                    AnswerOptions = _db.AnswerOptions
                        .Where(a => a.QuestionId == q.QuestionId)
                        .OrderBy(a => a.AnswerId)
                        .Select(a => new AnswerOptionResponseDto
                        {
                            AnswerId = a.AnswerId,
                            Text = a.Text,
                            Value = a.Value
                        })
                        .ToList(),
                    ScaleLinks = _db.QuestionScales
                        .Where(qs => qs.QuestionId == q.QuestionId)
                        .Select(qs => new QuestionScaleResponseDto
                        {
                            ScaleId = qs.ScaleId,
                            ScaleName = qs.Scale.Name,
                            ScaleIsPositive = qs.Scale.IsPositive,
                            Weight = qs.Weight
                        })
                        .ToList()
                })
                .ToListAsync();

            var metricLinks = await _db.TestScaleMetrics
                .AsNoTracking()
                .Where(tsm => tsm.TestId == testId)
                .Select(tsm => new TestScaleMetricResponseDto
                {
                    ScaleId = tsm.ScaleId,
                    ScaleName = tsm.Scale.Name,
                    ScaleIsPositive = tsm.Scale.IsPositive,
                    MetricId = tsm.MetricId,
                    MetricName = tsm.Metric.Name,
                    MetricIsPositive = tsm.Metric.IsPositive,
                    Weight = tsm.Weight
                })
                .ToListAsync();

            var scaleIds = questions
                .SelectMany(q => q.ScaleLinks.Select(sl => sl.ScaleId))
                .Concat(metricLinks.Select(ml => ml.ScaleId))
                .Distinct()
                .ToList();

            var metricIds = metricLinks.Select(ml => ml.MetricId).Distinct().ToList();

            var scales = await _db.Scales
                .AsNoTracking()
                .Where(s => scaleIds.Contains(s.ScaleId))
                .OrderBy(s => s.ScaleId)
                .Select(s => new ScaleResponseDto
                {
                    ScaleId = s.ScaleId,
                    Name = s.Name,
                    Description = s.Description,
                    IsPositive = s.IsPositive
                })
                .ToListAsync();

            var metrics = await _db.Metrics
                .AsNoTracking()
                .Where(m => metricIds.Contains(m.MetricId))
                .OrderBy(m => m.MetricId)
                .Select(m => new MetricResponseDto
                {
                    MetricId = m.MetricId,
                    Name = m.Name,
                    Description = m.Description,
                    IsPositive = m.IsPositive
                })
                .ToListAsync();

            return new FullTestResponseDto
            {
                TestId = test.TestId,
                Title = test.Title,
                CreatedById = test.CreatedById,
                CreatedAt = test.CreatedAt,
                Scales = scales,
                Metrics = metrics,
                Questions = questions,
                MetricLinks = metricLinks
            };
        }

        private async Task AddFullTestStructure(
            int testId,
            CreateFullTestDto dto,
            Dictionary<string, Scale> scaleMap,
            Dictionary<string, Metric> metricMap)
        {
            foreach (var questionDto in dto.Questions)
            {
                var question = new Question
                {
                    TestId = testId,
                    Text = questionDto.Text.Trim()
                };

                _db.Questions.Add(question);
                await _db.SaveChangesAsync();

                foreach (var answerDto in questionDto.AnswerOptions)
                {
                    _db.AnswerOptions.Add(new AnswerOption
                    {
                        QuestionId = question.QuestionId,
                        Text = answerDto.Text.Trim(),
                        Value = answerDto.Value
                    });
                }

                var questionScaleKeys = new HashSet<int>();
                foreach (var linkDto in questionDto.ScaleLinks)
                {
                    var scale = scaleMap[NormalizeName(linkDto.ScaleName)];
                    if (!questionScaleKeys.Add(scale.ScaleId))
                    {
                        throw new Exception($"Шкала '{scale.Name}' уже привязана к вопросу '{question.Text}'");
                    }

                    _db.QuestionScales.Add(new QuestionScale
                    {
                        QuestionId = question.QuestionId,
                        ScaleId = scale.ScaleId,
                        Weight = linkDto.Weight
                    });
                }
            }

            var metricLinkKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var linkDto in dto.MetricLinks)
            {
                var scale = scaleMap[NormalizeName(linkDto.ScaleName)];
                var metric = metricMap[NormalizeName(linkDto.MetricName)];
                var key = $"{scale.ScaleId}:{metric.MetricId}";

                if (!metricLinkKeys.Add(key))
                {
                        throw new Exception($"Связь шкалы '{scale.Name}' и показателя '{metric.Name}' уже задана");
                }

                _db.TestScaleMetrics.Add(new TestScaleMetric
                {
                    TestId = testId,
                    ScaleId = scale.ScaleId,
                    MetricId = metric.MetricId,
                    Weight = linkDto.Weight
                });
            }

            await _db.SaveChangesAsync();
        }

        private async Task RemoveTestStructure(int testId, bool removeAssignments, bool removeResults)
        {
            var questionIds = await _db.Questions
                .Where(q => q.TestId == testId)
                .Select(q => q.QuestionId)
                .ToListAsync();

            if (questionIds.Count > 0)
            {
                await _db.AnswerOptions
                    .Where(a => questionIds.Contains(a.QuestionId))
                    .ExecuteDeleteAsync();

                await _db.QuestionScales
                    .Where(qs => questionIds.Contains(qs.QuestionId))
                    .ExecuteDeleteAsync();
            }

            await _db.TestScaleMetrics
                .Where(tsm => tsm.TestId == testId)
                .ExecuteDeleteAsync();

            if (removeAssignments)
            {
                await _db.TestAssignments
                    .Where(a => a.TestId == testId)
                    .ExecuteDeleteAsync();
            }

            if (removeResults)
            {
                await _db.UserMetricSnapshots
                    .Where(snapshot => snapshot.SourceTestId == testId)
                    .ExecuteDeleteAsync();

                await _db.UserScaleResults
                    .Where(result => result.SourceTestId == testId)
                    .ExecuteDeleteAsync();
            }

            if (questionIds.Count > 0)
            {
                await _db.Questions
                    .Where(q => q.TestId == testId)
                    .ExecuteDeleteAsync();
            }
        }

        private async Task<Scale> GetOrCreateScale(ScaleDefinitionDto dto)
        {
            var name = NormalizeName(dto.Name);
            var isPositive = dto.IsPositive || LooksPositive(name);
            var scale = await _db.Scales.FirstOrDefaultAsync(s => s.Name == name);
            if (scale != null)
            {
                if (!string.IsNullOrWhiteSpace(dto.Description))
                {
                    scale.Description = dto.Description.Trim();
                }

                scale.IsPositive = isPositive;
                return scale;
            }

            scale = new Scale
            {
                Name = name,
                Description = dto.Description?.Trim() ?? string.Empty,
                IsPositive = isPositive
            };

            _db.Scales.Add(scale);
            await _db.SaveChangesAsync();

            return scale;
        }

        private async Task<Metric> GetOrCreateMetric(MetricDefinitionDto dto)
        {
            var name = NormalizeName(dto.Name);
            var isPositive = dto.IsPositive || LooksPositive(name);
            var metric = await _db.Metrics.FirstOrDefaultAsync(m => m.Name == name);
            if (metric != null)
            {
                if (!string.IsNullOrWhiteSpace(dto.Description))
                {
                    metric.Description = dto.Description.Trim();
                }

                metric.IsPositive = isPositive;
                return metric;
            }

            metric = new Metric
            {
                Name = name,
                Description = dto.Description?.Trim() ?? string.Empty,
                IsPositive = isPositive
            };

            _db.Metrics.Add(metric);
            await _db.SaveChangesAsync();

            return metric;
        }

        private static void ValidateFullTest(CreateFullTestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                throw new Exception("Название теста обязательно");
            }

            if (dto.CreatedById <= 0)
            {
                throw new Exception("Некорректный автор теста");
            }

            if (dto.Scales.Count == 0)
            {
                throw new Exception("Добавьте хотя бы одну шкалу");
            }

            if (dto.Metrics.Count == 0)
            {
                throw new Exception("Добавьте хотя бы один показатель мониторинга");
            }

            if (dto.Questions.Count == 0)
            {
                throw new Exception("Добавьте хотя бы один вопрос");
            }

            if (dto.MetricLinks.Count == 0)
            {
                throw new Exception("Настройте связи шкал с показателями мониторинга");
            }

            var scaleNames = dto.Scales
                .Select(s => NormalizeName(s.Name))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var metricNames = dto.Metrics
                .Select(m => NormalizeName(m.Name))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (scaleNames.Count != dto.Scales.Count)
            {
                throw new Exception("Названия шкал не должны повторяться");
            }

            if (metricNames.Count != dto.Metrics.Count)
            {
                throw new Exception("Названия показателей мониторинга не должны повторяться");
            }

            foreach (var question in dto.Questions)
            {
                if (string.IsNullOrWhiteSpace(question.Text))
                {
                    throw new Exception("Текст вопроса обязателен");
                }

                if (question.AnswerOptions.Count == 0)
                {
                    throw new Exception($"Добавьте варианты ответа для вопроса '{question.Text}'");
                }

                if (question.ScaleLinks.Count == 0)
                {
                    throw new Exception($"Привяжите вопрос '{question.Text}' хотя бы к одной шкале");
                }

                foreach (var answer in question.AnswerOptions)
                {
                    if (string.IsNullOrWhiteSpace(answer.Text))
                    {
                        throw new Exception($"Пустой вариант ответа в вопросе '{question.Text}'");
                    }
                }

                foreach (var link in question.ScaleLinks)
                {
                    if (!scaleNames.Contains(NormalizeName(link.ScaleName)))
                    {
                        throw new Exception($"Шкала '{link.ScaleName}' не объявлена");
                    }
                }
            }

            foreach (var link in dto.MetricLinks)
            {
                if (!scaleNames.Contains(NormalizeName(link.ScaleName)))
                {
                    throw new Exception($"Шкала '{link.ScaleName}' не объявлена");
                }

                if (!metricNames.Contains(NormalizeName(link.MetricName)))
                {
                    throw new Exception($"Показатель '{link.MetricName}' не объявлен");
                }

                if (link.Weight <= 0 || link.Weight > 1)
                {
                    throw new Exception("Вес связи шкалы с показателем должен быть больше 0 и не больше 1");
                }
            }

            var duplicateMetricLink = dto.MetricLinks
                .GroupBy(link => $"{NormalizeName(link.ScaleName)}::{NormalizeName(link.MetricName)}", StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(group => group.Count() > 1);
            if (duplicateMetricLink != null)
            {
                throw new Exception("Одна и та же шкала не должна быть дважды связана с одним показателем");
            }

            var unlinkedScale = dto.Scales.FirstOrDefault(scale =>
                !dto.MetricLinks.Any(link => string.Equals(
                    NormalizeName(link.ScaleName),
                    NormalizeName(scale.Name),
                    StringComparison.OrdinalIgnoreCase)));
            if (unlinkedScale != null)
            {
                throw new Exception($"Шкала '{unlinkedScale.Name}' должна обновлять хотя бы один показатель мониторинга");
            }

            var unlinkedMetric = dto.Metrics.FirstOrDefault(metric =>
                !dto.MetricLinks.Any(link => string.Equals(
                    NormalizeName(link.MetricName),
                    NormalizeName(metric.Name),
                    StringComparison.OrdinalIgnoreCase)));
            if (unlinkedMetric != null)
            {
                throw new Exception($"Показатель '{unlinkedMetric.Name}' должен быть связан хотя бы с одной шкалой");
            }

            var metricWeightGroups = dto.MetricLinks.GroupBy(link => NormalizeName(link.MetricName));
            foreach (var group in metricWeightGroups)
            {
                var sum = group.Sum(link => link.Weight);
                if (Math.Abs(sum - 1) > 0.0001)
                {
                    throw new Exception($"Сумма весов шкал для показателя '{group.Key}' должна быть равна 1");
                }
            }
        }

        private static string NormalizeName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new Exception("Название не может быть пустым");
            }

            return value.Trim();
        }

        private static bool LooksPositive(string name)
        {
            var normalized = name.ToLowerInvariant();
            return normalized.Contains("благополуч")
                || normalized.Contains("восстанов")
                || normalized.Contains("поддерж")
                || normalized.Contains("безопас")
                || normalized.Contains("вовлеч")
                || normalized.Contains("ясность")
                || normalized.Contains("настро")
                || normalized.Contains("стабил")
                || normalized.Contains("wellbeing")
                || normalized.Contains("mood")
                || normalized.Contains("stability");
        }

        private async Task<bool> HasUserCompletedAssignment(int userId, int testId, DateTime opensAt, DateTime closesAt)
        {
            return await _db.UserScaleResults
                .AsNoTracking()
                .AnyAsync(result =>
                    result.UserId == userId
                    && result.SourceTestId == testId
                    && result.CreatedAt >= opensAt
                    && result.CreatedAt <= closesAt);
        }

        private static DateTime ToUtc(DateTime value)
        {
            return value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => TimeZoneInfo.ConvertTimeToUtc(value, GetAppTimeZone())
            };
        }

        private static TimeZoneInfo GetAppTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Yekaterinburg");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Ekaterinburg Standard Time");
            }
        }
    }
}
