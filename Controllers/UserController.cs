using Microsoft.AspNetCore.Mvc;
using TaskManagementAPI.Data;
using TaskManagementAPI.Models;
using System.Security.Cryptography;
using System.Text;

namespace TaskManagementAPI.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ Create a new user (Admin or Team Member)
        [HttpPost]
        public IActionResult CreateUser([FromBody] User user)
        {
            if (string.IsNullOrWhiteSpace(user.Role))
                return BadRequest("Role cannot be empty.");

            string formattedRole = NormalizeRole(user.Role);
            if (formattedRole != "Admin" && formattedRole != "TeamMember")
                return BadRequest("Invalid role. Use 'Admin' or 'TeamMember'.");

            user.Role = formattedRole;

            // 🚨 Check if username already exists
            if (_context.Users.Any(u => u.Username == user.Username))
                return BadRequest("Username already exists. Choose a different one.");

            // 🚀 Hash the password before saving it
            user.Password = HashPassword(user.Password);

            _context.Users.Add(user);
            _context.SaveChanges();
            return Ok("User created successfully");
        }

        // ✅ User Login (Authenticate with Username & Password)
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == request.Username);

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid username or password" }); // JSON format for errors
            }

            string hashedPassword = HashPassword(request.Password);
            if (user.Password != hashedPassword)
            {
                return Unauthorized(new { message = "Invalid username or password" }); // JSON format for errors
            }

            return Ok(new
            {
                message = "Login successful",
                userId = user.UserId,
                role = user.Role
            });
        }


        // ✅ Get all users
        [HttpGet]
        public IActionResult GetUsers()
        {
            return Ok(_context.Users.ToList());
        }

        // ✅ Delete a user (Only Admins can delete)
        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id, [FromHeader] int adminId)
        {
            var admin = _context.Users.Find(adminId);
            if (admin == null || NormalizeRole(admin.Role) != "Admin")
                return Unauthorized("Only admins can delete users");

            var user = _context.Users.Find(id);
            if (user == null) return NotFound("User not found");

            _context.Users.Remove(user);
            _context.SaveChanges();
            return Ok("User deleted successfully");
        }

        // ✅ Utility function: Normalize Role (Case-Insensitive Handling)
        private string NormalizeRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role))
                return string.Empty;

            string lowerRole = role.ToLower();

            if (lowerRole == "admin") return "Admin";
            if (lowerRole == "teammember") return "TeamMember";

            return string.Empty;
        }

        // ✅ Utility function: Hash password using SHA256
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }

    // ✅ Login request model
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
