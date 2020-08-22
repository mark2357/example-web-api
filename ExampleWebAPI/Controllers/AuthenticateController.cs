using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ExampleWebAPI.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ExampleWebAPI.Controllers {

    [Route("users/")]
    [ApiController]
    public class AuthenticateController : ControllerBase {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IConfiguration _configuration;

        public AuthenticateController(UserManager<ApplicationUser> userManager, IConfiguration configuration) {
            this.userManager = userManager;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("authenticate")]
        public async Task<IActionResult> Login([FromBody] LoginModel model) {
            // checks user exists for user-name and password is valid
            var user = await userManager.FindByNameAsync(model.Username);
            if (user != null && await userManager.CheckPasswordAsync(user, model.Password)) {

                var userRoles = await userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                foreach (var userRole in userRoles) {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

                var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    expires: DateTime.Now.AddDays(7),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                    );

                return Ok(new {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo
                });
            }
            // returns 401 as user-name or password is incorrect
            return Unauthorized();
        }

        [HttpPost]
        [Route("create-account")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model) {
            var userExists = await userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User already exists!" });

            ApplicationUser user = new ApplicationUser() {
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username
            };
            var result = await userManager.CreateAsync(user, model.Password);

            // if error determine the best error message to return
            if (!result.Succeeded) {
                string msg = "User creation failed!";
                bool invalidPassword = false;

                // checks to see if the error is caused by the password being incorrect
                foreach (IdentityError error in result.Errors) {
                    if(error.Code.Contains("Password")) {
                        msg += " " + error.Description;
                        invalidPassword = true;
                    }
                }
                // if the password is invalid returns error with message describing why the password is invalid
                if (invalidPassword) {
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = msg});
                }

                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });

            }

            return Ok(new Response { Status = "Success", Message = "User created successfully!" });
        }
    }
}
