
Param(
    $azureLocation = "westeurope",                      # Azure region 
    $resourceGroupName = "",                            # Where to create AKS and where the public IP is created
    $publicIpName = "",                                 # Name of IP address
    $botDomain = "",                                    # Bot DNS, e.g 
    $acrName = "",                                      # Container registry name (not FQDN)
    $AKSClusterName = "RickrollBotCluster",             # AKS resource name to use/create
    $applicationId = "",                                # Bot appID
    $applicationSecret = "",                            # Bot secret
    $botName = "",                                      # Bot service name, e.g 'ProdBot'
    $containerTag = "latest",                           # Image tag to deploy to AKS
    $applicationInsightsKey = ""                        # Application Insights instrumentation key
)
$aksNamespace = "rickrollbot"       # Also used for helm template name

if ($azureLocation -eq "" -or $resourceGroupName -eq "" -or $publicIpName -eq "" -or $botDomain -eq "" -or $acrName -eq "" -or $AKSClusterName -eq "" -or $applicationId -eq "" -or $applicationSecret -eq ""  -or $botName -eq "" -or $containerTag -eq "" -or $applicationInsightsKey -eq "") {
    Write-Host "Missing parameters - please check & run again" -ForegroundColor Red
    exit
}

# Init script
$AKSmgResourceGroup = "MC_"+$resourceGroupName+"_"+"$AKSClusterName"+"_"+$azureLocation
Write-Host "(Args): RG: $resourceGroupName, MC rg: $AKSmgResourceGroup, location: $azureLocation" -ForegroundColor Green
Write-Host "Environment Azure CL: $(az --version)"

# Create the resource group if not already created
Write-Host "About to create resource group: $resourceGroupName" -ForegroundColor Yellow
az group create -l $azureLocation -n $resourceGroupName

# Create the AKS Cluster
$PASSWORD_WIN="AbcABC123!@#123456"
Write-Host "About to create AKS cluster: $AKSClusterName in $resourceGroupName" -ForegroundColor Yellow
az aks create --resource-group $resourceGroupName --name $AKSClusterName --node-count 1 --enable-addons monitoring --generate-ssh-keys --windows-admin-password $PASSWORD_WIN --windows-admin-username azureuser --vm-set-type VirtualMachineScaleSets --network-plugin azure 

# Add the Windows Node pool
# TODO: remove this password and make better
Write-Host "About to create AKS Windows node-pool in $AKSClusterName. This will error if it's already been created, and that's ok..." -ForegroundColor Yellow
az aks nodepool add --resource-group $resourceGroupName --cluster-name $AKSClusterName --os-type Windows --name scale --node-count 1 --node-vm-size Standard_DS3_v2

# Get IP address from Azure Public IP address pre-created
Write-Host "Verifying IP & DNS configuration..." -ForegroundColor Yellow
$publicIpAddress = az network public-ip show --resource-group $resourceGroupName --name $publicIpName --query 'ipAddress'
if ($publicIpAddress -eq $null) {

    # Try again in the AKS RG
    $publicIpAddress = az network public-ip show --resource-group $AKSmgResourceGroup --name $publicIpName --query 'ipAddress'

    if ($publicIpAddress -eq $null) {
        Write-Host "Fatal: Unable to locate Azure IP address '$publicIpName' in any resource-group." -ForegroundColor Red
        exit
    }
}
else {
    # Found IP address in original RG. Move to the AKS RG.
    # This is needed in order for the load balancer to get assigned with the Public IP, otherwise you might end up in a "pending" state.
    Write-Host "Moving Public IP resource to $AKSmgResourceGroup so AKS can use it" -ForegroundColor Yellow
    $publicIpAddressId = az network public-ip show --resource-group $resourceGroupName --name $publicIpName --query 'id'
    az resource move --destination-group $AKSmgResourceGroup --ids $publicIpAddressId
}
$publicIpAddress = $publicIpAddress.Replace('"', '')

Write-Host "Azure public IP to use is: $publicIpAddress" -ForegroundColor Green

# Check IP address against DNS
$dnsResult = Resolve-DnsName -Name $botDomain -Type A
if ($publicIpAddress -eq $dnsResult.IP4Address) {
    Write-Host "Verified DNS name $botDomain has valid A-record for IP address $publicIpAddress" -ForegroundColor Green
}
else {
    $tmpWrongIp = $dnsResult.IP4Address
    Write-Host "Fatal: DNS name '$botDomain' has A-record for IP address '$tmpWrongIp', and *not* our IP address $publicIpAddress. This will break the SSL manager - aborting." -ForegroundColor Red
    exit
}

# Create the Azure Container Registry to hold the bot's docker image (if not already there)
Write-Host "About to create ACR: $acrName. This will error if it's already been created, and that's ok..." -ForegroundColor Yellow
az acr create --resource-group $resourceGroupName --name $acrName --sku Basic --admin-enabled true

