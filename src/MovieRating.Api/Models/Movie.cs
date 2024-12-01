namespace MovieRating.Api.Models;

public class Movie
{
    public Guid Id { get; set; }
    
    public string Title { get; set; }
    
    public string Genre { get; set; }
    
    public double Rating => Comments.Select(c => c.Rating).Average();
    
    public string? Description { get; set; }
    
    public string? Director { get; set; }
    
    public List<string> Actors { get; set; }

    public List<Comment> Comments { get; set; }
    
    public Movie(Guid id, string title, string genre, string? description, string? director, List<string> actors)
    {
        Id = id;
        Title = title;
        Genre = genre;
        Description = description;
        Director = director;
        Actors = actors;
        Comments = new List<Comment>();
    }

    public Movie()
    {
    }
}