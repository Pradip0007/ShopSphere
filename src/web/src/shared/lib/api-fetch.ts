import { ApiError } from './api-error';
import { isProblemDetails, type ProblemDetails } from './problem-details';

export interface ApiFetchInit extends Omit<RequestInit, 'body'> {
  json?: unknown;
  noCredentials?: boolean;

  /** Internal: prevents infinite 401 retry loops. */
  _isRetry?: boolean;

  /** Skips Authorization header and refresh handling. */
  skipAuth?: boolean;
}

let refreshInFlight: Promise<boolean> | null = null;

async function performRefresh(): Promise<boolean> {
  const [{ store }, { setCredentials, logout }, { refresh }, { getUserFromAccessToken }] =
    await Promise.all([
      import('@/store'),
      import('@/store/auth.slice'),
      import('@/features/auth/api'),
      import('@/shared/lib/jwt'),
    ]);

  try {
    const response = await refresh();

    const user = getUserFromAccessToken(response.accessToken);

    store.dispatch(
      setCredentials({
        accessToken: response.accessToken,
        refreshToken: response.refreshToken,
        user,
      }),
    );

    return true;
  } catch {
    store.dispatch(logout());

    const { router } = await import('@/router');

    void router.navigate({
      to: '/login',
      search: {
        redirect: window.location.pathname,
      },
    });

    return false;
  }
}

function ensureRefresh(): Promise<boolean> {
  refreshInFlight ??= performRefresh().finally(() => {
    refreshInFlight = null;
  });

  return refreshInFlight;
}

async function currentAccessToken(): Promise<string | null> {
  const { store } = await import('@/store');

  return store.getState().auth.accessToken;
}

export async function apiFetch<T>(path: string, init: ApiFetchInit = {}): Promise<T> {
  const { json, noCredentials, headers, skipAuth, _isRetry, ...rest } = init;

  const requestHeaders = new Headers(headers);

  let body: BodyInit | null = null;

  if (json !== undefined) {
    requestHeaders.set('Content-Type', 'application/json');
    body = JSON.stringify(json);
  }

  if (!requestHeaders.has('Accept')) {
    requestHeaders.set('Accept', 'application/json');
  }

  if (!skipAuth) {
    const token = await currentAccessToken();

    if (token && !requestHeaders.has('Authorization')) {
      requestHeaders.set('Authorization', `Bearer ${token}`);
    }
  }

  const response = await fetch(path, {
    ...rest,
    headers: requestHeaders,
    body,
    credentials: noCredentials ? 'omit' : 'include',
  });

  if (response.status === 401 && !skipAuth && !_isRetry) {
    const refreshed = await ensureRefresh();

    if (refreshed) {
      return apiFetch<T>(path, {
        ...init,
        _isRetry: true,
      });
    }
  }

  if (response.status === 204) {
    return undefined as T;
  }

  const raw = await response.text();

  const parsed: unknown = raw.length > 0 ? tryParseJson(raw) : undefined;

  if (!response.ok) {
    const problem: ProblemDetails | undefined = isProblemDetails(parsed) ? parsed : undefined;

    const message = problem?.title ?? problem?.detail ?? `HTTP ${response.status}`;

    throw new ApiError(response.status, message, problem);
  }

  return parsed as T;
}

function tryParseJson(raw: string): unknown {
  try {
    return JSON.parse(raw);
  } catch {
    return raw;
  }
}
