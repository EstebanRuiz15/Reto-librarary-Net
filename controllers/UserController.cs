using System.Runtime.InteropServices;
using BookLibraryAPI.Data;
using BookLibraryAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace BookLibraryAPI.Controllers
{
    /// <summary>
    /// Controller for users, where they can create user, update, list, modify and delete user
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves the list of all users.
        /// </summary>
        /// <response code="200">Successfully retrieved users.</response>
        [HttpGet]
        [SwaggerResponse(200, "Successfully retrieved users", typeof(IEnumerable<User>))]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        /// <summary>
        /// Creates a new user.
        /// </summary>
        /// <param name="user">The user data to create.</param>
        /// <response code="201">User created successfully.</response>
        /// <response code="400">Invalid input data.</response>
        [HttpPost]
        [SwaggerResponse(201, "User created successfully", typeof(User))]
        [SwaggerResponse(400, "Invalid input data")]
        [SwaggerResponse(400, "Email already exist")]
        [SwaggerResponse(400, "Incorrect password format, must have at least 1 number, 1 capital letter and 6 digits")]
        public async Task<ActionResult<User>> CreateUser(User user)
        {
            Console.WriteLine($"Creating user: {user.FirstName} {user.LastName}");

            if (string.IsNullOrWhiteSpace(user.FirstName))
            {
                return BadRequest("First name is required");
            }

            if (string.IsNullOrWhiteSpace(user.LastName))
            {
                return BadRequest("Last name is required");
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                return BadRequest("Email is required");
            }

            var passwordRegex = new System.Text.RegularExpressions.Regex(@"^(?=.*[A-Z])(?=.*\d)[A-Za-z\d]{6,}$");
            if (string.IsNullOrWhiteSpace(user.Password) || !passwordRegex.IsMatch(user.Password))
            {
                return BadRequest("Password must be at least 6 characters long, contain one uppercase letter, and one number");
            }

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == user.Email);
            if (existingUser != null)
            {
                return BadRequest("Email already exists");
            }

            if (string.IsNullOrWhiteSpace(user.Password))
            {
                return BadRequest("Password is required");
            }

            var passwordHasher = new PasswordHasher<User>();
            user.Password = passwordHasher.HashPassword(user, user.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUsers), new { id = user.Id }, user);
        }

        /// <summary>
        /// Updates an existing user.
        /// </summary>
        /// <param name="id">The ID of the user to update.</param>
        /// <param name="updatedUser">The updated user data.</param>
        /// <response code="200">User updated successfully.</response>
        /// <response code="400">Invalid input data.</response>
        /// <response code="404">User not found.</response>
        [HttpPut("{id}")]
        [SwaggerResponse(200, "User updated successfully", typeof(string))]
        [SwaggerResponse(400, "Invalid input data")]
        [SwaggerResponse(404, "User not found")]
        public async Task<ActionResult<string>> UpdateUser(int id, UserDtoUpdate? updatedUser)
        {
            if (updatedUser == null)
                return BadRequest("User data is null");

            var userInDb = await _context.Users.FindAsync(id);
            if (userInDb == null)
                return NotFound("User not found");

            if (!string.IsNullOrEmpty(updatedUser.FirstName))
                userInDb.FirstName = updatedUser.FirstName;

            if (!string.IsNullOrEmpty(updatedUser.LastName))
                userInDb.LastName = updatedUser.LastName;

            if (!string.IsNullOrEmpty(updatedUser.Email))
                userInDb.Email = updatedUser.Email;

            await _context.SaveChangesAsync();

            return Ok($"User {userInDb.FirstName} updated");
        }

        /// <summary>
        /// Deletes a user by ID.
        /// </summary>
        /// <param name="id">The ID of the user to delete.</param>
        /// <response code="200">User deleted successfully.</response>
        /// <response code="404">User not found.</response>
        [HttpDelete("{id}")]
        [SwaggerResponse(200, "User deleted successfully", typeof(string))]
        [SwaggerResponse(404, "User not found")]
        public async Task<ActionResult<string>> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("User not found");
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok($"User {user.FirstName} deleted successfully");
        }

        /// <summary>
        /// Retrieves a user along with their books.
        /// </summary>
        /// <param name="id">The ID of the user.</param>
        /// <response code="200">User retrieved successfully.</response>
        /// <response code="404">User not found.</response>
        [HttpGet("{id}/with-books")]
        [SwaggerResponse(200, "User retrieved successfully", typeof(UserDto))]
        [SwaggerResponse(404, "User not found")]
        public async Task<ActionResult<UserDto>> GetUserWithBooks(int id)
        {
            var userWithBooks = await _context.Users
                .Include(u => u.Books)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (userWithBooks == null)
            {
                return NotFound("User not found");
            }

            var userDto = new UserDto
            {
                Id = userWithBooks.Id,
                FirstName = userWithBooks.FirstName,
                LastName = userWithBooks.LastName,
                Email = userWithBooks.Email,
                Books = userWithBooks.Books?.Select(b => new BookDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Author = b.Author,
                    PublicationYear = b.PublicationYear,
                    Rating = b.Rating,
                    Review = b.Review
                }).ToList()
            };

            return Ok(userDto);
        }
    }
}
