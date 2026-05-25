import { bootstrapApplication } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideBrowserGlobalErrorListeners } from '@angular/core';
import { AppComponent } from './app';
import { sampleAppRoutes } from './app.routes';
import { APP_CONFIG, SampleAppConfig } from './app-config';

/** Single entry-point used by each sample app's main.ts. */
export function bootstrapSampleApp(config: SampleAppConfig) {
  document.title = config.name;
  return bootstrapApplication(AppComponent, {
    providers: [
      provideBrowserGlobalErrorListeners(),
      provideRouter(sampleAppRoutes),
      provideHttpClient(),
      { provide: APP_CONFIG, useValue: config }
    ]
  }).catch(err => console.error(err));
}
