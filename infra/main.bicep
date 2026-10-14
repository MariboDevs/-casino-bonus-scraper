// Production shape for 200 casinos scraped daily. One resource group, one region,
// everything keyless: the job reaches SQL, Storage and Azure OpenAI with a managed identity.
targetScope = 'resourceGroup'

@description('Short environment name, used in resource names.')
@allowed(['prod', 'uat'])
param environmentName string = 'prod'

param location string = resourceGroup().location

@description('Container image of the scraper, e.g. <acr>.azurecr.io/bonus-scraper:<git-sha>.')
param scraperImage string

@description('Existing Azure OpenAI account the job calls (shared, so not created here).')
param openAiAccountName string

@description('Chat deployment on that account.')
param openAiDeployment string

@description('Entra ID group that administers the SQL server.')
param sqlAdminGroupName string

param sqlAdminGroupObjectId string

@description('Who is told when a run fails or stops happening.')
param alertEmail string

@description('Custom domain for the page, e.g. bonuses.example.com. Empty to skip.')
param customDomain string = ''

var suffix = uniqueString(resourceGroup().id)
var name = 'bonus-${environmentName}'

module monitoring 'modules/monitoring.bicep' = {
  name: 'monitoring'
  params: {
    name: name
    location: location
  }
}

resource identity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: 'id-${name}'
  location: location
}

module registry 'modules/registry.bicep' = {
  name: 'registry'
  params: {
    name: 'acr${replace(name, '-', '')}${suffix}'
    location: location
    pullPrincipalId: identity.properties.principalId
  }
}

module database 'modules/database.bicep' = {
  name: 'database'
  params: {
    serverName: 'sql-${name}-${suffix}'
    location: location
    adminGroupName: sqlAdminGroupName
    adminGroupObjectId: sqlAdminGroupObjectId
  }
}

module storage 'modules/storage.bicep' = {
  name: 'storage'
  params: {
    name: 'st${replace(name, '-', '')}${suffix}'
    location: location
    writerPrincipalId: identity.properties.principalId
    allowedOrigin: empty(customDomain) ? 'https://${site.outputs.defaultHostname}' : 'https://${customDomain}'
  }
}

module openAiAccess 'modules/openai-access.bicep' = {
  name: 'openai-access'
  params: {
    accountName: openAiAccountName
    principalId: identity.properties.principalId
  }
}

module job 'modules/scraper-job.bicep' = {
  name: 'scraper-job'
  params: {
    name: name
    location: location
    image: scraperImage
    identityId: identity.id
    identityClientId: identity.properties.clientId
    registryServer: registry.outputs.loginServer
    logAnalyticsWorkspaceId: monitoring.outputs.workspaceId
    sqlConnectionString: database.outputs.connectionString
    openAiEndpoint: openAiAccess.outputs.endpoint
    openAiDeployment: openAiDeployment
    offersContainerUrl: storage.outputs.offersContainerUrl
  }
}

module site 'modules/site.bicep' = {
  name: 'site'
  params: {
    name: 'stapp-${name}'
    customDomain: customDomain
  }
}

module alerts 'modules/alerts.bicep' = {
  name: 'alerts'
  params: {
    name: name
    location: location
    alertEmail: alertEmail
    workspaceId: monitoring.outputs.workspaceId
    jobName: job.outputs.jobName
  }
}

output siteHostname string = site.outputs.defaultHostname
output offersUrl string = '${storage.outputs.offersContainerUrl}/casinos.json'
output registryLoginServer string = registry.outputs.loginServer
