import { bootstrapSampleApp } from 'shared-sso/bootstrap';
import { ssoEnvironment } from 'shared-sso/environment';

bootstrapSampleApp({
  name: 'Finance',
  slug: 'finance',
  color: '#EA4335',
  apiBaseUrl: ssoEnvironment.apiBaseUrl,
  dashboardUrl: ssoEnvironment.dashboardUrl,
  storageKey: 'sampleapp_finance'
});
