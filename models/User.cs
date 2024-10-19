namespace BookLibraryAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

         public ICollection<Book>? Books { get; set; } 

        public User(int id, string firstName,string lastName, String email, string password){
            Id=id;
            FirstName=firstName;
            LastName=lastName;
            Email = email;
            Password=password;
        }

        public void AddBook(Book book)
        {
            if (Books == null)
            {
                Books = new List<Book>();
            }
            Books.Add(book);
        }
    }
}