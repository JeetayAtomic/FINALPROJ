import { ApplicationDto } from '../models/app.models';

/**
 * Open or focus a per-app browser tab.
 *
 * Each app gets a stable target name (derived from its BaseUrl path: e.g.
 * `http://localhost:5301/hr` → `app_hr`). On click we first probe with
 * `window.open('', name)`:
 *
 *   - returns a *blank* (`about:blank`) window  →  no tab exists yet — load `urlProvider()`.
 *   - returns a cross-origin window we can't read →  tab is already open — just focus it.
 *
 * `urlProvider` is a function (not a string) because the dashboard needs to mint
 * a one-time SSO token via the API only when a fresh tab is actually needed —
 * we don't want to waste a token just to refocus an existing tab.
 */
export function openOrFocusAppTab(
  app: ApplicationDto,
  urlProvider: () => Promise<string>
): void {
  const name = appWindowName(app);
  const probe = window.open('', name);
  if (!probe) {
    // Popup blocked: a deferred window.open (after the token await) would be blocked too,
    // so guarantee the redirect by navigating the current tab instead.
    urlProvider()
      .then(url => { window.location.href = url; })
      .catch(() => { /* token failed — leave the dashboard as-is */ });
    return;
  }

  let isFresh = false;
  try {
    const href = probe.location.href;
    isFresh = !href || href === 'about:blank';
  } catch {
    // Cross-origin existing tab — we can't read it, so assume it's a live app tab.
    isFresh = false;
  }

  if (isFresh) {
    // Brand-new tab opened by the probe — load the app URL into it.
    urlProvider()
      .then(url => {
        try {
          probe.location.href = url;
          probe.focus();
        } catch {
          // Couldn't drive the probe tab — fall back to navigating the current tab.
          window.location.href = url;
        }
      })
      .catch(() => {
        // Token mint failed — don't strand a blank tab.
        try { probe.close(); } catch { /* ignore */ }
      });
  } else {
    // Existing tab — just bring it to the front. No SSO token wasted, no reload.
    try { probe.focus(); } catch { /* ignore */ }
  }
}

/**
 * Deterministic window name for an app, shared by the Angular dashboard and the
 * sample apps' 9-dots launcher. Derived from the BaseUrl path so both ends agree
 * without any extra registry: `http://localhost:5301/hr` → `app_hr`.
 */
export function appWindowName(app: ApplicationDto): string {
  try {
    const u = new URL(app.baseUrl);
    const seg = u.pathname.replace(/^\/+|\/+$/g, '').split('/')[0] || '';
    return seg ? `app_${seg.toLowerCase()}` : `app_${app.id}`;
  } catch {
    return `app_${app.id}`;
  }
}
