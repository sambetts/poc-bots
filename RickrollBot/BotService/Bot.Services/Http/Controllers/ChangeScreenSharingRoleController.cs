using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Communications.Common.Telemetry;
using RickrollBot.Model.Constants;
using RickrollBot.Services.Bot;
using RickrollBot.Services.Contract;
using RickrollBot.Services.ServiceSetup;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Bot.Services.Http.Controllers
{
    /// <summary>
    /// ChangeScreenSharingRoleController is a third-party controller (non-Bot Framework) that changes the bot's screen sharing role.
    /// </summary>
    public class ChangeScreenSharingRoleController : ApiController
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
        /// The <see cref="HttpResponseMessage"/>.
        /// </returns>
        [HttpPost]
        [Route(HttpRouteConstants.CallRoute + "/" + HttpRouteConstants.OnChangeRoleRoute)]
        public async Task<HttpResponseMessage> ChangeScreenSharingRoleAsync(string callLegId, [FromBody] ChangeRoleBody changeRoleBody)
        {
            if (changeRoleBody == null)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            try
            {
                await _botService.ChangeSharingRoleAsync(callLegId, changeRoleBody.Role).ConfigureAwait(false);
                return this.Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return e.InspectExceptionAndReturnResponse();
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
