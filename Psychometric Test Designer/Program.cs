using Psychometric_Test_Designer.Data;
using Microsoft.EntityFrameworkCore;
using Psychometric_Test_Designer.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddScoped<PasswordService>();
builder.Services.AddScoped<AuthService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));




var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/test-db", async (AppDbContext db) => { return await db.Users.ToListAsync(); });

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
