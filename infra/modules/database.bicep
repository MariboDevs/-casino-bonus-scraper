// Azure SQL Database, serverless: it auto-pauses between daily runs, so compute is
// billed for minutes a day. Backups are the platform's: point-in-time restore for 14
// days on geo-redundant storage, plus weekly and monthly long-term copies for a year.
param serverName string
param location string
param adminGroupName string
param adminGroupObjectId string

resource server 'Microsoft.Sql/servers@2023-08-01' = {
  name: serverName
  location: location
  properties: {
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
    administrators: {
      administratorType: 'ActiveDirectory'
      azureADOnlyAuthentication: true
      login: adminGroupName
      sid: adminGroupObjectId
      principalType: 'Group'
      tenantId: subscription().tenantId
    }
  }
}

resource allowAzure 'Microsoft.Sql/servers/firewallRules@2023-08-01' = {
  parent: server
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

resource database 'Microsoft.Sql/servers/databases@2023-08-01' = {
  parent: server
  name: 'bonusdb'
  location: location
  sku: {
    name: 'GP_S_Gen5_1'
    tier: 'GeneralPurpose'
  }
  properties: {
    autoPauseDelay: 60
    minCapacity: json('0.5')
    maxSizeBytes: 2147483648
    requestedBackupStorageRedundancy: 'Geo'
    zoneRedundant: false
  }
}

resource shortTermBackups 'Microsoft.Sql/servers/databases/backupShortTermRetentionPolicies@2023-08-01' = {
  parent: database
  name: 'default'
  properties: {
    retentionDays: 14
    diffBackupIntervalInHours: 24
  }
}

resource longTermBackups 'Microsoft.Sql/servers/databases/backupLongTermRetentionPolicies@2023-08-01' = {
  parent: database
  name: 'default'
  properties: {
    weeklyRetention: 'P8W'
    monthlyRetention: 'P12M'
    weekOfYear: 1
  }
}

output connectionString string = 'Server=tcp:${server.properties.fullyQualifiedDomainName},1433;Database=${database.name};Authentication=Active Directory Managed Identity;Encrypt=True;'
