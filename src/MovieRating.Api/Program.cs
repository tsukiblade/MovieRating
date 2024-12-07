using Microsoft.EntityFrameworkCore;
using MovieRating.Api;
using MovieRating.Api.Database;
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

app.MapMovieEndpoints();
app.MapTestDataEndpoints();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MovieRatingDbContext>();
    await db.Database.MigrateAsync();
}

await app.RunAsync();