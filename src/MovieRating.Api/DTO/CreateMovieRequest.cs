namespace MovieRating.Api.DTO;

public record CreateMovieRequest(string Title, string Description, string Genre, string Director, List<string> Actors);