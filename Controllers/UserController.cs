using Microsoft.AspNetCore.Mvc;

using TaskManagementAPI.Data;

using TaskManagementAPI.Models;

using System.Security.Cryptography;

using System.Text;

using TaskManagementAPI.Model;
 
 
namespace TaskManagementAPI.Controllers

{

    [Route("api/users")]

    [ApiController]

    public class UserController : ControllerBase

    {

        private readonly AppDbContext userContext;
 
        public UserController(AppDbContext context)

        {

            userContext = context;

        }
 
        // creating or registering new user

        [HttpPost]

        public IActionResult CreateUser([FromBody] User user)

        {

            if (string.IsNullOrWhiteSpace(user.Role))

                return BadRequest("Role cannot be empty.");
 
            string formattedRole = NormalizeRole(user.Role);

            if (formattedRole != "Admin" && formattedRole != "TeamMember")

            {

                return BadRequest("Invalid role. Use 'Admin' or 'TeamMember'.");

            }
 
            user.Role = formattedRole;
 
            //if user exist then this  will display below message

            if (userContext.Users.Any(u => u.Username == user.Username))

            {

                return BadRequest("Username already exists. Choose a different UserName.");
                    
            }
 
            //password stored in db with hashing

            user.Password = HashPassword(user.Password);
 
            userContext.Users.Add(user);

            userContext.SaveChanges();

            return Ok("User created successfully");

        }
 
        // user/admin  login

        [HttpPost("login")]

        public IActionResult Login([FromBody] LoginRequest request)

        {

            var user = userContext.Users.FirstOrDefault(u => u.Username == request.Username);
 
            if (user == null)

            {

                return Unauthorized(new { message = "Invalid username or password" });

            }
 
            string hashedPassword = HashPassword(request.Password);

            if (user.Password != hashedPassword)

            {

                return Unauthorized(new { message = "Invalid username or password" }); 

            }
 
            return Ok(new

            {

                message = "Login successful",

                userId = user.UserId,

                role = user.Role

            });

        }
 
 
        // view all users

        [HttpGet]

        public IActionResult GetUsers()

        {

            return Ok(userContext.Users.ToList());

        }
 
 
        //normalizing  for uppercase and lowercase

        private string NormalizeRole(string role)

        {

            if (string.IsNullOrWhiteSpace(role))

                return string.Empty;
 
            string lowerRole = role.ToLower();
 
            if (lowerRole == "admin") return "Admin";

            if (lowerRole == "teammember") return "TeamMember";
 
            return string.Empty;

        }
 
        // hasing using Sha

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
 
   

}

 