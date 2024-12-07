using Bogus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieRating.Api.Database;
using MovieRating.Api.DTO;
using MovieRating.Api.Models;

namespace MovieRating.Api;

public static class Endpoints
{
    public static void MapMovieEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/movies", async ([FromQuery] int? page, [FromQuery] int? pageSize,
                [FromServices] MovieRatingDbContext dbContext,
                [FromServices] ILogger<Program> logger, CancellationToken cancellationToken) =>
            {
                logger.LogInformation("GetMovies called");

                var movies = await dbContext.Movies
                    .Include(m => m.Comments)
                    .AsNoTracking()
                    .Skip(pageSize * (page - 1) ?? 0)
                    .Take(pageSize ?? 10)
                    .ToListAsync(cancellationToken: cancellationToken);

                return Results.Ok(movies);
            })
            .WithName("GetMovies")
            .WithOpenApi();

        app.MapPost("/movies",
                async (CreateMovieRequest request, ILogger<Program> logger, MovieRatingDbContext dbContext) =>
                {
                    logger.LogInformation("Processing request {@Request}", request);
                    var movie = new Movie
                    {
                        Title = request.Title,
                        Description = request.Description,
                        Genre = request.Genre,
                        Director = request.Director,
                        Actors = request.Actors,
                        Comments = []
                    };

                    await dbContext.Movies.AddAsync(movie);
                    await dbContext.SaveChangesAsync();

                    return Results.Created($"/movies/{movie.Id}", movie);
                })
            .WithName("CreateMovie")
            .WithOpenApi();

        app.MapGet("/movies/{id:guid}", async (Guid id, ILogger<Program> logger, MovieRatingDbContext dbContext) =>
            {
                logger.LogInformation("GetMovie called with id {Id}", id);

                var movie = await dbContext.Movies
                    .Include(m => m.Comments)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.Id == id);

                return Results.Ok(movie);
            })
            .WithName("GetMovie")
            .WithOpenApi();


        app.MapPost("/movies/{id:guid}/comments",
                async (Guid id, AddCommentRequest request, ILogger<Program> logger, MovieRatingDbContext dbContext) =>
                {
                    logger.LogInformation("Adding comment to movie {Id}", id);

                    var movie = await dbContext.Movies
                        .Include(m => m.Comments)
                        .FirstOrDefaultAsync(m => m.Id == id);

                    if (movie == null)
                    {
                        return Results.NotFound();
                    }

                    var comment = new Comment
                    {
                        Username = request.Username,
                        Title = request.Title,
                        Content = request.Content,
                        Rating = request.Rating
                    };

                    movie.Comments.Add(comment);
                    await dbContext.SaveChangesAsync();

                    return Results.Created($"/movies/{id}/comments/{comment.Id}", comment);
                })
            .WithName("AddComment")
            .WithOpenApi();
    }


    public static void MapTestDataEndpoints(this IEndpointRouteBuilder app)
    {
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

        app.MapPost("/clear-data", async ([FromServices] MovieRatingDbContext dbContext) =>
            {
                await dbContext.Comments.ExecuteDeleteAsync();
                await dbContext.Movies.ExecuteDeleteAsync();

                return Results.NoContent();
            })
            .WithName("ClearTestData")
            .WithOpenApi();
    }
}