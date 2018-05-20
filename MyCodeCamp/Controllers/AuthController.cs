using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MyCodeCamp.Data;
using MyCodeCamp.Data.Entities;
using MyCodeCamp.Filters;


namespace MyCodeCamp.Controllers
{
    public class AuthController : Controller
    {
        private readonly CampContext _context;
        private readonly SignInManager<CampUser> _signInMgr;
        private readonly ILogger<AuthController> _logger;
        private readonly UserManager<CampUser> _userMgr;
        private readonly IConfigurationRoot _config;

        public AuthController(CampContext context, SignInManager<CampUser> signInMgr, ILogger<AuthController> logger,
            UserManager<CampUser> userMgr, IConfiguration config
        )
        {
            _context = context;
            _signInMgr = signInMgr;
            _logger = logger;
            _userMgr = userMgr;
            _config = config as IConfigurationRoot;
        }

        [HttpPost("api/auth/login")]
        [ValidateModel]
        public async Task<IActionResult> Login([FromBody] CredentialModel model)
        {
            try
            {
               var result = await _signInMgr.PasswordSignInAsync(model.UserName, model.Password, false, false);
                if (result.Succeeded)
                {
                    return Ok();
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Exception thrown while loggin in: {e}");
            }

            return BadRequest("Failed to login");
        }


        [ValidateModel]
        [HttpPost("api/auth/token")]
        public async Task<IActionResult> CreateToken([FromBody] CredentialModel model)
        {
            try
            {
                var user = await _userMgr.FindByNameAsync(model.UserName);
                if (user != null)
                {
                    var result = await _signInMgr.CheckPasswordSignInAsync(user, model.Password, false) ;
                    if (result.Succeeded)
                    {
                        var claims = new[]
                        {
                            new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        };

                        // в боевом коде нужно брать из секретного хранилища или конфига
                        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Tokens:Key"]));

                        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                        var token = new JwtSecurityToken(
                            issuer: _config["Tokens:Issuer"],
                            audience:  _config["Tokens:Audience"],
                            claims: claims,
                            expires: DateTime.UtcNow.AddMinutes(60),
                            signingCredentials: creds
                        );

                        return Ok(new
                        {
                            token = new JwtSecurityTokenHandler().WriteToken(token),
                            expiration = token.ValidTo,
                        });

                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Exception thrown while creating JWT: {e}");
            }

            return BadRequest("Failed to generate token");

        }
    }
}