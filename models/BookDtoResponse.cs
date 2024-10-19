namespace BookLibraryAPI.Models
{
public class BookDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Author { get; set; }
    public int PublicationYear { get; set; }
    public int Rating { get; set; }
    public string? Review { get; set; }
}
}