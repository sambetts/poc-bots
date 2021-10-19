
using Bot.Services.Util;
using Microsoft.Skype.Bots.Media;
using RickrollBot.Model.Constants;
using RickrollBot.Services.Contract;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace RickrollBot.Services.ServiceSetup
{
    /// <summary>
    /// Class AzureSettings.
    /// Implements the <see cref="RickrollBot.Services.Contract.IAzureSettings" />
    /// </summary>
    /// <seealso cref="RickrollBot.Services.Contract.IAzureSettings" />
    public class AzureSettings : IAzureSettings
    {
        /// <summary>
        /// Gets or sets the name of the bot.
        /// </summary>
        /// <value>The name of the bot.</value>
        public string BotName { get; set; }

        /// <summary>
        /// Gets or sets the name of the service DNS.
        /// </summary>
        /// <value>The name of the service DNS.</value>
        public string ServiceDnsName { get; set; }

        /// <summary>
        /// Gets or sets the service cname.
        /// </summary>
        /// <value>The service cname.</value>
        public string ServiceCname { get; set; }

        /// <summary>
        /// Gets or sets the certificate thumbprint.
        /// </summary>
        /// <value>The certificate thumbprint.</value>
        public string CertificateThumbprint { get; set; }

        /// <summary>
        /// Gets or sets the call control listening urls.
        /// </summary>
        /// <value>The call control listening urls.</value>
        public IEnumerable<string> CallControlListeningUrls { get; set; }

        /// <summary>
        /// Gets or sets the call control base URL.
        /// </summary>
        /// <value>The call control base URL.</value>
        public Uri CallControlBaseUrl { get; set; }

        /// <summary>
        /// Gets or sets the place call endpoint URL.
        /// </summary>
        /// <value>The place call endpoint URL.</value>
        public Uri PlaceCallEndpointUrl { get; set; }

        /// <summary>
        /// Gets the media platform settings.
        /// </summary>
        /// <value>The media platform settings.</value>
        public MediaPlatformSettings MediaPlatformSettings { get; private set; }

        /// <summary>
        /// Gets or sets the aad application identifier.
        /// </summary>
        /// <value>The aad application identifier.</value>
        public string AadAppId { get; set; }

        public string AadTenantId { get; set; }


        /// <summary>
        /// Gets or sets the aad application secret.
        /// </summary>
        /// <value>The aad application secret.</value>
        public string AadAppSecret { get; set; }

        /// <summary>
        /// Gets or sets the instance public port.
        /// </summary>
        /// <value>The instance public port.</value>
        public int InstancePublicPort { get; set; }

        /// <summary>
        /// Gets or sets the instance internal port.
        /// </summary>
        /// <value>The instance internal port.</value>
        public int InstanceInternalPort { get; set; }

        /// <summary>
        /// Gets or sets the call signaling port.
        /// </summary>
        /// <value>The call signaling port.</value>
        public int CallSignalingPort { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [capture events].
        /// </summary>
        /// <value><c>true</c> if [capture events]; otherwise, <c>false</c>.</value>
        public bool CaptureEvents { get; set; } = false;

        /// <summary>
        /// Gets or sets the name of the pod.
        /// </summary>
        /// <value>The name of the pod.</value>
        public string PodName { get; set; }

        /// <summary>
        /// Gets or sets the media folder.
        /// </summary>
        /// <value>The media folder.</value>
        public string MediaFolder { get; set; }

        /// <summary>
        /// Gets or sets the events folder.
        /// </summary>
        /// <value>The events folder.</value>
        public string EventsFolder { get; set; }

        public string ApplicationInsightsKey { get; set; }

        /// <summary>
        /// Gets or sets the audio settings.
        /// </summary>
        /// <value>The audio settings.</value>
        public AudioSettings AudioSettings { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is stereo.
        /// </summary>
        /// <value><c>true</c> if this instance is stereo; otherwise, <c>false</c>.</value>
        public bool IsStereo { get; set; }

        public Dictionary<string, VideoFormat> H264FileLocations { get; } = new Dictionary<string, VideoFormat>();
        /// <summary>
        /// Gets or sets the wav quality.
        /// </summary>
        /// <value>The wav quality.</value>
        public int WAVQuality { get; set; }

        public string BaseContentDir { get; set; }

        /// <summary>
        /// Gets the h264 1280 x 720 file location.
        /// </summary>
        public string H2641280x720x30FpsFile { get; set; }

        /// <summary>
        /// Gets the h264 640 x 360 file location.
        /// </summary>
        public string H264640x360x30xFpsFile { get; set; }

        /// <summary>
        /// Gets the h264 320 x 180 file location.
        /// </summary>
        public string H264320x180x15FpsFile { get; set; }

        /// <summary>
        /// Backing WAV file for video
        /// </summary>
        public string WavFile { get; set; }

        public string AudioFileLocation { get; set; }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public void Initialize()
        {
            if (string.IsNullOrWhiteSpace(ServiceCname))
            {
                ServiceCname = ServiceDnsName;
            }

            var defaultCertificate = this.GetCertificateFromStore();
            var controlListenUris = new List<string>();

            var baseDomain = "+";
            int podNumber = 0;
            if (!string.IsNullOrEmpty(this.PodName))
            {
                int.TryParse(Regex.Match(this.PodName, @"\d+$").Value, out podNumber);
            }

            /*
             * Files converted with:
             * 
                ./ffmpeg.exe -i "C:\Users\sambetts\Desktop\Rick Astley - Never Gonna Give You Up 4K 60 FPS Remastered.mp4" -g 1 -an -crf 35 -vcodec libx264 -movflags faststart -vf "scale=1280:720, fps=29.97" C:\Users\sambetts\Desktop\"rickroll.1280x720x30.h264"
                ./ffmpeg.exe -i "C:\Users\sambetts\Desktop\Rick Astley - Never Gonna Give You Up 4K 60 FPS Remastered.mp4" -g 1 -an -movflags faststart -vf "scale=640:360, fps=30" "C:\Users\sambetts\Desktop\rickroll.640x360x30.h264"
                ./ffmpeg.exe -i "C:\Users\sambetts\Desktop\Rick Astley - Never Gonna Give You Up 4K 60 FPS Remastered.mp4" -g 1 -an -movflags faststart -vf "scale=320:180, fps=15" "C:\Users\sambetts\Desktop\rickroll.320x180x15.h264"

                ffmpeg GoP needs to be 1 as Teams Video only seems to work with keyframes? Left to default, video stutters in Teams stream. 
             */

            // Audio/Video config
            this.H264FileLocations.Add(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, this.H2641280x720x30FpsFile), VideoFormat.H264_1280x720_30Fps);
            this.H264FileLocations.Add(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, this.H264320x180x15FpsFile), VideoFormat.H264_320x180_15Fps);
            this.H264FileLocations.Add(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, this.H264640x360x30xFpsFile), VideoFormat.H264_640x360_30Fps);
            this.AudioFileLocation = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, WavFile);

            Utilities.ValidateVideos(this);


            // Create structured config objects for service.
            this.CallControlBaseUrl = new Uri($"https://{this.ServiceCname}/{podNumber}/{HttpRouteConstants.CallSignalingRoutePrefix}/{HttpRouteConstants.OnNotificationRequestRoute}");

            controlListenUris.Add($"https://{baseDomain}:{CallSignalingPort}/");
            controlListenUris.Add($"https://{baseDomain}:{CallSignalingPort}/{podNumber}/");
            controlListenUris.Add($"http://{baseDomain}:{CallSignalingPort + 1}/"); // required for AKS pod graceful termination

            this.CallControlListeningUrls = controlListenUris;

            this.MediaPlatformSettings = new MediaPlatformSettings()
            {
                MediaPlatformInstanceSettings = new MediaPlatformInstanceSettings()
                {
                    CertificateThumbprint = defaultCertificate.Thumbprint,
                    InstanceInternalPort = InstanceInternalPort,
                    InstancePublicIPAddress = IPAddress.Any,
                    InstancePublicPort = InstancePublicPort + podNumber,
                    ServiceFqdn = this.ServiceCname
                },
                ApplicationId = this.AadAppId,
            };

            Console.WriteLine($"{nameof(MediaPlatformSettings)}:");
            Console.WriteLine($"{nameof(MediaPlatformSettings.MediaPlatformInstanceSettings.CertificateThumbprint)}: {MediaPlatformSettings.MediaPlatformInstanceSettings.CertificateThumbprint}.");
            Console.WriteLine($"{nameof(MediaPlatformSettings.MediaPlatformInstanceSettings.InstanceInternalPort)}: {MediaPlatformSettings.MediaPlatformInstanceSettings.InstanceInternalPort}.");
            Console.WriteLine($"{nameof(MediaPlatformSettings.MediaPlatformInstanceSettings.InstancePublicPort)}: {MediaPlatformSettings.MediaPlatformInstanceSettings.InstancePublicPort}.");
            Console.WriteLine($"{nameof(MediaPlatformSettings.MediaPlatformInstanceSettings.ServiceFqdn)}: {MediaPlatformSettings.MediaPlatformInstanceSettings.ServiceFqdn}.");
            Console.WriteLine($"{nameof(MediaPlatformSettings.ApplicationId)}: {MediaPlatformSettings.ApplicationId}.");
            Console.WriteLine();

        }

        /// <summary>
        /// Helper to search the certificate store by its thumbprint.
        /// </summary>
        /// <returns>Certificate if found.</returns>
        /// <exception cref="Exception">No certificate with thumbprint {CertificateThumbprint} was found in the machine store.</exception>
        private X509Certificate2 GetCertificateFromStore()
        {
            if (string.IsNullOrEmpty(CertificateThumbprint))
            {
                throw new ArgumentNullException(nameof(CertificateThumbprint), "No certificate thumbprint found");
            }
            X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            try
            {
                X509Certificate2Collection certs = store.Certificates.Find(X509FindType.FindByThumbprint, CertificateThumbprint, validOnly: false);

                if (certs.Count != 1)
                {
                    throw new Exception($"No certificate with thumbprint {CertificateThumbprint} was found in the machine store.");
                }

                return certs[0];
            }
            finally
            {
                store.Close();
            }
        }
    }
}
