using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace RickrollBot.Services.Http
{
    /// <summary>
    /// Extension methods for converting ASP.NET Core HttpRequest to HttpRequestMessage
    /// for compatibility with Bot Framework Graph SDK
    /// </summary>
    public static class HttpRequestExtensions
    {
        /// <summary>
        /// Converts ASP.NET Core HttpRequest to HttpRequestMessage for Bot Framework SDK compatibility
        /// </summary>
        public static async Task<HttpRequestMessage> ToHttpRequestMessageAsync(this HttpRequest request)
        {
            var httpRequest = new HttpRequestMessage
            {
                Method = new HttpMethod(request.Method),
                RequestUri = new Uri(request.GetDisplayUrl()),
            };

            // Copy headers
            foreach (var header in request.Headers)
            {
                if (!httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()))
                {
                    // If header can't be added to request headers, try content headers later
                }
            }

            // Copy content
            if (request.ContentLength > 0 || request.Headers.ContainsKey("Transfer-Encoding"))
            {
                var memoryStream = new MemoryStream();
                await request.Body.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                httpRequest.Content = new StreamContent(memoryStream);

                // Copy content headers
                if (request.ContentType != null)
                {
                    httpRequest.Content.Headers.TryAddWithoutValidation("Content-Type", request.ContentType);
                }

                if (request.ContentLength.HasValue)
                {
                    httpRequest.Content.Headers.TryAddWithoutValidation("Content-Length", request.ContentLength.Value.ToString());
                }

                foreach (var header in request.Headers.Where(h => 
                    h.Key.StartsWith("Content-", StringComparison.OrdinalIgnoreCase) && 
                    h.Key != "Content-Type" && 
                    h.Key != "Content-Length"))
                {
                    httpRequest.Content.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                }
            }

            return httpRequest;
        }
    }
}
