import { ApiError } from './api-error';
import { isProblemDetails, type ProblemDetails } from './problem-details';

export interface ApiFetchInit extends Omit<RequestInit, 'body'> {
  /** JSON body — will be stringified and Content-Type set. */
  json?: unknown;
  /** Skip credentials — default is 'include' for auth cookies. */
  noCredentials?: boolean;
}

export async function apiFetch<T>(path: string, init: ApiFetchInit = {}): Promise<T> {
  const { json, noCredentials, headers, ...rest } = init;

  const requestHeaders = new Headers(headers);
  let body: BodyInit | null = null;

  if (json !== undefined) {
    requestHeaders.set('Content-Type', 'application/json');
    body = JSON.stringify(json);
  }

  if (!requestHeaders.has('Accept')) {
    requestHeaders.set('Accept', 'application/json');
  }

  const response = await fetch(path, {
    ...rest,
    headers: requestHeaders,
    body,
    credentials: noCredentials ? 'omit' : 'include',
  });

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
