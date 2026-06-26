using Copower_API.Models.User;
using Copower_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Copower_API.Controllers
{
    /// <summary>
    /// Dashboard controller
    /// </summary>
    /// <param name="dashboardService">Dashboard service component</param>
    /// <param name="utilsService">Utils service component</param>
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController(IDashboardService dashboardService, IUtilsService utilsService) : ControllerBase
    {
        /// <summary>
        /// Get dashboard sensors
        /// </summary>
        /// <returns>Return sensors for the user's dashboard</returns>
        [HttpGet("sensors/get")]
        [Authorize(AuthenticationSchemes = "ApiKeyAndJwt")]
        public async Task<IActionResult> GetDashboardSensors()
        {
            try
            {
                var userId = utilsService.CheckAuthorization(Request);

                var sensors = await dashboardService.GetDashboardSensors(userId);

                return Ok(sensors);
            }
            catch (Exception e)
            {
                if (e.Message.Length == 6)
                    return BadRequest("DSGT" + e.Message);
                else
                    return BadRequest("DSGT481713");
            }
        }

        /// <summary>
        /// Get default or user dashboard
        /// </summary>
        /// <returns></returns>
        [HttpGet("get/{dashboardType:regex(^(default|public|user)$)}")]
        [Authorize(AuthenticationSchemes = "ApiKeyAndJwt")]
        public async Task<IActionResult> GetDefaultOrUserDashboard(string dashboardType)
        {
            try
            {
                var userId = utilsService.CheckAuthorization(Request);

                var dashboard = await dashboardService.GetDashboard(dashboardType, userId);

                return Ok(dashboard);
            }
            catch (Exception e)
            {
                if (e.Message.Length == 6)
                    return BadRequest("DSGD" + e.Message);
                else
                    return BadRequest("DSGD481713");
            }
        }

        /// <summary>
        /// Retrieves the HMI dashboard data for the authenticated user.
        /// </summary>
        /// <remarks>This method requires authentication using both API key and JWT schemes. The user must
        /// be authorized to access the dashboard data. If an error occurs during processing, the response will include
        /// a specific error code in the BadRequest result.</remarks>
        /// <returns>An IActionResult containing the HMI dashboard data if the request is successful; otherwise, a BadRequest
        /// result indicating an error.</returns>
        [HttpGet("get/hmi")]
        [Authorize(AuthenticationSchemes = "ApiKeyAndJwt")]
        public async Task<IActionResult> GetHMI()
        {
            try
            {
                var userId = utilsService.CheckAuthorization(Request);

                var dashboard = await dashboardService.GetHMIDashboard(userId);

                return Ok(dashboard);
            }
            catch (Exception e)
            {
                if (e.Message.Length == 6)
                    return BadRequest("DSHI" + e.Message);
                else
                    return BadRequest("DSHI481713");
            }
        }

        /// <summary>
        /// Get public dashboard
        /// </summary>
        /// <returns></returns>
        [HttpGet("get/public")]
        [Authorize(AuthenticationSchemes = "ApiKey")]
        public async Task<IActionResult> GetPublicDashboard()
        {
            try
            {
                var dashboard = await dashboardService.GetDashboard("public", null);

                return Ok(dashboard);
            }
            catch (Exception e)
            {
                if (e.Message.Length == 6)
                    return BadRequest("DSGD" + e.Message);
                else
                    return BadRequest("DSGD481713");
            }
        }

        /// <summary>
        /// Update dashboard
        /// </summary>
        /// <param name="dashboardType">Dashboard type (default, public, user)</param>
        /// <param name="dashboard">New dashboard</param>
        /// <returns></returns>
        [HttpPost("update/{dashboardType:regex(^(default|public|user)$)}")]
        [Authorize(AuthenticationSchemes = "ApiKeyAndJwt")]
        public async Task<IActionResult> UpdateDashboard(string dashboardType, [FromBody] List<DashboardUIEdit> dashboard)
        {
            try
            {
                if (dashboard.Count > 50)
                    return BadRequest("590283");

                var userId = utilsService.CheckAuthorization(Request);
                var result = await dashboardService.UpdateDashboard(dashboardType, dashboard, userId);

                return Ok(result);
            }
            catch (Exception e)
            {
                if (e.Message.Length == 6)
                    return BadRequest("UDSB" + e.Message);
                else
                    return BadRequest("UDSB481713");
            }
        }
    }
}



