export const environment = {
  production: false,
  // Empty base => calls hit '/api/election/*' on this origin, proxied to the
  // ElectionTracker backend (http://localhost:5210) by proxy.conf.json.
  apiBaseUrl: '',
  loginUrl: 'http://localhost:4200/login',
};
