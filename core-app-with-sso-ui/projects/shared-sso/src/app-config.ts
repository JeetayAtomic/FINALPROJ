import { InjectionToken } from '@angular/core';

export interface SampleAppConfig {
  /** Display name, e.g. "HR Portal". */
  name: string;
  /** Short slug for page title fallback, e.g. "hr". */
  slug: string;
  /** Header accent color (hex). */
  color: string;
  /** Dashboard API root (e.g. http://localhost:5213). */
  apiBaseUrl: string;
  /** localStorage key prefix — keeps apps on different ports from clobbering each other. */
  storageKey: string;
}

export const APP_CONFIG = new InjectionToken<SampleAppConfig>('SampleAppConfig');
