import { beforeEach, describe, expect, it, vi } from 'vitest';
import { store } from '@/store';
import { logout, setCredentials } from '@/store/auth.slice';
import { apiFetch } from './api-fetch';

function createJwt(payload: Record<string, unknown>): string {
  const encode = (value: string): string =>
    btoa(value).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');

  const header = encode(
    JSON.stringify({
      alg: 'HS256',
      typ: 'JWT',
    }),
  );

  const body = encode(JSON.stringify(payload));

  return `${header}.${body}.signature`;
}

describe('apiFetch auth refresh interceptor', () => {
  beforeEach(() => {
    vi.restoreAllMocks();
    store.dispatch(logout());
  });

  it('refreshes once and retries the original request after a 401', async () => {
    const expiredToken = createJwt({
      sub: 'user-1',
      email: 'user@example.com',
      role: 'User',
    });

    const freshToken = createJwt({
      sub: 'user-1',
      email: 'user@example.com',
      role: 'Admin',
      permission: ['read:orders'],
    });

    let profileCallCount = 0;

    const fetchMock = vi.fn(async (input: RequestInfo | URL) => {
      const url = String(input);

      if (url === '/api/v1/auth/refresh') {
        return new Response(
          JSON.stringify({
            accessToken: freshToken,
            expiresAt: '2099-01-01T00:00:00Z',
            refreshToken: 'rotated-refresh',
            refreshExpiresAt: '2099-02-01T00:00:00Z',
            tokenType: 'Bearer',
          }),
          {
            status: 200,
            headers: {
              'Content-Type': 'application/json',
            },
          },
        );
      }

      if (url === '/api/v1/profile') {
        profileCallCount += 1;

        if (profileCallCount === 1) {
          return new Response('', {
            status: 401,
          });
        }

        return new Response(
          JSON.stringify({
            id: 'user-1',
            email: 'user@example.com',
          }),
          {
            status: 200,
            headers: {
              'Content-Type': 'application/json',
            },
          },
        );
      }

      throw new Error(`Unexpected fetch call: ${url}`);
    });

    vi.stubGlobal('fetch', fetchMock);

    store.dispatch(
      setCredentials({
        accessToken: expiredToken,
        refreshToken: 'stale-refresh',
        user: {
          id: 'user-1',
          email: 'user@example.com',
          roles: ['User'],
          permissions: [],
        },
      }),
    );

    const result = await apiFetch<{ id: string; email: string }>('/api/v1/profile');

    expect(result).toEqual({
      id: 'user-1',
      email: 'user@example.com',
    });

    expect(fetchMock).toHaveBeenCalledTimes(3);

    expect(
      fetchMock.mock.calls.filter(([url]) => String(url) === '/api/v1/auth/refresh'),
    ).toHaveLength(1);

    expect(fetchMock.mock.calls.filter(([url]) => String(url) === '/api/v1/profile')).toHaveLength(
      2,
    );
  });

  it('issues exactly one refresh for three simultaneous 401 responses', async () => {
    const expiredToken = createJwt({
      sub: 'user-2',
      email: 'user2@example.com',
      role: 'User',
    });

    const freshToken = createJwt({
      sub: 'user-2',
      email: 'user2@example.com',
      role: 'User',
      permission: ['read:orders'],
    });

    let ordersCallCount = 0;

    const fetchMock = vi.fn(async (input: RequestInfo | URL) => {
      const url = String(input);

      if (url === '/api/v1/auth/refresh') {
        return new Response(
          JSON.stringify({
            accessToken: freshToken,
            expiresAt: '2099-01-01T00:00:00Z',
            refreshToken: 'rotated-refresh-2',
            refreshExpiresAt: '2099-02-01T00:00:00Z',
            tokenType: 'Bearer',
          }),
          {
            status: 200,
            headers: {
              'Content-Type': 'application/json',
            },
          },
        );
      }

      if (url === '/api/v1/orders') {
        ordersCallCount += 1;

        if (ordersCallCount <= 3) {
          return new Response('', {
            status: 401,
          });
        }

        return new Response(
          JSON.stringify({
            ok: true,
          }),
          {
            status: 200,
            headers: {
              'Content-Type': 'application/json',
            },
          },
        );
      }

      throw new Error(`Unexpected fetch call: ${url}`);
    });

    vi.stubGlobal('fetch', fetchMock);

    store.dispatch(
      setCredentials({
        accessToken: expiredToken,
        refreshToken: 'stale-refresh-2',
        user: {
          id: 'user-2',
          email: 'user2@example.com',
          roles: ['User'],
          permissions: [],
        },
      }),
    );

    const results = await Promise.all([
      apiFetch<{ ok: boolean }>('/api/v1/orders'),
      apiFetch<{ ok: boolean }>('/api/v1/orders'),
      apiFetch<{ ok: boolean }>('/api/v1/orders'),
    ]);

    expect(results).toEqual([{ ok: true }, { ok: true }, { ok: true }]);

    expect(
      fetchMock.mock.calls.filter(([url]) => String(url) === '/api/v1/auth/refresh'),
    ).toHaveLength(1);

    expect(fetchMock.mock.calls.filter(([url]) => String(url) === '/api/v1/orders')).toHaveLength(
      6,
    );
  });
});
