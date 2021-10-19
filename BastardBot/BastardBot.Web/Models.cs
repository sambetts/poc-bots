using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BastardBot.Web
{
    public class ContactUs
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }

        public string RecaptchaValue { get; set; }

        [JsonIgnore]
        public bool IsValid
        {
            get
            {
                return !string.IsNullOrEmpty(this.Name) && 
                    !string.IsNullOrEmpty(this.Email) &&
                    !string.IsNullOrEmpty(this.Message) &&
                    !string.IsNullOrEmpty(this.RecaptchaValue);
            }
        }
    }

    public class OperationResult
    {
        public string Status { get; set; }

        public static OperationResult GetOk() { return new OperationResult { Status = "success" }; }
    }
}
