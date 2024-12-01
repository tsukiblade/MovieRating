using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieRating.Api.Models;

namespace MovieRating.Api.Database.Configuration;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.Username)
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(c => c.Title)
            .HasMaxLength(100);
        
        builder.Property(c => c.Content)
            .HasMaxLength(255);
        
        builder.Property(c => c.Rating)
            .IsRequired();
    }
}