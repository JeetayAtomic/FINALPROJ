import { bootstrapSampleApp } from 'shared-sso/bootstrap';
import { ssoEnvironment } from 'shared-sso/environment';

bootstrapSampleApp({
  name: 'HR Portal',
  slug: 'hr',
  color: '#4285F4',
  apiBaseUrl: ssoEnvironment.apiBaseUrl,
  storageKey: 'sampleapp_hr'
});
