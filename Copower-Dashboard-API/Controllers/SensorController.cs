using Copower_API.Helpers;
using Copower_API.Models.Sensor;
using Copower_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Copower_API.Controllers
{
    /// <summary>
    /// Sensor Controller
    /// </summary>
    /// <remarks>
    /// Sensor Controller Initialisation
    /// </remarks>
    /// <param name="configuration">Configuration</param>
    /// <param name="generalService">General service</param>
    /// <param name="sensorService">Sensor service</param>
    /// <param name="utilsService">Utils service</param>
    /// <param name="settings">Application settings</param>
    [Authorize(AuthenticationSchemes = "ApiKeyAndJwt")]
    [Route("api/[controller]")]
    [ApiController]
    public class SensorController(IConfiguration configuration, IGeneralService generalService, ISensorService sensorService, IUtilsService utilsService, IOptions<Settings> settings) : ControllerBase
    {
        private readonly IConfiguration _configuration = configuration;
        private readonly IGeneralService _generalService = generalService;
        private readonly ISensorService _sensorService = sensorService;
        private readonly IUtilsService _utilsService = utilsService;

        /// <summary>
        /// Add a new sensor
        /// </summary>
        /// <param name="sensorData">Sensor data</param>
        /// <param name="orgId">Organisation Id</param>
        /// <returns></returns>
        [HttpPut("add/{orgId:Guid}")]
        public async Task<IActionResult> Add([FromBody] SensorAddEditModel sensorData, Guid orgId)
        {
            try
            {
                if ((String.IsNullOrWhiteSpace(sensorData.Name) == true) || (sensorData.Name.Length > settings.Value.InputMax.Name))
                    throw new Exception("951538");

                if ((String.IsNullOrWhiteSpace(sensorData.Source) == true) || (sensorData.Source.Split(".").Length != 2) || (sensorData.Source.Length > settings.Value.InputMax.StringLong))
                    throw new Exception("951538");

                if ((sensorData.Unit == null) || (sensorData.Unit.Length > settings.Value.InputMax.StringShort))
                    throw new Exception("951538");

                var userId = _utilsService.CheckAuthorization(Request);

                var response = await _sensorService.Add(sensorData, userId, orgId);
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

        /// <summary>
        /// Delete a sensor
        /// </summary>
        /// <param name="orgId">Organisation Id</param>
        /// <param name="sensorId">Sensor Id</param>
        /// <returns></returns>
        [HttpDelete("{orgId}/{sensorId}")]
        public async Task<IActionResult> Delete(Guid orgId, Guid sensorId)
        {
            try
            {
                var userId = _utilsService.CheckAuthorization(Request);

                var delete = await _sensorService.Delete(userId, orgId, sensorId);

                return Ok(delete);
            }
            catch (Exception e)
            {
                if (e.Message.Length == 6)
                    return BadRequest("DSNR" + e.Message);
                else
                    return BadRequest("DSNR481713");
            }
        }

        /// <summary>
        /// Edit a sensor
        /// </summary>
        /// <param name="sensorData">Sensor data</param>
        /// <param name="orgId">Organisation Id</param>
        /// <param name="sensorId">Sensor Id</param>
        /// <returns></returns>
        [HttpPut("edit/{orgId:Guid}/{sensorId:Guid}")]
        public async Task<IActionResult> Edit([FromBody] SensorAddEditModel sensorData, Guid orgId, Guid sensorId)
        {
            try
            {
                if ((String.IsNullOrWhiteSpace(sensorData.Name) == true) || (sensorData.Name.Length > settings.Value.InputMax.Name))
                    throw new Exception("951538");

                if ((String.IsNullOrWhiteSpace(sensorData.Source) == true) || (sensorData.Source.Split(".").Length != 2) || (sensorData.Source.Length > settings.Value.InputMax.StringLong))
                    throw new Exception("951538");

                if ((sensorData.Unit == null) || (sensorData.Unit.Length > settings.Value.InputMax.StringShort))
                    throw new Exception("951538");

                if (sensorData.ValueChange <= 0.000000001 || sensorData.ValueChange >= 100000000)
                    sensorData.ValueChange = 1;

                var userId = _utilsService.CheckAuthorization(Request);

                var sensors = await _sensorService.Edit(sensorData, userId, orgId, sensorId);
                return Ok(sensors);
            }
            catch (Exception e)
            {
                if (e.Message.Length == 6)
                    return BadRequest("ESNR" + e.Message);
                else
                    return BadRequest("ESNR481713");
            }
        }

        /// <summary>
        /// Get list of sensors for the user
        /// </summary>
        /// <returns>List of sensors</returns>
        [HttpGet()]
        public async Task<IActionResult> Get()
        {
            try
            {
                var userId = _utilsService.CheckAuthorization(Request);

                var sensors = await _sensorService.Get(userId);
                return Ok(sensors);
            }
            catch (Exception e)
            {
                if (e.Message.Length == 6)
                    return BadRequest("GSNR" + e.Message);
                else
                    return BadRequest("GSNR481713");
            }
        }

        /// <summary>
        /// Get list of available sensors
        /// </summary>
        /// <param name="organisation">Organisation Id</param>
        /// <returns>List of sensors</returns>
        [AllowAnonymous]
        [HttpGet("{organisation:Guid}/list")]
        public async Task<IActionResult> GetList(Guid organisation)
        {
            try
            {
                var userId = _utilsService.CheckAuthorization(Request);

                var sensors = await _sensorService.GetList(organisation, userId);
                return Ok(sensors);
            }
            catch (Exception e)
            {
                if (e.Message.Length == 6)
                    return BadRequest("GLP" + e.Message);
                else
                    return BadRequest("GLP481713");
            }
        }

        /// <summary>
        /// Get sensors for edit
        /// </summary>
        /// <returns></returns>
        [HttpGet("edit/{dashboardType:regex(^(default|public|user)$)}")]
        public async Task<IActionResult> GetEdit(string dashboardType)
        {
            try
            {
                var userId = _utilsService.CheckAuthorization(Request);

                var sensors = await _sensorService.GetEdit(userId, dashboardType);
                return Ok(sensors);
            }
            catch (Exception e)
            {
                if (e.Message.Length == 6)
                    return BadRequest("GESR" + e.Message);
                else
                    return BadRequest("GESR481713");
            }
        }
    }
}
