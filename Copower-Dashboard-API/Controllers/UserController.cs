using Copower_API.Entities;
using Copower_API.Helpers;
using Copower_API.Models.User;
using Copower_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace Copower_API.Controllers
{
    /// <summary>
    /// User controller
    /// </summary>
    /// <param name="userService">User service component</param>
    /// <param name="utilsService">Utils service component</param>
    /// <param name="settings">Application settings</param>
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(IUserService userService, IUtilsService utilsService, IOptions<Settings> settings) : ControllerBase
    {
        /// <summary>
        /// Authenticate user
        /// </summary>
        /// <param name="model">User email and password</param>
        /// <returns></returns>
        /// 
        [HttpPost("authenticate")]
        [Authorize(AuthenticationSchemes = "ApiKey")]
        //[AllowAnonymous]// This endpoint should be accessible without login
        public async Task<IActionResult> Authenticate([FromBody] AuthenticateModel model)
        {
            try
            {
                var user = await userService.Authenticate(model);
                return Ok(user);
            }
            catch (Exception e)
            {
                if (e.Message.Length == 6)
                    return BadRequest("AUT" + e.Message);
                else
                    return BadRequest("AUT481713");
            }
        }

        /// <summary>
        /// Check if the login is still valid
        /// </summary>
        /// <returns></returns>
        [HttpGet("check")]
        [Authorize(AuthenticationSchemes = "ApiKeyAndJwt")]
        public async Task<IActionResult> Check()
        {
            try
            {
                var userId = utilsService.CheckAuthorization(Request);

                var userdata = await userService.Check(userId);
                if (userdata != null)
                    return Ok(userdata);
                else
                    return BadRequest(false);
            }
            catch (Exception e)
            {
                if (e.Message.Length == 6)
                    return BadRequest("USRC" + e.Message);
                else
                    return BadRequest("URSC696660");
            }
        }

        /// <summary>
        /// Deletes a user from the system.
        /// </summary>
        /// <returns>An IActionResult indicating the result of the delete operation.</returns>
        [HttpPost("delete")]
        [Authorize(AuthenticationSchemes = "ApiKeyAndJwt")]
        public async Task<IActionResult> DeleteUser(DeleteUserModel model)
        {
            try
            {
                var userId = utilsService.CheckAuthorization(Request);

                var deleted = await userService.DeleteUser(userId, model.Password);
                return Ok(deleted);
            }
            catch (Exception e)
            {
                if (e.Message.Length == 6)
                    return BadRequest("UDL" + e.Message);
                else
                    return BadRequest("UDL848713");
            }
        }

        /// <summary>
        /// Handles the forgot password functionality by sending a password reset link to the user's email.
        /// </summary>
        /// <param name="model">The model containing the email address of the user who forgot their password.</param>
        /// <returns>
        /// An IActionResult indicating the result of the operation. If the email is registered, a success message is returned.
        /// Otherwise, an error message is returned.
        /// </returns>
        [HttpPost("forgot-password")]
        [Authorize(AuthenticationSchemes = "ApiKey")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordModel model)
        {
            try
            {
                if (utilsService.CheckEmailValidity(model.Email) == false)
                    throw new Exception("278587");

                if (String.IsNullOrWhiteSpace(model.Language))
                    model.Language = "en";
                else if (settings.Value.Languages.Contains(model.Language) == false)
                    throw new Exception("278587");

                var result = await userService.ForgotPassword(model.Email, model.Language);
                return Ok(true);
            }
            catch (Exception e)
            {
                if (e.Message.Length == 6)
                    return BadRequest("UFPW" + e.Message);
                else
                    return BadRequest("UFPW848713");
            }
        }

        /// <summary>
        /// Get user profile
        /// </summary>
        /// <returns>User profile information</returns>
        [HttpGet("")]
        [Authorize(AuthenticationSchemes = "ApiKeyAndJwt")]
        public async Task<IActionResult> Get()
        {
            try
            {
                var userId = utilsService.CheckAuthorization(Request);

                var user = await userService.Get(userId);
                return Ok(user);
            }
            catch (Exception e)
            {
                if (e.Message.Length == 6)
                    return BadRequest("GUSR" + e.Message);
                else
                    return BadRequest("GUSR481713");
            }
        }

        /// <summary>
        /// Resets the password for a user using the provided token and new password.
        /// </summary>
        /// <param name="model">The reset password model containing the token and new password.</param>
        /// <returns>An IActionResult indicating the result of the password reset operation.</returns>
        [HttpPost("reset-password")]
        [Authorize(AuthenticationSchemes = "ApiKey")]
        //[SwaggerOperation(Tags = new[] { "Password" })]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
        {
            try
            {
                if ((string.IsNullOrWhiteSpace(model.Token) == true) || (model.Token.Length != settings.Value.ResetPasswordTokenLength))
                    throw new Exception("278587");

                var result = await userService.ResetPassword(model);
                return Ok(result);
            }
            catch (Exception e)
            {
                if (e.Message.Length == 6)
                    return BadRequest("RSPW" + e.Message);
                else
                    return BadRequest("RSPW481713");
            }
        }

        /// <summary>
        /// Updates an existing user in the system.
        /// </summary>
        /// <param name="updated">The updated user details.</param>
        /// <returns>An ActionResult containing the updated user details or a NotFound result if the user does not exist.</returns>
        [HttpPut("")]
        [Authorize(AuthenticationSchemes = "ApiKeyAndJwt")]
        public async Task<IActionResult> UpdateUser(UserProfileUpdateModel updated)
        {
            try
            {
                if (utilsService.CheckEmailValidity(updated.Email) == false)
                    throw new Exception("278587");

                if ((String.IsNullOrWhiteSpace(updated.Name) == true) || (updated.Name.Length > settings.Value.InputMax.Name))
                    throw new Exception("278587");

                if (utilsService.CheckPasswordValidity(updated.CurrentPassword) != 0)
                    throw new Exception("278587");

                if ((String.IsNullOrWhiteSpace(updated.NewPassword) == false) && (utilsService.CheckPasswordValidity(updated.NewPassword) != 0))
                    throw new Exception("278587");

                var userId = utilsService.CheckAuthorization(Request);

                var updatedUser = await userService.UpdateUser(userId, updated);
                return Ok();
            }
            catch (Exception e)
            {
                if (e.Message.Length == 6)
                    return BadRequest("UPU" + e.Message);
                else
                    return BadRequest("UPU481713");
            }
        }

        /// <summary>
        /// Validates the provided reset token to ensure it is valid and not expired.
        /// </summary>
        /// <param name="resetToken">Password reset token to validate.</param>
        /// <returns>
        /// An IActionResult indicating whether the token is valid or not. 
        /// Returns a BadRequest if the token is invalid or expired, otherwise returns an Ok result.
        /// </returns>
        [HttpGet("validate-reset-token/{resetToken}")]
        [Authorize(AuthenticationSchemes = "ApiKey")]
        public async Task<IActionResult> ValidateResetToken(String resetToken)
        {
            try
            {
                if ((string.IsNullOrWhiteSpace(resetToken) == true) || (resetToken.Length != settings.Value.ResetPasswordTokenLength))
                    throw new Exception("278587");

                var response = await userService.ValidateResetToken(resetToken);
                return Ok(response);
            }
            catch (Exception e)
            {
                if (e.Message.Length == 6)
                    return BadRequest("VRT" + e.Message);
                else
                    return BadRequest("VRT481713");
            }
        }
    }
}
