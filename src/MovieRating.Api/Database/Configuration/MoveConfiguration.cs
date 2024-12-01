using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieRating.Api.Models;

namespace MovieRating.Api.Database.Configuration;

public class MoveConfiguration : IEntityTypeConfiguration<Movie>
{
    public void Configure(EntityTypeBuilder<Movie> builder)
    {
        builder.HasKey(m => m.Id);
        
        builder.Property(m => m.Title)
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(m => m.Genre)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(m => m.Description);
        
        builder.Property(m => m.Director)
            .HasMaxLength(100);
        
        builder.Property(m => m.Actors);

        builder.HasMany(m => m.Comments)
            .WithOne();
    }
}