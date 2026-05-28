import { bootstrapSampleApp } from 'shared-sso/bootstrap';
import { ssoEnvironment } from 'shared-sso/environment';

bootstrapSampleApp({
  name: 'Inventory',
  slug: 'inventory',
  color: '#FF6D01',
  apiBaseUrl: ssoEnvironment.apiBaseUrl,
  dashboardUrl: ssoEnvironment.dashboardUrl,
  storageKey: 'sampleapp_inventory'
});
