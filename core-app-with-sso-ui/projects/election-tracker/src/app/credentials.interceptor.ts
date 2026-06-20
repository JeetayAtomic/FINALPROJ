import { HttpInterceptorFn } from '@angular/common/http';

// Ensure the session cookie is sent/stored on every API call (matters when the SPA
// talks to the backend cross-origin rather than through the dev-server proxy).
export const credentialsInterceptor: HttpInterceptorFn = (req, next) =>
  next(req.clone({ withCredentials: true }));
