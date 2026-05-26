using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using PsychometricTestDesigner.Frontend.Models;

namespace PsychometricTestDesigner.Frontend.Services;

public sealed class ApiClient
{
    private readonly HttpClient _http;
    private readonly AuthState _auth;
    private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiClient(HttpClient http, AuthState auth)
    {
        _http = http;
        _auth = auth;
    }

    public string? LastError { get; private set; }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var auth = await PostAsync<LoginRequest, AuthResponse>("api/auth/login", request, false);
        ApplyAuth(auth);
        return auth;
    }

    public async Task<AuthResponse?> RegisterStudentAsync(RegisterRequest request)
    {
        var auth = await PostAsync<RegisterRequest, AuthResponse>("api/auth/register", request, false);
        ApplyAuth(auth);
        return auth;
    }

    public async Task<AuthResponse?> RegisterStaffAsync(RegisterStaffRequest request)
    {
        var auth = await PostAsync<RegisterStaffRequest, AuthResponse>("api/auth/register-staff", request, false);
        ApplyAuth(auth);
        return auth;
    }

    public Task<List<GroupRiskSummary>> GetRiskSummariesAsync() =>
        GetListAsync<GroupRiskSummary>("api/groups/risk-summary");

    public Task<List<GroupDto>> GetGroupsAsync() =>
        GetListAsync<GroupDto>("api/groups");

    public Task<List<GroupMetric>> GetGroupMetricsAsync(int groupId) =>
        GetListAsync<GroupMetric>($"api/groups/{groupId}/metrics");

    public Task<List<GroupMetricHistoryPoint>> GetMetricHistoryAsync(int groupId) =>
        GetListAsync<GroupMetricHistoryPoint>($"api/groups/{groupId}/metric-history");

    public Task<List<GroupScaleDistribution>> GetScaleDistributionAsync(int groupId) =>
        GetListAsync<GroupScaleDistribution>($"api/groups/{groupId}/scale-distribution");

    public Task<List<TestSummary>> GetTestsAsync() =>
        GetListAsync<TestSummary>("api/tests");

    public Task<List<TestAssignment>> GetAvailableTestsAsync() =>
        GetListAsync<TestAssignment>("api/tests/available/me");

    public Task<List<TestAssignment>> GetTestAssignmentsAsync() =>
        GetListAsync<TestAssignment>("api/tests/assignments");

    public Task<TestAssignment?> CreateTestAssignmentAsync(CreateTestAssignment request) =>
        PostAsync<CreateTestAssignment, TestAssignment>("api/tests/assignments", request);

    public Task<FullTestResponse?> GetFullTestAsync(int testId) =>
        GetAsync<FullTestResponse>($"api/tests/{testId}/full");

    public Task<List<UserMetric>> GetMyMetricsAsync() =>
        GetListAsync<UserMetric>("api/users/me/metrics");

    public Task<List<UserScaleResult>> GetMyScaleResultsAsync() =>
        GetListAsync<UserScaleResult>("api/users/me/scale-results");

    public Task<ProcessTestResult?> SubmitCurrentUserTestAsync(SubmitCurrentUserTest request) =>
        PostAsync<SubmitCurrentUserTest, ProcessTestResult>("api/test-processing/submit/me", request);

    public Task<FeedbackResponse?> SubmitFeedbackAsync(string text) =>
        PostAsync<FeedbackSubmit, FeedbackResponse>("api/feedback/me", new FeedbackSubmit { Text = text });

    public Task<GroupFeedbackTrend?> GetFeedbackTrendAsync(int groupId) =>
        GetAsync<GroupFeedbackTrend>($"api/feedback/groups/{groupId}/trends");

    public Task<GroupFeedbackTrend?> GetAllFeedbackTrendAsync() =>
        GetAsync<GroupFeedbackTrend>("api/feedback/trends/all");

    public Task<List<StudentResultSummary>> GetGroupStudentResultsAsync(int groupId) =>
        GetListAsync<StudentResultSummary>($"api/users/groups/{groupId}/results");

    public Task<AdminAnalytics?> GetAdminAnalyticsAsync() =>
        GetAsync<AdminAnalytics>("api/analytics/admin");

    public Task<List<GroupFillRate>> GetFillRatesAsync() =>
        GetListAsync<GroupFillRate>("api/analytics/fill-rates");

    public Task<List<TriggerAlert>> GetAlertsAsync() =>
        GetListAsync<TriggerAlert>("api/analytics/alerts");

    public Task<List<TokenResponse>> GetTokensAsync() =>
        GetListAsync<TokenResponse>("api/tokens");

    public Task<GroupDto?> CreateGroupAsync(GroupDto request) =>
        PostAsync<GroupDto, GroupDto>("api/groups", request);

    public Task<TokenResponse?> CreateTokenAsync(CreateTokenRequest request) =>
        PostAsync<CreateTokenRequest, TokenResponse>("api/tokens", request);

    public Task<FullTestResponse?> CreateDemoTestAsync(int creatorId) =>
        PostAsync<CreateFullTest, FullTestResponse>("api/tests/full", DemoTestFactory.Create(creatorId));

    public Task<FullTestResponse?> CreateFullTestAsync(CreateFullTest request) =>
        PostAsync<CreateFullTest, FullTestResponse>("api/tests/full", request);

    private async Task<List<T>> GetListAsync<T>(string url)
    {
        var result = await GetAsync<List<T>>(url);
        return result ?? new List<T>();
    }

    private async Task<T?> GetAsync<T>(string url)
    {
        LastError = null;
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        ApplyBearer(request);
        return await SendAsync<T>(request);
    }

    private async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest body, bool withToken = true)
    {
        LastError = null;
        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(body, options: _json)
        };

        if (withToken)
        {
            ApplyBearer(request);
        }

        return await SendAsync<TResponse>(request);
    }

    private async Task<T?> SendAsync<T>(HttpRequestMessage request)
    {
        try
        {
            using var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                LastError = await ReadErrorAsync(response);
                return default;
            }

            return await response.Content.ReadFromJsonAsync<T>(_json);
        }
        catch (Exception ex)
        {
            LastError = RuText.Fix(ex.Message);
            return default;
        }
    }

    private void ApplyBearer(HttpRequestMessage request)
    {
        if (_auth.IsAuthenticated)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _auth.Token);
        }
    }

    private void ApplyAuth(AuthResponse? auth)
    {
        if (auth != null)
        {
            _auth.Set(auth.UserId, auth.GroupId, auth.GroupName, auth.FullName, auth.Role, auth.Token);
        }
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(content))
        {
            return $"API вернул статус {(int)response.StatusCode}";
        }

        try
        {
            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.TryGetProperty("message", out var message))
            {
                return RuText.Fix(message.GetString());
            }
        }
        catch (JsonException)
        {
        }

        return RuText.Fix(content);
    }

    private static class DemoTestFactory
    {
        public static CreateFullTest Create(int creatorId) => new()
        {
            Title = "Опрос климата и вовлечённости учебной группы",
            CreatedById = creatorId,
            Scales =
            [
                new() { Name = "Стресс", Description = "Субъективная напряжённость и перегрузка" },
                new() { Name = "Благополучие", Description = "Эмоциональная устойчивость и спокойствие" },
                new() { Name = "Изоляция", Description = "Ощущение одиночества внутри группы" },
                new() { Name = "Дефицит вовлечённости", Description = "Слабое участие в жизни группы и учёбе" }
            ],
            Metrics =
            [
                new() { Name = "Индекс стресса", Description = "Групповой уровень напряжения" },
                new() { Name = "Индекс благополучия", Description = "Позитивное состояние группы" },
                new() { Name = "Социальная изоляция", Description = "Риск выпадения из группового взаимодействия" },
                new() { Name = "Дефицит вовлечённости", Description = "Риск слабого участия в группе" }
            ],
            Questions =
            [
                ScaleQuestion("Насколько сильным был ваш стресс за последние дни?", "Стресс",
                    ("Низкий", 0m), ("Умеренный", 0.33m), ("Средний", 0.66m), ("Высокий", 1m)),
                ScaleQuestion("Насколько часто вы чувствуете перегрузку из-за учёбы?", "Стресс",
                    ("Почти никогда", 0m), ("Иногда", 0.33m), ("Часто", 0.66m), ("Почти постоянно", 1m)),
                ScaleQuestion("Насколько спокойно и уверенно вы чувствуете себя в группе?", "Благополучие",
                    ("Совсем не спокойно", 0m), ("Скорее не спокойно", 0.33m), ("Скорее спокойно", 0.66m), ("Полностью спокойно", 1m)),
                ScaleQuestion("Насколько вы ощущаете поддержку со стороны одногруппников?", "Благополучие",
                    ("Не ощущаю", 0m), ("Редко", 0.33m), ("Часто", 0.66m), ("Постоянно", 1m)),
                ScaleQuestion("Насколько часто вы чувствуете себя отдельно от группы?", "Изоляция",
                    ("Почти никогда", 0m), ("Иногда", 0.33m), ("Часто", 0.66m), ("Почти всегда", 1m)),
                ScaleQuestion("Насколько сложно вам обращаться к группе за помощью?", "Изоляция",
                    ("Легко", 0m), ("Скорее легко", 0.33m), ("Скорее сложно", 0.66m), ("Очень сложно", 1m)),
                ScaleQuestion("Насколько вы сейчас включены в учебные и групповые дела?", "Дефицит вовлечённости",
                    ("Активно включён", 0m), ("Скорее включён", 0.33m), ("Скорее не включён", 0.66m), ("Почти не участвую", 1m)),
                ScaleQuestion("Насколько часто вам хочется пропустить общие активности группы?", "Дефицит вовлечённости",
                    ("Почти никогда", 0m), ("Иногда", 0.33m), ("Часто", 0.66m), ("Почти всегда", 1m))
            ],
            MetricLinks =
            [
                new() { ScaleName = "Стресс", MetricName = "Индекс стресса", Weight = 1 },
                new() { ScaleName = "Благополучие", MetricName = "Индекс благополучия", Weight = 1 },
                new() { ScaleName = "Изоляция", MetricName = "Социальная изоляция", Weight = 1 },
                new() { ScaleName = "Дефицит вовлечённости", MetricName = "Дефицит вовлечённости", Weight = 1 }
            ]
        };

        private static CreateFullQuestion ScaleQuestion(
            string text,
            string scaleName,
            params (string Text, decimal Value)[] answers) => new()
            {
                Text = text,
                ScaleLinks = [new() { ScaleName = scaleName, Weight = 1 }],
                AnswerOptions = answers
                    .Select(answer => new CreateAnswerOption { Text = answer.Text, Value = answer.Value })
                    .ToList()
            };
    }
}
