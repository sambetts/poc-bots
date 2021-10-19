
using Newtonsoft.Json;
using System;

namespace RickrollBot.Model.Models
{
    /// <summary>
    /// Class JoinURLResponse.
    /// </summary>
    public partial class JoinURLResponse
    {
        /// <summary>
        /// Gets or sets the call identifier.
        /// </summary>
        /// <value>The call identifier.</value>
        [JsonProperty("callId")]
        public object CallId { get; set; }

        /// <summary>
        /// Gets or sets the scenario identifier.
        /// </summary>
        /// <value>The scenario identifier.</value>
        [JsonProperty("scenarioId")]
        public Guid ScenarioId { get; set; }

        /// <summary>
        /// Gets or sets the call.
        /// </summary>
        /// <value>The call.</value>
        [JsonProperty("call")]
        public string Call { get; set; }

        [JsonProperty("changeScreenSharingRole")]
        public string ChangeScreenSharingRole { get; set; }
    }
}
