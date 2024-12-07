namespace MovieRating.Api.DTO;

public record AddCommentRequest(string Username, string Title, string Content, int Rating);