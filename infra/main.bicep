param location string = resourceGroup().location
param sku string = 'B1'
param appServicePlanName string = 'asp-jobfindernet'
param webAppName string = 'app-jobfindernet'
param serverName string = 'pg-jobfindernet'

resource appServicePlan 'Microsoft.Web/serverfarms@2023-01-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: sku
  }
}

resource webApp 'Microsoft.Web/sites@2023-01-01' = {
  name: webAppName
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      alwaysOn: true
      netFrameworkVersion: 'v10.0'
      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Production'
        }
        {
          name: 'ConnectionStrings__DefaultConnection'
          value: 'Host=${serverName}.postgres.database.azure.com;Database=JobFinderDb;Username=jobfinderadmin;Password=${listConnectionStrings(serverName)}'
        }
        {
          name: 'Jwt__Key'
          value: '@Microsoft.KeyVault(SecretUri=https://kv-jobfindernet.vault.azure.net/secrets/JwtKey/)'
        }
        {
          name: 'Jwt__Issuer'
          value: 'JobFinderNet'
        }
        {
          name: 'Jwt__Audience'
          value: 'JobFinderNet'
        }
      ]
    }
  }
}
