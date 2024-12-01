using System.Reflection;
using Microsoft.EntityFrameworkCore;
using MovieRating.Api.Models;

namespace MovieRating.Api.Database;

public class MovieRatingDbContext : DbContext
{
    public MovieRatingDbContext(DbContextOptions<MovieRatingDbContext> options)
        : base(options)
    {
    }
    
    public virtual DbSet<Movie> Movies { get; set; }
    
    public virtual DbSet<Comment> Comments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        
        base.OnModelCreating(modelBuilder);
    }
}