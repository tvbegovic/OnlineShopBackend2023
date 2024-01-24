using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineShopBackend.JWT;
using System.Data.SqlClient;
using System.Security.Claims;

namespace OnlineShopBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly IJwtAuthManager jwtAuthManager;

        public UserController(IConfiguration configuration, IJwtAuthManager jwtAuthManager)
        {
            this.configuration = configuration;
            this.jwtAuthManager = jwtAuthManager;
        }

        [HttpGet("login")]
        public IActionResult Login(string? username, string? password)
        {
            using (var db = new SqlConnection(configuration.GetConnectionString("connString")))
            {
                var user = db.QueryFirstOrDefault<Employee>(
                    @"SELECT * FROM Employee WHERE (username = @username OR email = @username) 
                        AND password = @password", new { username, password });
                if(user == null)
                {
                    return BadRequest("Pogrešni korisnički podaci");
                }
                user.Password = null;
                var claims = new Claim[] { new Claim(ClaimTypes.Name, username) };
                var jwtResult = jwtAuthManager.GenerateTokens(username, claims, DateTime.Now);
                var loginResult = new LoginResult();
                loginResult.User = user;
                loginResult.AccessToken = jwtResult.AccessToken;
                loginResult.RefreshToken = jwtResult.RefreshToken.TokenString;
                return Ok(loginResult);

            }
        }
    }

    public class LoginResult
    {
        public Employee User { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
