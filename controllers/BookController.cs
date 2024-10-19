using BookLibraryAPI.Data;
using BookLibraryAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace BookLibraryAPI.Controllers
{
    /// <summary>
    /// Controller that allows adding, deleting and listing books from a user's collection
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BookController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Adds a new book to a user's collection.
        /// </summary>
        /// <param name="book">The book to add.</param>
        /// <param name="userId">The ID of the user.</param>
        /// <response code="200">Book added successfully to user's collection.</response>
        /// <response code="404">User not found.</response>
        [HttpPost]
        [SwaggerResponse(200, "Book added successfully to user's collection", typeof(string))]
        [SwaggerResponse(404, "User not found")]
        public async Task<ActionResult<string>> AddBookToList(Book book, int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            book.UserId = userId;
            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            return Ok($"Book added at your collection {user.FirstName}");
        }

        /// <summary>
        /// Retrieves all books belonging to a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <response code="200">List of books returned successfully.</response>
        /// <response code="404">User not found.</response>
        [HttpGet("user/{userId}")]
        [SwaggerResponse(200, "List of books returned successfully", typeof(IEnumerable<BookDto>))]
        [SwaggerResponse(404, "User not found")]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetBooksByUser(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var books = await _context.Books
                .Where(b => b.UserId == userId)
                .Select(b => new BookDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Author = b.Author,
                    PublicationYear = b.PublicationYear,
                    Rating = b.Rating,
                    Review = b.Review
                })
                .ToListAsync();

            return Ok(books);
        }

        /// <summary>
        /// Deletes a book from a user's collection.
        /// </summary>
        /// <param name="bookId">The ID of the book to delete.</param>
        /// <param name="userId">The ID of the user who owns the book.</param>
        /// <response code="200">Book deleted successfully.</response>
        /// <response code="404">Book or user not found.</response>
        [HttpDelete("{bookId}/user/{userId}")]
        [SwaggerResponse(200, "Book deleted successfully", typeof(string))]
        [SwaggerResponse(404, "Book or user not found")]
        public async Task<ActionResult<string>> DeleteBook(int bookId, int userId)
        {
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == bookId && b.UserId == userId);
            if (book == null)
            {
                return NotFound("Book or user not found.");
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return Ok($"the book {book.Title} delete successful");
        }

        /// <summary>
        /// Updates the review and rating of a book.
        /// </summary>
        /// <param name="bookId">The ID of the book to update.</param>
        /// <param name="request">The review and rating data.</param>
        /// <response code="200">Review and rating updated successfully.</response>
        /// <response code="400">Invalid rating provided.</response>
        /// <response code="404">Book not found.</response>
        [HttpPatch("{bookId}")]
        [SwaggerResponse(200, "Review and rating updated successfully", typeof(string))]
        [SwaggerResponse(400, "The rating must be between 1 and 5")]
        [SwaggerResponse(404, "Book not found")]
        public async Task<ActionResult<string>> UpdateReviewAndRating(int bookId, [FromBody] UpdateReviewAndRatingRequest request)
        {
            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
            {
                return NotFound("Book not found");
            }

            if (request.Rating < 1 || request.Rating > 5)
            {
                return BadRequest("The rating must be between 1 and 5");
            }

            book.Review = request.Review;
            book.Rating = request.Rating;

            _context.Entry(book).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok("Review and rating updated");
        }

        /// <summary>
        /// Retrieves a specific book by ID.
        /// </summary>
        /// <param name="id">The ID of the book to retrieve.</param>
        /// <response code="200">Book returned successfully.</response>
        /// <response code="404">Book not found.</response>
        [HttpGet("{id}")]
        [SwaggerResponse(200, "Book returned successfully", typeof(Book))]
        [SwaggerResponse(404, "Book not found")]
        public async Task<ActionResult<Book>> GetBook(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound("Book not found.");
            }

            return Ok(book);
        }
    }

    public class UpdateReviewAndRatingRequest
    {
        public string? Review { get; set; }
        public int Rating { get; set; }
    }
}
