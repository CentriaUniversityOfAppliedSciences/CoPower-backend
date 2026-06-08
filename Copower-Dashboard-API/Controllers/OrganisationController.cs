using Copower_API.Entities;
using Copower_API.Models.Organisation;
using Copower_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph.Models;

namespace Copower_API.Controllers
{
    /// <summary>
    /// Organisation controller
    /// </summary>
    /// <remarks>
    /// Organisation controller constructor
    /// </remarks>
    /// <param name="organisationService">Organisation service</param>
    /// <param name="utilsService">Utils service</param>
    [Authorize(AuthenticationSchemes = "ApiKeyAndJwt")]
    [Route("api/[controller]")]
    [ApiController]
    public class OrganisationController(IOrganisationService organisationService, IUtilsService utilsService) : ControllerBase
    {
        /// <summary>
        /// Add new organisation
        /// </summary>
        /// <param name="organisation">Organisation name</param>
        /// <returns></returns>
        [HttpPost("add")]
        public async Task<IActionResult> AddOrganisation([FromBody] OrganisationAdd organisation)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(organisation.Name) || (organisation.Name.Length > 40))
                    throw new Exception("922712");

                var userId = utilsService.CheckAuthorization(Request);

                var orgs = await organisationService.AddNew(userId, organisation);
                return Ok(orgs);
            }
            catch (Exception e)
            {
                if (e.Message.Length == 6)
                    return BadRequest("AOR" + e.Message);
                else
                    return BadRequest("AOR696660");
            }
        }

        /// <summary>
        /// Delete organisation
        /// </summary>
        /// <param name="org">Organisation id</param>
        /// <returns></returns>
        [HttpDelete("delete")]
        public IActionResult DeleteOrganisation(Guid org)
        {
            try
            {
                if (utilsService.CheckUUID(org) == false)
                    throw new Exception("303086");

                var userId = utilsService.CheckAuthorization(Request);

                organisationService.Delete(userId, org);

                return Ok();
            }
            catch (Exception e)
            {
                if (e.Message.Length == 6)
                    return BadRequest("DOR" + e.Message);
                else
                    return BadRequest("DOR696660");
            }
        }

        /// <summary>
        /// Edit organisation
        /// </summary>
        /// <param name="edit">Organisation edit data</param>
        /// <returns></returns>
        [HttpPut("edit")]
        public async Task<IActionResult> EditOrganisation([FromBody] OrganisationEdit edit)
        {
            try
            {
                if (utilsService.CheckUUID(edit.Id) == false)
                    throw new Exception("303086");

                if (string.IsNullOrWhiteSpace(edit.Name) || (edit.Name.Length > 40))
                    throw new Exception("922712");

                var userId = utilsService.CheckAuthorization(Request);

                organisationService.Edit(userId, edit);

                return Ok();
            }
            catch (Exception e)
            {
                if (e.Message.Length == 6)
                    return BadRequest("EOR" + e.Message);
                else
                    return BadRequest("EOR696660");
            }
        }

        /// <summary>
        /// Get list of available organisations
        /// </summary>
        [AllowAnonymous]
        [HttpGet("list")]
        public async Task<IActionResult> GetOrganisation()
        {
            try
            {
                var userId = utilsService.CheckAuthorization(Request);

                var orgs = await organisationService.GetList(userId);
                return Ok(orgs);
            }
            catch (Exception e)
            {
                if (e.Message.Length == 6)
                    return BadRequest("GOR" + e.Message);
                else
                    return BadRequest("GOR481713");
            }
        }

        /// <summary>
        /// Get list of available organisations
        /// </summary>
        /// <returns>List of organisations</returns>
        [HttpGet("init")]
        public async Task<IActionResult> Init()
        {
            try
            {
                var userId = utilsService.CheckAuthorization(Request);

                var orgs = await organisationService.GetInit(userId);
                return Ok(orgs);
            }
            catch (Exception e)
            {
                if (e.Message.Length == 6)
                    return BadRequest("GOI" + e.Message);
                else
                    return BadRequest("GOI481713");
            }
        }

        /// <summary>
        /// Update organisation
        /// </summary>
        /// <param name="model">Organisation update data</param>
        /// <returns></returns>
        [HttpPost("update")]
        public async Task<IActionResult> UpdateOrganisation(OrganisationUpdate model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.Name) || (model.Name.Length > 40))
                    throw new Exception("922712");

                var userId = utilsService.CheckAuthorization(Request);

                await organisationService.Update(userId, model);

                return Ok();
            }
            catch (Exception e)
            {
                if (e.Message.Length == 6)
                    return BadRequest("EOR" + e.Message);
                else
                    return BadRequest("EOR696660");
            }
        }
    }
}
