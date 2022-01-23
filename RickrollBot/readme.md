# RickrollBot Setup Guide
RickrollBot is a "Real-time Media Platform" Teams bot that is designed to Rickroll Teams meetings. In laymans terms it's a bot that can send/receive audio & video in Teams (or other platforms too).

The purpose of RickrollBot is two-fold: a research project to experiment with especially for people interested in these types of bots, and for the lulz too.

It's designed to run either locally via ngrok or in Azure Kubernetes Service so it can scale for "production" scenarios. 

It's not trivial to get running mainly thanks to the fact that these type of bots require TCP-level integration into the host OS. This means they won't work inside the usual context of IIS or any other webserver which would handle a lot of the hassle around SSL and TCP. But this guide should get you there anyway, if you have some patience. It's all in a good cause though: Rick Astley in your Teams meetings. 

# Requirements
To pull this off you need:

1. Azure subscription on same Azure tenant as Office 365/Teams
2. **Dev deploy only** :
    - ngrok with pro licence already configured (pro version needed to allow TCP + HTTP tunnelling).
    - Free SSL certificate for ngrok URL (see below on how to generate). Self-signed SSL will not work. 
    - Visual Studio 2019/2022
3. **Production deploy** :
    - Public bot domain (root-level) + DNS control for domain.
    - Docker for Windows to build bot container image.
