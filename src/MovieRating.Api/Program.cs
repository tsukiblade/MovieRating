using Bogus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieRating.Api.Database;
using MovieRating.Api.Models;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resourceBuilder => resourceBuilder.AddService("MovieRating"))
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation();

        metrics.AddOtlpExporter();
    })
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation();

        tracing.AddOtlpExporter();
    });

builder.Logging.AddOpenTelemetry(logging => { logging.AddOtlpExporter(); });

builder.Services.AddDbContext<MovieRatingDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("MovieRating"));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/test-data", async ([FromServices] MovieRatingDbContext dbContext) =>
    {
        var testComments = new Faker<Comment>()
            .StrictMode(true)
            .RuleFor(c => c.Id, f => f.Random.Guid())
            .RuleFor(c => c.Username, f => f.Internet.UserName())
            .RuleFor(c => c.Title, f => f.Lorem.Sentence())
            .RuleFor(c => c.Content, f => f.Rant.Review())
            .RuleFor(c => c.Rating, f => f.Random.Number(1, 5));

        var testMovies = new Faker<Movie>()
            .StrictMode(true)
            .RuleFor(m => m.Id, f => f.Random.Guid())
            .RuleFor(m => m.Title, f => f.Lorem.Word())
            .RuleFor(m => m.Description, f => f.Lorem.Sentences(3))
            .RuleFor(m => m.Actors, f => f.Lorem.Words(4).ToList())
            .RuleFor(m => m.Genre, f => f.Lorem.Word())
            .RuleFor(m => m.Director, f => f.Name.FullName())
            .RuleFor(m => m.Comments, f => testComments.Generate(f.Random.Number(1, 10)).ToList());

        var movies = testMovies.Generate(1000);

        await dbContext.Movies.AddRangeAsync(movies);

        await dbContext.SaveChangesAsync();

        return Results.Created("/movies", movies);
    })
    .WithName("GenerateTestData")
    .WithOpenApi();

app.MapGet("/movies", async ([FromQuery] int? top, [FromServices] MovieRatingDbContext dbContext, [FromServices] ILogger<Program> logger, CancellationToken cancellationToken) =>
    {
        logger.LogInformation("GetMovies called");
        
        return await dbContext.Movies
            .Include(m => m.Comments)
            .AsNoTracking()
            .Take(top ?? 100)
            .ToListAsync(cancellationToken: cancellationToken);
    })
    .WithName("GetMovies")
    .WithOpenApi();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MovieRatingDbContext>();
    await db.Database.MigrateAsync();
}

await app.RunAsync();