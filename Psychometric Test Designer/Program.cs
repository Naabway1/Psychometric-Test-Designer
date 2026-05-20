using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Psychometric_Test_Designer.Core;
using Psychometric_Test_Designer.Data;
using Psychometric_Test_Designer.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddScoped<PasswordService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<TokenGenerator>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<GroupService>();
builder.Services.AddScoped<TestService>();
builder.Services.AddScoped<TestProcessingService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<FeedbackService>();
builder.Services.AddScoped<AnalyticsService>();
builder.Services.AddScoped<NotificationService>();

var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "PsychometricTestDesigner";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "PsychometricTestDesignerClient";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("StaffOnly", policy =>
        policy.RequireRole(UserRoles.Admin, UserRoles.SocialTeacher));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();

await DatabaseBootstrapper.EnsureSchemaAsync(app.Services);
await DatabaseSeeder.SeedAsync(app.Services);

app.UseDeveloperExceptionPage();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var useHttpsRedirection = builder.Configuration.GetValue<bool?>("UseHttpsRedirection") ?? true;
if (useHttpsRedirection)
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
