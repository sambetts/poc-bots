
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Graph;
using Microsoft.Graph.Communications.Common.Telemetry;
using Microsoft.Graph.Communications.Core.Serialization;
using RickrollBot.Model.Constants;
using RickrollBot.Model.Models;
using RickrollBot.Services.Contract;
using RickrollBot.Services.ServiceSetup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RickrollBot.Services.Http.Controllers
{
    /// <summary>
    /// JoinCallController is a third-party controller (non-Bot Framework) that can be called in CVI scenario to trigger the bot to join a call.
    /// </summary>
    [ApiController]
    [Route("")]
    public class JoinCallController : ControllerBase
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


        /// <summary>
        /// Initializes a new instance of the <see cref="JoinCallController" /> class.

        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="botService">The bot service.</param>
        /// <param name="settings">The settings.</param>
        public JoinCallController(IGraphLogger logger, IBotService botService, IAzureSettings settings)
        {
            _logger = logger;
            _botService = botService;
            _settings = (AzureSettings)settings;
        }

        /// <summary>
        /// The join call async.
        /// </summary>
        /// <param name="joinCallBody">The join call body.</param>
        /// <returns>The <see cref="IActionResult" />.</returns>
        [HttpPost(HttpRouteConstants.JoinCall)]
        public async Task<IActionResult> JoinCallAsync([FromBody] JoinCallBody joinCallBody)
        {
            try
            {
                var call = await _botService.JoinCallAsync(joinCallBody).ConfigureAwait(false);
                var callPath = $"/{HttpRouteConstants.CallRoute.Replace("{callLegId}", call.Id)}";
                var callUri = $"{_settings.ServiceCname}{callPath}";
                _logger.Info($"{nameof(JoinCallAsync)} - Call.id = {call.Id}");

                var values = new JoinURLResponse()
                {
                    Call = callUri,
                    CallId = call.Id,
                    ScenarioId = call.ScenarioId,
                    ChangeScreenSharingRole = callUri + "/" + HttpRouteConstants.OnChangeRoleRoute
                };

                var serializer = new CommsSerializer(pretty: true);
                var json = serializer.SerializeObject(values);
                return Content(json, "application/json", Encoding.UTF8);
            }
            catch (ServiceException e)
            {
                var statusCode = e.ResponseStatusCode >= 300 ? e.ResponseStatusCode : 500;

                if (e.ResponseHeaders != null)
                {
                    foreach (var responseHeader in e.ResponseHeaders)
                    {
                        Response.Headers[responseHeader.Key] = new StringValues(responseHeader.Value.ToArray());
                    }
                }

                return StatusCode(statusCode, e.ToString());
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Received HTTP {Request.Method}, {Request.Path}");
                return StatusCode(500, e.Message);
            }
        }
    }
}
