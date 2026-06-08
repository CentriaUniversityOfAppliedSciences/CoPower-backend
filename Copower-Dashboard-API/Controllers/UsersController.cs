using Copower_API.Models.User;
using Copower_API.Models.Users;
using Copower_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Copower_API.Controllers
{
    /// <summary>
    /// User controller
    /// </summary>
    /// <param name="emailService">Email service component</param>
    /// <param name="usersService">Users service component</param>
    /// <param name="utilsService">Utils service component</param>
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "ApiKeyAndJwt")]
    [ApiController]
    public class UsersController(IEmailService emailService, IUsersService usersService, IUtilsService utilsService) : ControllerBase
    {
        /// <summary>
        /// Add new user to the system.
        /// </summary>
        /// <param name="add">User add model</param>
        /// <param name="orgId">Organisation ID</param>
        /// <returns></returns>
        [HttpPut("add/{orgId}")]
        public async Task<ActionResult<Boolean>> Add([FromBody] UsersAdd add, Guid orgId)
        {
            try
            {
                if (utilsService.CheckUUID(orgId) == false)
                    throw new Exception("575186");

                await usersService.Add(utilsService.CheckAuthorization(Request), orgId, add);

                return Ok();
            }
            catch (Exception e)
            {
                if (e.Message.Length == 6)
                    return BadRequest("AAUU" + e.Message);
                else
                    return BadRequest("AAUU696660");
            }
        }

        /// <summary>
        /// Delete an user
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpDelete("{orgId}/{userId}")]
        public async Task<ActionResult<Boolean>> Delete(Guid orgId, Guid userId)
        {
            try
            {
                if (utilsService.CheckUUID(orgId) == false)
                    throw new Exception("575186");

                if (utilsService.CheckUUID(userId) == false)
                    throw new Exception("949913");

                await usersService.Delete(utilsService.CheckAuthorization(Request), orgId, userId);

                return Ok();
            }
            catch (Exception e)
            {
                if (e.Message.Length == 6)
                    return BadRequest("DAUU" + e.Message);
                else
                    return BadRequest("DAUU696660");
            }
        }

        /// <summary>
        /// Edit an existing user in the system.
        /// </summary>
        /// <param name="edit">Edit user model</param>
        /// <param name="orgId">Organisation ID</param>
        /// <param name="userId">User ID</param>
        /// <returns></returns>
        [HttpPut("edit/{orgId}/{userId}")]
        public async Task<ActionResult<Boolean>> Edit([FromBody] UsersEditAdmin edit, Guid orgId, Guid userId)
        {
            try
            {
                if (utilsService.CheckUUID(orgId) == false)
                    throw new Exception("575186");

                if (utilsService.CheckUUID(userId) == false)
                    throw new Exception("949913");

                await usersService.Edit(utilsService.CheckAuthorization(Request), orgId, userId, edit);

                return Ok();
            }
            catch (Exception e)
            {
                if (e.Message.Length == 6)
                    return BadRequest("EAUU" + e.Message);
                else
                    return BadRequest("EAUU696660");
            }
        }

        /// <summary>
        /// Email service component
        /// </summary>
        public IEmailService EmailService { get; } = emailService;

        /// <summary>
        /// Retrieves all users in the system.
        /// </summary>
        /// <returns>A list of all users as UserViewModel objects.</returns>
        [HttpGet("")]
        public async Task<ActionResult<IEnumerable<UserViewModel>>> GetAllUsers()
        {
            try
            {
                var userId = utilsService.CheckAuthorization(Request);

                var users = await usersService.GetAllUsers(userId);
                return Ok(users);
            }
            catch (Exception e)
            {
                if (e.Message.Length == 6)
                    return BadRequest("GAUU" + e.Message);
                else
                    return BadRequest("GAUU696660");
            }
        }

        /// <summary>
        /// Resend registration invitation to user
        /// </summary>
        /// <returns>A list of all users as UserViewModel objects.</returns>
        [HttpPost("resend-invitation/{resendUserId}")]
        public async Task<ActionResult<Boolean>> ResendInvitation(Guid resendUserId)
        {
            try
            {
                var userId = utilsService.CheckAuthorization(Request);

                var users = await usersService.ResendInvitation(userId, resendUserId);
                return Ok(users);
            }
            catch (Exception e)
            {
                if (e.Message.Length == 6)
                    return BadRequest("URI" + e.Message);
                else
                    return BadRequest("URI696660");
            }
        }
    }
}
