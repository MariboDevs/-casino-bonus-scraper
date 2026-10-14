// Keyless access to the shared Azure OpenAI account: the job's identity gets the
// narrowest role that can call deployments, and no API key exists to leak.
param accountName string
param principalId string

resource account 'Microsoft.CognitiveServices/accounts@2024-10-01' existing = {
  name: accountName
}

var openAiUser = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '5e0bd9bd-7b93-4f28-af87-19fc36ad61bd')

resource user 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: account
  name: guid(account.id, principalId, openAiUser)
  properties: {
    roleDefinitionId: openAiUser
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}

output endpoint string = account.properties.endpoint
