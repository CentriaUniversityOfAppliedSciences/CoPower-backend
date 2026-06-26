using Azure;
using Copower_API.Helpers;
using Copower_API.Models.Measurements;
using Copower_API.Models.Sensor;
using Copower_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Copower_API.Controllers
{
    /// <summary>
    /// Public routes
    /// </summary>
    /// <param name="publicService">Public service</param>
    [Route("api/[controller]")]
    [ApiController]
    public class PublicController(IPublicService publicService) : ControllerBase
    {
        /// <summary>
        /// Get public dashboard configuration
        /// </summary>
        /// <returns>Public dashboard configuration</returns>
        [HttpGet("dashboard/get")]
        [AllowAnonymous]
        public async Task<IActionResult> Get()
        {
            try
            {
                var response = await publicService.GetPublicDashboard();
                return Ok(response);
            }
            catch (Exception e)
            {
                if (e.Message.Length == 6)
                    return BadRequest("ASNR" + e.Message);
                else
                    return BadRequest("ASNR481713");
            }
        }

        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        /// <summary>
        /// Get measurements
        /// </summary>
        /// <param name="model">Start and end time of the required measurement data</param>
        /// <param name="sensor">Sensor Id</param>
        /// <returns></returns>
        [HttpPost("measurements/{sensor:Guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetMeasurements([FromBody] MeasurementsGetModel model, Guid sensor)
        {
            try
            {
                var measurements = await publicService.GetMeasurements(sensor, model.StartTime, model.EndTime);

                return Ok(measurements);
            }
            catch (Exception e)
            {
                if (e.Message.Length == 6)
                    return BadRequest("MEG" + e.Message);
                else
                {
                    return BadRequest("MEG481713");
                }
            }
        }
    }
}
