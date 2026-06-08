using Copower_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Copower_API.Controllers
{
    /// <summary>
    /// API controller
    /// </summary>
    /// <param name="utilsService">Utils services</param>
    /// <param name="apiService">API services</param>
    [Authorize(AuthenticationSchemes = "ApiKeyAndJwt")]
    [Route("api/[controller]")]
    [ApiController]
    public class APIController(IUtilsService utilsService, IAPIService apiService) : ControllerBase
    {
        /// <summary>
        /// Create a new API key for user's own organisation
        /// </summary>
        /// <returns></returns>
        [HttpPost("add")]
        public async Task<IActionResult> AddToUserOrganisation()
        {
            try
            {
                var userId = utilsService.CheckAuthorization(Request);
                var data = await apiService.Add(userId, null);

                return Ok(data);
            }
            catch (Exception e)
            {
                if (e.Message.Length == 6)
                    return BadRequest("APOC" + e.Message);
                else
                    return BadRequest("APOC893451");
            }
        }

        /// <summary>
        /// Create a new API key
        /// </summary>
        /// <param name="orgId">Organisation Id to which create the API key for</param>
        /// <returns></returns>
        [HttpPost("add/{orgId}")]
        public async Task<IActionResult> Add(Guid orgId)
        {
            try
            {
                var userId = utilsService.CheckAuthorization(Request);
                var data = await apiService.Add(userId, orgId);

                return Ok(data);
            }
            catch (Exception e)
            {
                if (e.Message.Length == 6)
                    return BadRequest("APC" + e.Message);
                else
                    return BadRequest("APC893451");
            }
        }

        /// <summary>
        /// Delete a API key
        /// </summary>
        /// <param name="apikey">Api key to delete</param>
        /// <returns></returns>
        [HttpDelete("delete/{apikey}")]
        public async Task<IActionResult> Delete(String apikey)
        {
            try
            {
                var userId = utilsService.CheckAuthorization(Request);
                var data = await apiService.Delete(userId, apikey);

                return Ok(data);
            }
            catch (Exception e)
            {
                if (e.Message.Length == 6)
                    return BadRequest("APD" + e.Message);
                else
                    return BadRequest("APD893451");
            }
        }

        /// <summary>
        /// Gets the initialisation data for API keys (only for appadmin's)
        /// </summary>
        /// <returns>List of API keys</returns>
        [HttpGet("init")]
        public async Task<IActionResult> GetInit()
        {
            try
            {
                var userId = utilsService.CheckAuthorization(Request);
                var data = await apiService.GetInit(userId);

                return Ok(data);
            }
            catch (Exception e)
            {
                if (e.Message.Length == 6)
                    return BadRequest("API" + e.Message);
                else
                    return BadRequest("API893451");
            }
        }

        /// <summary>
        /// Gets the list of API keys
        /// </summary>
        /// <returns>List of API keys</returns>
        [HttpGet("list")]
        public async Task<IActionResult> GetList()
        {
            try
            {
                var userId = utilsService.CheckAuthorization(Request);
                var list = await apiService.GetList(userId);

                return Ok(list);
            }
            catch (Exception e)
            {
                if (e.Message.Length == 6)
                    return BadRequest("APL" + e.Message);
                else
                    return BadRequest("APL893451");
            }
        }
    }
}
