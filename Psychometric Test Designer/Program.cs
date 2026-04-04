using Psychometric_Test_Designer.Data;
using Microsoft.EntityFrameworkCore;
using Psychometric_Test_Designer.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Psychometric_Test_Designer.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddScoped<PasswordService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<TokenGenerator>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<GroupService>();
builder.Services.AddScoped<TestService>();
builder.Services.AddScoped<TestProcessingService>();
builder.Services.AddScoped<UserService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();

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

app.UseAuthorization();

app.MapControllers();

app.Run();