6. Source code: [https://github.com/sambetts/poc-bots/tree/main/RickrollBot](https://github.com/sambetts/poc-bots/tree/main/RickrollBot)
7. Bot permissions in Azure AD application:
    - AccessMedia.All
    - JoinGroupCall.All
    - JoinGroupCallAsGuest.All

# Required Configuration Information
Most of these values we'll get after creating the resources below.

1. **Dev only** :
    - ngrok domains, TCP address, and auth token for pro license - $ngrokAuthToken.
    - SSL certificate thumbprint - $certThumbPrint.
2. Bot service DNS name - $botDomain.
    - **Production only:** this is your own domain.
    - **Dev** : this is your reserved NGrok domain
3. **Production only:**
    - Azure container registry name/URL - $acrName (for 'contosoacr').
    - Azure App Service to host Teams App; the DNS hostname - $teamsAppDNS.
    - Application Insights instrumentation key - $appInsightsKey
4. Azure AD: tenant ID, Bot App ID &amp; secret.
    - $azureAdTenantId, $applicationId, $applicationSecret
5. Azure Bot Service name – $botName

# Setup Steps
These steps differ depending on whether you plan on running the bot in AKS/K8 or directly on from Visual Studio for developing the solution.

## Prepare Local Files
Some configuration specific files aren't tracked in git, so need creating locally from the templates.

- **Production only:**
    - Copy 'deploy\cluster-issuer - template.yaml' to 'deploy\cluster-issuer.yaml'
    - Edit 'cluster-issuer.yaml' and replace '$YOUR\_EMAIL\_HERE' with your own email.
        - This is used for LetsEncrypt and needs to be a proper email address; not a free one (Gmail, Outlook, etc)
- **Dev Only**
    - Copy 'BotService\Bot.Console\template.env' to just 'BotService\Bot.Console\\.env'
        - If Windows explorer doesn't like the rename, you may need to run: copy .\template.env ".\\.env"

## Dev Only: Setup ngrok configuration
For developer machines you'll want to run the bot directly from Visual Studio instead of from a container. For this to happen, we need inbound tunnelling to the right places.

1. In [https://dashboard.ngrok.com/](https://dashboard.ngrok.com/), reserve a TCP address &amp; domain, all based in the US region.
    - Reserved TCP address for Skype Media endpoint – take note of address ($streamingAddressFull) and port of the TCP addres ($streamingAddressPort).
    - A new ngrok forwarding domain for the Azure bot service - $botDomain.
    - Important: for some reason (that's not entirely clear to me) [if your reserved TCP port is something other than 0.tcp.ngrok.io or 1.tcp.ngrok.io, the bot will not connect.](https://github.com/microsoftgraph/microsoft-graph-comms-samples/issues/405#issuecomment-787608319)

2. Copy 'ngrok-bot-tunnels - template.yaml' to 'ngrok-bot-tunnels.yaml'. Update:
    - $ngrokAuthToken
    - $streamingAddressFull

3. Run ngrok to open tunnels like so: "ngrok start --all -config .\ngrok-bot-tunnels.yaml"

The ngrok output should look something like this:

    - Region United States
    - tcp://1.tcp.ngrok.io:26065 -> localhost:8445
    - http://rickrollbot.ngrok.io -> https://localhost:9441
    - https://rickrollbot.ngrok.io -> https://localhost:9441

## Dev Only: Generate SSL for Bot Media TCP Endpoint
As this bot receives audio/video streams it must expose a TCP endpoint with SSL in addition to the normal HTTP endpoints. For dev we must request these certificates manually; in production there is an AKS service we deploy to do it automatically.

For an AKS deployment the following tasks are automated in the cluster, but for dev we need to do this ourselves. 

1. Generate an SSL certificate for your developer ngrok addresses as per [this guide](https://github.com/microsoftgraph/microsoft-graph-comms-samples/blob/master/Samples/V1.0Samples/AksSamples/teams-recording-bot/docs/setup/certificate.md#%23generate-ssl-certificate).
    - In short, you need to use [certbot](https://certbot.eff.org/instructions?ws=other&os=windows) to generate SSL certificates via LetsEncrypt (an org that give free SSL cerificates out. Perfect for us).
    - Let's prove we "own" the ngrok domain. Open port 80 of your bot domain with a specific ngrok command (don't use your normal ngrok config file launch):
        - ngrok http 80 -subdomain $botDomain
        - Example: 'ngrok http 80 -subdomain rickrollbot' (example domain is: rickrollbot.ngrok.io)
    - Now run certbot to validate you own the domain &amp; download the certificates, via the command-line wizard.
        - certbot certonly --standalone
    - certbot will create a temporary webserver that LetsEncrypt will read to validate ownership of the domain $botDomain – your NGrok tunnel domain. Once validated, certificates for that domain are downloaded in PEM format - look for the certbot output for where.
2. Once the PEM files have been created by certbot, you need to convert them to PFX format with [Open SSL](https://slproweb.com/products/Win32OpenSSL.html), in the directory the PEM files were created (C:\Certbot\archive\$botDomain usually):
    - openssl pkcs12 -export -out ngrokbotdomain.pfx -inkey privkey1.pem -in cert1.pem -certfile chain1.pem
3. You'll need to create a password for the PFX file.
4. Now install the certificate into the machines certificate store via MMC or Windows explorer.
5. Take note of the certificate thumbprint – $certThumbPrint

## Create Azure Resources
Create: Azure Bot Service, and for production only: Application Insights. 

1. Create new "Azure bot" in the Azure portal (can use az cmd if you wish too). 
    - Use the free tier. 
    - Type of app: "single tenant" or "multi tenant" (just not 'managed identity' - important).
    - Create a new application registration for bot service.
    - Once created, configure channel to Microsoft Teams, with calling enabled with endpoint: https://$botDomain/api/calling
2. Take note of associated bot service app registration ID &amp; secret – the secret of which is stored in an associated key vault in the same resource-group.
    - You'll have to add yourself to the access control of the key vault to get the secret. 

## Grant Bot Teams Access
The bot app registration needs the rights to join meetings next. In "API permissions" add the Graph API permissions granted specified in the requirements.
**Important**: don't forget to grant admin consent to these permissions. 

## Production only: build Docker image of bot
For AKS deployments we first need an image of the bot service. You don't if you're just developing locally. 
1. Create an Azure container registry to push/pull bot image to.
2. With Docker in 'Windows container' mode, build a bot image from the root directory.
    - docker build -f ./build/Dockerfile . -t [TAG]
        - [TAG] is the FQDN of you container registry + image name, e.g. 'RickrollBotregistry.azurecr.io/RickrollBot:1.0.5'
3. Push image to container registry with 'docker push'. Take note of version tag (e.g 'RickrollBotregistry.azurecr.io/RickrollBot:1.0.5' – this number/value is your $containerTag).
    - You may need to authenticate to your ACR first with 'az acr login --name $acrName'

## Production only: Create AKS resource via PowerShell
1. Create public IP address (standard SKU) for bot domain & create/update DNS A-record. Resource-group can be the same as AKS resource.
2. Run 'setup.ps1' to create AKS + bot architecture, with parameters:
    - $azureLocation – example: 'westeurope'
    - $resourceGroupName – example: 'RickrollBotProd'
    - $publicIpName – example: 'AksIpStandard'
    - $botDomain – example: 'RickrollBot.teamsplatform.app'
    - $acrName – example: 'RickrollBotregistry'
    - $AKSClusterName– example: 'ClassroomCluster'
    - $applicationId – example: '151d9460-b018-4904-8f81-14203ac3cb4f'
    - $applicationSecret – example: '9p96lolQJSD~\*\*\*\*\*\*\*\*\*\*\*\*' (example truncated)
    - $botName – example: 'RickrollBotProd'
    - $containerTag– example: 'latest'

## Dev Only: Create netsh http and ssl bindings
Because we're hosting this bot outside of IIS, we need to do some once-only configuration to create SSL bindings.
In "RickrollBot\build" copy "certs-dev-template.bat" to "certs-dev.bat". 

Edit the file, replacing values in the .bat from "RickrollBot\BotService\Bot.Console\\.env" file:
- %AzureSettings__CallSignalingPort%
- %AzureSettings__InstanceInternalPort%
- %AzureSettings__CertificateThumbprint%

Example file:

    set /A CallSignalingPort2 = 9441 + 1

    REM Deleting bindings
    netsh http delete sslcert ipport=0.0.0.0:9441
    netsh http delete sslcert ipport=0.0.0.0:8445
    netsh http delete urlacl url=https://+:9441/
    netsh http delete urlacl url=https://+:8445/
    netsh http delete urlacl url=http://+:%CallSignalingPort2%/

    REM Add URLACL bindings
    netsh http add urlacl url=https://+:9441/ sddl=D:(A;;GX;;;S-1-1-0)
    netsh http add urlacl url=https://+:8445/ sddl=D:(A;;GX;;;S-1-1-0)
    netsh http add urlacl url=http://+:%CallSignalingPort2%/ sddl=D:(A;;GX;;;S-1-1-0)

    REM ensure the app id matches the GUID in AssemblyInfo.cs
    REM Ensure the certhash matches the certificate

    netsh http add sslcert ipport=0.0.0.0:9441 certhash=ccb180918bc68b38f2660a2b2f3f943554d45052 appid={aeeb866d-e17b-406f-9385-32273d2f8691}
    netsh http add sslcert ipport=0.0.0.0:8445 certhash=ccb180918bc68b38f2660a2b2f3f943554d45052 appid={aeeb866d-e17b-406f-9385-32273d2f8691}

Run "certs-dev.bat" with admin priveledges and check output for errors. The first time you run you'll see errors deleting old bindings. 

## Dev Only: Run Solution from Visual Studio
If you're deploying to AKS, this step won't be necessary as configuration is stored within the AKS cluster itself. For dev environments though:

1. Open "RickrollBot\BotService\Bot.Console\.env" and update the following values.
    - AzureSettings\_\_BotName - $botName
    - AzureSettings\_\_AadAppId - $applicationId
    - AzureSettings\_\_AadTenantId - $azureAdTenantId
    - AzureSettings\_\_AadAppSecret - $applicationSecret
    - AzureSettings\_\_ServiceDnsName - $botDomain
    - AzureSettings\_\_CertificateThumbprint - $certThumbPrint
    - AzureSettings\_\_InstancePublicPort - $streamingAddressPort

2. Run Visual Studio as administrator and start debugging 'Bot.Console'.
    - Set Bot.Console as the start-up project.

# Testing and Running Solution
Once the bot service is running we should test if it's working. 

First networking. We assume there are no firewalls interfering with the service endpoints:
- https://$botDomain (default SSL port 443) - used for Teams signals & our own bot control API.
- $streamingAddressPort

Test localhost from browser (https://localhost:9441/) - accept SSL warning. You should get a 404. 

Dev only: test ngrok URL - https://$botDomain (e.g https://rickrollbot.ngrok.io). You should also see a 404.

Next let's check if the bot can join a Teams call.
    POST to https://rickrollbot.ngrok.io/joinCall
```json

    {
        "JoinURL": $teamsJoinUrl,
        "DisplayName": "Rick Astley"
    }
```

Example body:
```json
    {
        "JoinURL": "https://teams.microsoft.com/l/meetup-join/19%3ameeting_NTMyM2M4YTYtY2ZiMi00NjkxLWI1YzQtZDA4MzJjM2E4NWFm%40thread.v2/0?context=%7b%22Tid%22%3a%22ffcdb539-892e-4eef-94f6-0d9851c479ba%22%2c%22Oid%22%3a%2248fe59a4-c951-43ca-9d16-972083aa6305%22%7d",
        "DisplayName": "Rick Astley"
    }
```

And with that, Rick should join your Teams call.

# Troubleshooting
Review logs Teams control messages with ngrok log:
http://127.0.0.1:4040

Review Visual Studio output/App Insights Telemetry. Here's a working bot start-up log:

    Initializing MP with Service FQDN: rickrollbot.ngrok.io, Instance public port: 26065, Instance internal port: 8445
    UseMPAzureAppHostPerfCounterProvider is false. Discarding MP perf counters
    erf is not registered: no key found at SYSTEM\CurrentControlSet\Services\MediaPerf\Performance
    UseMPAzureAppHostPerfCounterProvider is false. Not checking for perf counter registration
    MP Service Event: ReportEvent_MPSVC_I_SERVICE_STARTING
    00001 (MPSERVICEHOSTLIB,.ctor:MPInstanceDescriptor.cs(158)) [MPBindingConfig] SecurityKeys are Issuer: CN=R3, O=Let's Encrypt, C=US  CertSN: 036CC5B81C0D71B483E5F80C6C351B0DBFD5
    2 (MPSERVICEHOSTLIB,.ctor:MPInstanceDescriptor.cs(161)) [MPBindingConfig] PrincipalName (localhost) is irrelevant in MTLS scenarios and will be ignored. Using certificate with SN: 036CC5B81C0D71B483E5F80C6C351B0DBFD5,  Issuer: CN=R3, O=Let's Encrypt, C=US
    00001 (AVMP,.cctor:MediaProcessor.cs(484)) [MP] MediaProcessor Service is created.
    [DevBox]3416.1::01/23/2022-07:55:16.914.00000002 (AVMP,InitializeHostMonitor:MediaProcessor.cs(1751)) [MP] InitializeHostMonitor: Set supportunencryptedaudioportrange to false!
    [DevBox]3416.1::01/23/2022-07:55:16.917.00000003 (AVMP,InitializeHostMonitor:MediaProcessor.cs(1751)) [MP] InitializeHostMonitor: Set UseBundledPortRangeForConsumer to false!
    [DevBox]3416.1::01/23/2022-07:55:16.920.00000004 (AVMP,InitializeHostMonitor:MediaProcessor.cs(1751)) [MP] InitializeHostMonitor: Set UseBundledPortRangeForBusiness to false!
    [DevBox]3416.1::01/23/2022-07:55:16.923.00000005 (AVMP,InitializeHostMonitor:MediaProcessor.cs(1767)) [MP] InitializeHostMonitor: Set bundledminportrange to false!
    [DevBox]3416.1::01/23/2022-07:55:16.925.00000006 (AVMP,InitializeHostMonitor:MediaProcessor.cs(1767)) [MP] InitializeHostMonitor: Set bundledmaxportrange to false!
    [DevBox]3416.1::01/23/2022-07:55:18.245.00000003 (MPSERVICEHOSTLIB,EndInitialize:MPHostImpl.cs(777)) [MPServiceHost] EndInitialize - Blocking
    MP Service Event: ReportEvent_MPSVC_I_SERVICE_STARTED
    Initialized MP With TCP Uri: [net.tcp://rickrollbot.ngrok.io:26065/MediaProcessor] and HTTP Uri: []
    [DevBox]3416.1::01/23/2022-07:55:18.290.00000007 (AVMP,OnPublicRtpIPAddressChanged:MediaProcessor.cs(737)) MediaProcessor.OnPublicRtpIPAddressChanged, Interpreting Public Address [0.0.0.0] as 'not configured'
    [DevBox]3416.1::01/23/2022-07:55:18.298.00000008 (AVMP,OnPublicRtpIPAddressChanged:MediaProcessor.cs(766)) MediaProcessor.OnPublicRtpIPAddressChanged, Configuring public Addresses failed due to invalid or missing mappings
    Set PIP on MP: [0.0.0.0]
    MP workload configuration set to [(500,500)]
    Initialized MediaApi Platform
    Initialized MediaPlatform. ApplicationId : 20923ad3-db6b-4488-ad4d-d0d17232197d, MPUri: net.tcp://rickrollbot.ngrok.io:26065/MediaProcessor, IsTest: False.
