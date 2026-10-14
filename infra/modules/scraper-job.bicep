// A Container Apps Job on the consumption plan: it exists only while it runs, so a
// daily scrape costs seconds of compute. Cron trigger, one retry, hard timeout.
param name string
param location string
param image string
param identityId string
param identityClientId string
param registryServer string
param logAnalyticsWorkspaceId string
param sqlConnectionString string
param openAiEndpoint string
param openAiDeployment string
param offersContainerUrl string

resource workspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' existing = {
  name: last(split(logAnalyticsWorkspaceId, '/'))
}

resource environment 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: 'cae-${name}'
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: workspace.properties.customerId
        sharedKey: workspace.listKeys().primarySharedKey
      }
    }
  }
}

resource job 'Microsoft.App/jobs@2024-03-01' = {
  name: 'caj-${name}-scraper'
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: { '${identityId}': {} }
  }
  properties: {
    environmentId: environment.id
    configuration: {
      triggerType: 'Schedule'
      scheduleTriggerConfig: {
        cronExpression: '17 5 * * *'
        parallelism: 1
        replicaCompletionCount: 1
      }
      replicaRetryLimit: 1
      replicaTimeout: 3600
      registries: [
        {
          server: registryServer
          identity: identityId
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'scraper'
          image: image
          args: ['scrape']
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
          env: [
            { name: 'AZURE_CLIENT_ID', value: identityClientId }
            { name: 'ConnectionStrings__bonusdb', value: sqlConnectionString }
            { name: 'Extraction__Provider', value: 'AzureOpenAI' }
            { name: 'Extraction__AzureOpenAI__Endpoint', value: openAiEndpoint }
            { name: 'Extraction__AzureOpenAI__Deployment', value: openAiDeployment }
            { name: 'Publishing__BlobContainerUrl', value: offersContainerUrl }
          ]
        }
      ]
    }
  }
}

output jobName string = job.name
