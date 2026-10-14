// The published JSON lives in Blob storage with versioning on, so every day's file is
// kept and a bad run can be rolled back by promoting the previous version.
@minLength(3)
param name string
param location string
param writerPrincipalId string
param allowedOrigin string

resource account 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: take(name, 24)
  location: location
  sku: { name: 'Standard_ZRS' }
  kind: 'StorageV2'
  properties: {
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: true
    allowSharedKeyAccess: false
    supportsHttpsTrafficOnly: true
  }
}

resource blobs 'Microsoft.Storage/storageAccounts/blobServices@2023-05-01' = {
  parent: account
  name: 'default'
  properties: {
    isVersioningEnabled: true
    deleteRetentionPolicy: { enabled: true, days: 30 }
    cors: {
      corsRules: [
        {
          allowedOrigins: [allowedOrigin]
          allowedMethods: ['GET', 'HEAD']
          allowedHeaders: ['*']
          exposedHeaders: ['ETag', 'Last-Modified']
          maxAgeInSeconds: 3600
        }
      ]
    }
  }
}

resource offers 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = {
  parent: blobs
  name: 'offers'
  properties: {
    publicAccess: 'Blob'
  }
}

var blobContributor = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'ba92f5b4-2d11-453d-a403-e96b0029c9fe')

resource writer 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: offers
  name: guid(offers.id, writerPrincipalId, blobContributor)
  properties: {
    roleDefinitionId: blobContributor
    principalId: writerPrincipalId
    principalType: 'ServicePrincipal'
  }
}

output offersContainerUrl string = '${account.properties.primaryEndpoints.blob}offers'
