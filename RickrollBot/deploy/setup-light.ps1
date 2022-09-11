
Param(
    $azureLocation = "westeurope",                      # Azure region 
    $resourceGroupName = "AKSBotProd",                  # Where to create AKS and where the public IP is created
    $botDomain = "simplebot.teamsplatform.app",                                    # Bot DNS, e.g 
    $AKSClusterName = "botscluster",               # AKS resource name to use/create
    $applicationId = "248e433e-d809-4ebf-8a75-cab1c06e7b9c",                                # Bot appID
    $applicationSecret = "",                            # Bot secret
    $botName = "sfbrandobot",                                      # Bot service name, e.g 'ProdBot'
    $applicationInsightsKey = "64eacac7-e8d7-44c6-9aca-ed4295c9bacc",                        # Application Insights instrumentation key,
    $acrName = "sfbdev"                             # Container registry name (not FQDN)
)


if ($azureLocation -eq "" -or $resourceGroupName -eq "" -or $botDomain -eq "" -or $acrName -eq "" -or $AKSClusterName -eq "" -or $applicationId -eq "" -or $applicationSecret -eq ""  -or $botName -eq "" -or $applicationInsightsKey -eq "") {
    Write-Host "Missing parameters - please check & run again" -ForegroundColor Red
    exit
}

# Init script
Write-Host "(Args): RG: $resourceGroupName, location: $azureLocation" -ForegroundColor Green
Write-Host "Environment Azure CL: $(az --version)"

# Create the resource group if not already created
Write-Host "About to create resource group: $resourceGroupName" -ForegroundColor Yellow
az group create -l $azureLocation -n $resourceGroupName

# Create the AKS Cluster
$PASSWORD_WIN="AbcABC123!@#123456"
Write-Host "About to create AKS cluster: $AKSClusterName in $resourceGroupName" -ForegroundColor Yellow
az aks create --resource-group $resourceGroupName --name $AKSClusterName --node-count 1 --enable-addons monitoring `
    --generate-ssh-keys `
    --windows-admin-password $PASSWORD_WIN `
    --windows-admin-username azureuser `
    --vm-set-type VirtualMachineScaleSets `
    --network-plugin azure 

# Add the Windows Node pool so we can run Windows images.
# TODO: remove this password and make better
Write-Host "About to create AKS Windows node-pool in $AKSClusterName. This will error if it's already been created, and that's ok..." -ForegroundColor Yellow
az aks nodepool add --resource-group $resourceGroupName --cluster-name $AKSClusterName --os-type Windows --name scale --node-count 1 --node-vm-size Standard_DS3_v2

# Attach ACR to AKS cluster so we can download images from it
Write-Host "Updating AKS cluster with ACR" -ForegroundColor Yellow
az aks update -n $AKSClusterName -g $resourceGroupName --attach-acr $acrName

# Save credentials of AKS cluster
Write-Output "Getting AKS credentials for cluster: $AKSClusterName"
az aks get-credentials --resource-group $resourceGroupName --name $AKSClusterName --overwrite-existing 

kubectl create namespace bot-aks-simple

# Config
kubectl create secret generic bot-application-secrets --namespace bot-aks-simple `
    --from-literal=applicationId="$applicationId" `
    --from-literal=applicationSecret="$applicationSecret" `
    --from-literal=botName="$botName" `
    --from-literal=applicationInsightsKey="$applicationInsightsKey" `
    --from-literal=serviceDnsName="$botDomain"
    

# Upload certs
Write-Host "Uploading certificate to new secret" -ForegroundColor Yellow
kubectl create secret tls tls-secret --cert=tls.crt --key=tls.key --namespace bot-aks-simple

# Create bot & NLB
Write-Host "Applying bot YAML configuration" -ForegroundColor Yellow
kubectl apply -f .\bots-light-k8.yaml --namespace bot-aks-simple

# Give NLB a chance to get an IP address
Write-Host "Waiting to check for external IP address..." -ForegroundColor Yellow
Start-Sleep -Seconds 30

# Get IPs
Write-Host "Getting NLB service details" -ForegroundColor Yellow
kubectl --namespace bot-aks-simple get services
Write-Host "Ensure 'EXTERNAL-IP' has a value and that we have a DNS A-record from $botDomain to this external IP address..." -ForegroundColor Yellow

# Get pods
kubectl --namespace bot-aks-simple get pods
Write-Host "Ensure pod STATUS has no error & restarts say 0..." -ForegroundColor Yellow

# Create A-record for NLB IP to match domain...
