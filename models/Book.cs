namespace BookLibraryAPI.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public int PublicationYear { get; set; }
        public int Rating { get; set; }
        public string? Review { get; set; }
         public int? UserId { get; set; } 
        public User? User { get; set; }
        public Book(string title, string author, int publicationYear)
    {
        Title = title;  
        Author = author;
        PublicationYear = publicationYear;
    }
    }
}