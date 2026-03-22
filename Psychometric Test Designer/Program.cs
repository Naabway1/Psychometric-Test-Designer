using Psychometric_Test_Designer.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/test-db", async (AppDbContext db) => { return await db.Users.ToListAsync(); });

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
