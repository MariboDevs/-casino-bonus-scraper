// The Angular page on Static Web Apps Free: global CDN, and a managed, auto-renewing
// TLS certificate for the custom domain once its CNAME points at the default hostname.
param name string
param customDomain string

resource site 'Microsoft.Web/staticSites@2023-12-01' = {
  name: name
  location: 'westeurope'
  sku: {
    name: 'Free'
    tier: 'Free'
  }
  properties: {}
}

resource domain 'Microsoft.Web/staticSites/customDomains@2023-12-01' = if (!empty(customDomain)) {
  parent: site
  name: empty(customDomain) ? 'placeholder.invalid' : customDomain
  properties: {
    validationMethod: 'cname-delegation'
  }
}

output defaultHostname string = site.properties.defaultHostname
