using Copower_API.Models.Measurements;
using Copower_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Models.Security;

namespace Copower_API.Controllers
{
    /// <summary>
    /// Measurement data controller
    /// </summary>
    /// <remarks>
    /// Measurement data controller constructor
    /// </remarks>
    [Route("api/[controller]")]
    [ApiController]
    public class MeasurementsController(IMeasurementsService measurementsService, IUtilsService utilsService) : ControllerBase
    {
        /// <summary>
        /// Retrieves the Human-Machine Interface (HMI) measurements for the currently authorized user.
        /// </summary>
        /// <remarks>This method requires the caller to be authorized. If authorization fails or an error
        /// occurs during retrieval, a Bad Request response is returned with a specific error code in the
        /// message.</remarks>
        /// <returns>An <see cref="IActionResult"/> that contains the HMI measurements if the request is successful. Returns a
        /// 200 OK response with the measurements on success; otherwise, returns a 400 Bad Request response with an
        /// error message.</returns>
        [HttpGet("hmi/get")]
        [Authorize(AuthenticationSchemes = "ApiKeyAndJwt")]
        public async Task<IActionResult> HMIGet()
        {
            try
            {
                var userId = utilsService.CheckAuthorization(Request);

                var measurements = await measurementsService.GetHMI(userId);

                return Ok(measurements);
            }
            catch (Exception e)
            {
                if (e.Message.Length == 6)
                    return BadRequest("HMG" + e.Message);
                else
                    return BadRequest("HMG481713");
            }
        }

        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        /// <summary>
        /// Get measurements
        /// </summary>
        /// <param name="model">Start and end time of the required measurement data</param>
        /// <param name="sensor">Sensor Id</param>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = "ApiKeyAndJwt")]
        [HttpPost("{sensor:Guid}")]
        public async Task<IActionResult> MeasurementsGet([FromBody] MeasurementsGetModel model, Guid sensor)
        {
            try
            {
                var userId = utilsService.CheckAuthorization(Request);

                var measurements = await measurementsService.GetMeasurements(userId, sensor, model.StartTime, model.EndTime);

                return Ok(measurements);
            }
            catch (Exception e)
            {
                if (e.Message.Length == 6)
                    return BadRequest("MEG" + e.Message);
                else
                    return BadRequest("MEG481713");
            }
        }

        /// <summary>
        /// Save measurements
        /// </summary>
        /// <param name="model">Array of measurement data to save</param>
        /// <param name="apikey">API key for authentication</param>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = "ApiKey")]
        [HttpPost("save/{apikey}")]
        public async Task<IActionResult> Save([FromBody] List<MeasurementsSaveModel> model, String apikey)
        {
            try
            {
                var save = await measurementsService.SaveMeasurements(apikey, model);

                return Ok(save);
            }
            catch (Exception e)
            {
                if (e.Message.Length == 6)
                    return BadRequest("MES" + e.Message);
                else
                    return BadRequest("MES481713");
            }
        }
    }
}
