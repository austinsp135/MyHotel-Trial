using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MyHotel.Data;
using MyHotel.Models;
using MyHotel.Models.RequestModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MyHotel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {

        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IConfiguration configuration;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private object signinManager;

        public AccountsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            this.db = context;
            this.userManager = userManager;
            this.configuration = configuration;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }



        [HttpPost("Login")]

        public async Task<IActionResult> Login(LoginRequestModel model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return BadRequest();
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, true);
            if (!result.Succeeded)
            {
                return BadRequest(new ResponseModel<string>
                {
                    Success = false,
                    Message = "Invalid email / password."
                });
            }

            var token = await GenerateToken(user);

            return Ok(new ResponseModel<string>
            {
                Data = token,
                Message = "Login Successful",
            });

        }



        //first code login

        //[HttpGet]
        //public IActionResult Login()
        //{
        //    return View();
        //}

        //[HttpPost]
        //public async Task<IActionResult> Login(LoginRequestModel model)
        //{
        //    if (!ModelState.IsValid)
        //        return View(model);

        //    var user = await userManager.FindByEmailAsync(model.Email);
        //    if (user == null)
        //    {
        //        ModelState.AddModelError("", "Invalid details");
        //        return View(model);
        //    }

        //    var res = await S.PasswordSignInAsync(user, model.Password, true, true);

        //    if (res.Succeeded)

        //        return View(model);
        //}



        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody]RegisterRequestModel model)
        {
            var user = new ApplicationUser()
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                UserName = Guid.NewGuid().ToString().Replace("-",""),
                PhoneNumber = model.PhoneNumber,
                Aadhar= model.Aadhar,
                Age = model.Age,
            };
            var res = await userManager.CreateAsync(user, model.Password);
            await userManager.AddToRoleAsync(user,"USER");
            if (!res.Succeeded)
               return BadRequest(res);

            await db.SaveChangesAsync();
            return Ok(user);
        }

        //public async Task<IActionResult> Logout()
        //{
        //    await _signInManager.SignOutAsync();
        //}

        
        private async Task<string> GenerateToken(ApplicationUser user)
        {
            var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
            var issuer = configuration["Jwt:Issuer"];
            var audience = configuration["Jwt:Audience"];

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var userRoles = await userManager.GetRolesAsync(user);
            var role = userRoles.FirstOrDefault();
            var claims = new Claim[]
            {
                  new(ClaimTypes.NameIdentifier, user.UserName),
                  new(ClaimTypes.Email, user.Email),
                  new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                  new("Role", role)
            };

            var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpGet]
        public async Task<IActionResult> GenerateData()
        {
            await _roleManager.CreateAsync(new IdentityRole() { Name = "Admin" });
            await _roleManager.CreateAsync(new IdentityRole() { Name = "User" });




            var users = await userManager.GetUsersInRoleAsync("Admin");
            if (users.Count == 0)
            {
                var appUser = new ApplicationUser()
                {
                    FirstName = "Admin",
                    LastName = "User",
                    Email = "admin@admin.com",
                    UserName = "admin",
                };
                var res = await userManager.CreateAsync(appUser, "Pass@123");
                await userManager.AddToRoleAsync(appUser, "Admin");
            }
            return Ok("Data generated");
        }

    }
}

