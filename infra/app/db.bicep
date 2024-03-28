param accountName string
param cosmosDatabaseName string
param location string = resourceGroup().location
param tags object = {}

var containers = [
  {
    name: 'messages'
    partitionKey: '/id'
  }
]
param keyVaultName string

module cosmosDbAccount '../core/database/cosmos/sql/cosmos-sql-account.bicep' = {
  name: 'cosmos-db-account'
  params: {
    name: accountName
    location: location
    tags: tags
    keyVaultName: keyVaultName
  }
}

module cosmosDbDatabase '../core/database/cosmos/sql/cosmos-sql-db.bicep' = {
  name: cosmosDatabaseName
  params: {
    accountName: cosmosDbAccount.outputs.name
    databaseName: 'messagedb'
    location: location
    tags: tags
    containers: containers
    keyVaultName: keyVaultName
  }
}

output connectionStringKey string = cosmosDbDatabase.outputs.connectionStringKey
output databaseName string = cosmosDbDatabase.outputs.databaseName
output endpoint string = cosmosDbDatabase.outputs.endpoint
