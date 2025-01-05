using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Account_Apis.Data;
using Account_Apis.Dtos;
using Account_Apis.Interfaces;
using Account_Apis.Models;
using Account_Apis.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace Account_Apis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly MyDbContext _context;
        // private readonly IAccountRepository _accountRepository;

        private readonly UserManager<IdentityUser> _userManager;

        private readonly IEmailService _emailService;

        
        public UsersController(MyDbContext context, IEmailService emailService, UserManager<IdentityUser> userManager)
        {
            _context = context;
            // _accountRepository = accountRepository;
            _userManager = userManager;
            _emailService = emailService;
        }
        

        // sign up using _userManager
        [HttpPost]
        [Route("sign-up")]
        public async Task<IActionResult> SignUp([FromBody] UserDto userDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // verify if user already exists or not 
            var userExists= await _userManager.FindByEmailAsync(userDto.Email!);

            if (userExists != null)
            {
                return BadRequest("User already exists");
            }
            // add user to the database
            IdentityUser user = new ()
            {
                UserName = userDto.UserName,
                SecurityStamp=Guid.NewGuid().ToString(),
                Email = userDto.Email
            };
            var result = await _userManager.CreateAsync(user, userDto.Password!);

            if(result.Succeeded)
            {
                return Ok("User created successfully");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("error", error.Description);
                }
                return Ok(ModelState);
            }
            
        }


        // login
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email && u.Password == loginDto.Password);

            var user = await _userManager.FindByEmailAsync(loginDto.Email!);

            if (user != null && await _userManager.CheckPasswordAsync(user, loginDto.Password!))
            {
                return Ok("Login successful");
            }
            else
            {
                return BadRequest("Invalid credentials");
            }
        }
        
        // get all users
        [HttpGet]
        [Route("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }

        // get user by id
        [HttpGet]
        [Route("user/{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null)
            {
                return NotFound("User not found");
            }
            else
            {
                return Ok(user);
            }
            
        }

        // delete user
        [HttpDelete]
        [Route("delete/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null)
            {
                return NotFound("User not found");
            }
            else
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                return Ok("User deleted successfully");
            }
        }

        
        // test email sending 
        [HttpGet]
        [Route("test-email")]
        public async Task<IActionResult> TestEmail()
        {
            var message = new Message(["mahmoud123abdelhamid@gmail.com"], "Test email", "LOOOOL");
            await _emailService.SendEmail(message);

            return StatusCode(StatusCodes.Status200OK);
        }
        


        // forgot password 
        [HttpPost]
        [Route("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgetPasswordDto forgetPasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // verify if user already exists or not 
            var user1 = _context.Users.FirstOrDefault(u => u.Email == forgetPasswordDto.Email);

            if (user1 != null)
            {
                IdentityUser user = new ()
                {
                    Email = forgetPasswordDto.Email
                };
                

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var ForgetPasswordLink = Url.Action(nameof(ResetPassword), "Users", new {token ,email = user.Email }, Request.Scheme);

                if (string.IsNullOrEmpty(ForgetPasswordLink))
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to generate the reset password link.");
                }

                var message = new Message(new string[]{ user.Email! }, "Forget Password Link", ForgetPasswordLink!);
                await _emailService.SendEmail(message);
                return Ok("Password reset link sent to your email");

            }
            else
            {
                return BadRequest("User not found");
            }
        }

        // Get reset password
        [HttpGet]
        [Route("reset-password")]
        public async Task<IActionResult> ResetPassword(string token, string email)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var model = new ResetPasswordDto {
                Token = token,
                Email = email
            };

            return Ok(model);
            
        }

        // reset password (Update password)
        [HttpPost]
        [Route("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user1 = _context.Users.FirstOrDefault(u => u.Email == resetPasswordDto.Email);

            if (user1 != null)
            {
                IdentityUser user = new ()
                {
                    Email = resetPasswordDto.Email

                };
                

                var result = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.Password);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("error", error.Description);
                    }
                    return Ok(ModelState);
                }
                else
                {
                    return BadRequest("Password reset failed");
                }   

            }
            else
            {
                return BadRequest("User not found");
            }
        }

        


    }
}