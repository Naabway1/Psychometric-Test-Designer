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

            _db.Tests.Remove(test);
            int result = await _db.SaveChangesAsync();
            return result > 0 ? test : null;
        }
    }
}