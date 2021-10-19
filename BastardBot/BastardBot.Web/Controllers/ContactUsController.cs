using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Mail;
using System.Threading.Tasks;

namespace BastardBot.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ContactUsController
    {
        private SystemSettings _systemSettings;
        public ContactUsController(SystemSettings systemSettings)
        {
            _systemSettings = systemSettings;
        }

        [HttpPost]
        public async Task<OperationResult> SendEmail(ContactUs contact)
        {
            if (contact.IsValid)
            {

                var httpClient = new HttpClient();

                var uri = $"https://www.google.com/recaptcha/api/siteverify?secret={_systemSettings.RecaptchaSecret}&response={contact.RecaptchaValue}";
                var response = await httpClient.GetAsync(uri);
                response.EnsureSuccessStatusCode();

                string recaptureValidationResponse = await response.Content.ReadAsStringAsync();

                dynamic obj = JsonConvert.DeserializeObject(recaptureValidationResponse);
                var validCapture = (bool)obj.SelectToken("success");

                if (validCapture)
                {

                    var email = new MailMessage(_systemSettings.EmailAddress, _systemSettings.EmailAddress);
                    email.Body = contact.Message;
                    email.Subject = $"BastardBot Comment from {contact.Email}";
                    email.IsBodyHtml = false;
                    if (_systemSettings.SendEmails)
                    {
                        await SendEmail(email, _systemSettings);
                    }
                    else
                    {
                        Console.WriteLine("Not sending email as it's disabled.");
                    }
                    return OperationResult.GetOk();
                }
                else
                {
                    return new OperationResult { Status = "Invalid Recaptcha Value" };
                }
            }
            else
            {
                return new OperationResult { Status = "Invalid contact details" };
            }

        }


        private static async Task SendEmail(MailMessage email, SystemSettings config)
        {
            var client = new SmtpClient(config.EmailSMTP, config.EmailPort)
            {
                Credentials = new System.Net.NetworkCredential(config.EmailAddress, config.EmailPassword),
                EnableSsl = true
            };

            await client.SendMailAsync(email);
        }
    }
}
