namespace MovieRating.Api.Models;

public class Comment
{
    public Guid Id { get; set; }

    public string Username { get; set; }

    public string? Title { get; set; }
    
    public string? Content { get; set; }
    
    public int Rating { get; set; }

    public Comment(Guid id, string username, string? title, string? content, int rating)
    {
        Id = id;
        Username = username;
        Title = title;
        Content = content;
        Rating = rating;
    }

    public Comment()
    {
    }
}