Write-Host "Updating AKS cluster with ACR" -ForegroundColor Yellow
az aks update -n $AKSClusterName -g $resourceGroupName --attach-acr $acrName

# Starting with basic setup
Write-Output "Getting AKS credentials for cluster: $AKSClusterName"
az aks get-credentials --resource-group $resourceGroupName --name $AKSClusterName --overwrite-existing 


# BEGIN K8 deployment
# Make sure everything is clean before doing things on first run this will give errors, but when running it again it will restore things to initial state.
Write-Host "Cleaning up resources from previous script execution. This will error if this is the 1st run, and this is fine..." -ForegroundColor Yellow
# Uninstall via helm the bot
helm uninstall $aksNamespace --namespace $aksNamespace
# Delete certificates namespace
kubectl delete ns $aksNamespace
kubectl delete ns cert-manager
# Delete ngix ingress
kubectl delete ns ingress-nginx
# make sure the secret is updated - so delete it if there
kubectl delete secrets bot-application-secrets --namespace $aksNamespace

# Create Kubernetes resources
Write-Host "About to create cert-manager namespace"
kubectl create ns cert-manager

Write-Output "Updating helm repo"
helm repo add jetstack https://charts.jetstack.io
helm repo update

Write-Host "Installing cert-manager" -ForegroundColor Yellow
helm install cert-manager jetstack/cert-manager --namespace cert-manager `
    --set nodeSelector."kubernetes\.io/os"=linux `
    --set webhook.nodeSelector."kubernetes\.io/os"=linux `
    --set cainjector.nodeSelector."kubernetes\.io/os"=linux `
    --set installCRDs=true

Write-Host "Waiting for cert-manager to be ready"
kubectl wait pod -n cert-manager --for condition=ready --timeout=60s --all

# Setup Ingress
Write-Output "Creating ingress-nginx namespace"
kubectl create namespace ingress-nginx
kubectl label namespace ingress-nginx cert-manager.io/disable-validation=true

Write-Output "Adding helm repositories"
helm repo add nginx-stable https://helm.nginx.com/stable
helm repo update

Write-Host "Installing ingress-nginx" -ForegroundColor Yellow
helm upgrade --install ingress-nginx ingress-nginx --repo https://kubernetes.github.io/ingress-nginx --namespace ingress-nginx --create-namespace `
    --set controller.replicaCount=3 `
    --set controller.nodeSelector."kubernetes\.io/os"=linux `
    --set controller.admissionWebhooks.enabled=false `
    --set controller.service.enabled=false `
    --set controller.service.loadBalancerIP=$publicIpAddress --version 4.2.5
    

kubectl delete -A ValidatingWebhookConfiguration ingress-nginx-admission

# Apply letsencrypt
kubectl apply -f cluster-issuer.yaml

# Setup AKS namespace & secrets for bot
Write-Host "Creating $aksNamespace namespace and bot secret that holds BOT_ID, BOT_SECRET, BOT_NAME, App Insights key" -ForegroundColor Yellow
kubectl create ns $aksNamespace
kubectl create secret generic bot-application-secrets --namespace $aksNamespace --from-literal=applicationId="$applicationId" --from-literal=applicationSecret="$applicationSecret" --from-literal=botName="$botName" --from-literal=applicationInsightsKey="$applicationInsightsKey"

# Setup Helm for recording bot
Write-Host "Setting up helm for $aksNamespace for bot domain: $botDomain and Public IP: $publicIpAddress" -ForegroundColor Yellow
helm install $aksNamespace ./$aksNamespace --namespace $aksNamespace --create-namespace `
    --set host=$botDomain `
    --set public.ip=$publicIpAddress `
    --set image.domain="$acrName.azurecr.io" `
    --set image.tag=$containerTag


# Check for external IP
$externalIpValidation = kubectl --namespace ingress-nginx get services -o wide ingress-nginx-controller
if ($externalIpValidation -notlike '*<pending>*') 
{
    Write-Host "External IP verified as bound to load-balancer" -ForegroundColor Green
}
else 
{
    Write-Host "Warning: we don't seem to have found an external IP. Check in a minute with 'kubectl --namespace ingress-nginx get services -o wide ingress-nginx-controller'" -ForegroundColor red
}


# Validate certificate, wait a minute or two
Write-Host "Sleeping for 5 mins before running validation..." -ForegroundColor Yellow
Start-Sleep -Seconds 300
$certValidation = kubectl get cert -n $aksNamespace
if ($certValidation -like '*True*') 
{
    Write-Host "SSL configuration working & verified" -ForegroundColor Green
}
else 
{
    Write-Host "SSL configuration validation failed..." -ForegroundColor red
    Write-Output "it might need some more time, or something went wrong..."
    Write-Output "try manually executing: 'kubectl get cert -n $aksNamespace' in a few minutes."
    exit -1    
}
