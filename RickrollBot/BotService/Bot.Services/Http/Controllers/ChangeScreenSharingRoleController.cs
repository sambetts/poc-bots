using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Communications.Common.Telemetry;
using RickrollBot.Model.Constants;
using RickrollBot.Services.Bot;
using RickrollBot.Services.Contract;
using RickrollBot.Services.ServiceSetup;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Bot.Services.Http.Controllers
{
    /// <summary>
    /// ChangeScreenSharingRoleController is a third-party controller (non-Bot Framework) that changes the bot's screen sharing role.
    /// </summary>
    [ApiController]
    [Route("")]
    public class ChangeScreenSharingRoleController : ControllerBase
    {
        /// <summary>
        /// The logger
        /// </summary>
        private readonly IGraphLogger _logger;
        /// <summary>
        /// The bot service
        /// </summary>
        private readonly IBotService _botService;
        /// <summary>
        /// The settings
        /// </summary>
        private readonly AzureSettings _settings;

        public ChangeScreenSharingRoleController()
        {
            _logger = AppHost.AppHostInstance.Resolve<IGraphLogger>();
            _botService = AppHost.AppHostInstance.Resolve<IBotService>();
            _settings = AppHost.AppHostInstance.Resolve<IOptions<AzureSettings>>().Value;
        }

        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="eventPublisher">The event publisher.</param>
        /// <param name="botService">The bot service.</param>
        /// <param name="settings">The settings.</param>
        public ChangeScreenSharingRoleController(IGraphLogger logger, IBotService botService, IAzureSettings settings)
        {
            _logger = logger;
            _botService = botService;
            _settings = (AzureSettings)settings;
        }

        /// <summary>
        /// Changes screen sharing role.
        /// </summary>
        /// <param name="callLegId">
        /// The call leg identifier.
        /// </param>
        /// <param name="changeRoleBody">
        /// The role to change to.
        /// </param>
        /// <returns>
        /// The <see cref="IActionResult"/>.
        /// </returns>
        [HttpPost(HttpRouteConstants.CallRoute + "/" + HttpRouteConstants.OnChangeRoleRoute)]
        public async Task<IActionResult> ChangeScreenSharingRoleAsync(string callLegId, [FromBody] ChangeRoleBody changeRoleBody)
        {
            if (changeRoleBody == null)
            {
                return BadRequest();
            }
            try
            {
                await _botService.ChangeSharingRoleAsync(callLegId, changeRoleBody.Role).ConfigureAwait(false);
                return Ok();
            }
            catch (Exception e)
            {
                return StatusCode(500, e.ToString());
            }
        }

        /// <summary>
        /// Request body content to update screen sharing role.
        /// </summary>
        public class ChangeRoleBody
        {
            /// <summary>
            /// Gets or sets the role.
            /// </summary>
            /// <value>
            /// The role to change to.
            /// </value>
            public ScreenSharingRole Role { get; set; }
        }
    }
}
