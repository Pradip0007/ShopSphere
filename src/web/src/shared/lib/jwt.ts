export interface JwtPayload {
  sub?: string;
  email?: string;
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'?: string | string[];
  permission?: string | string[];
}

export interface AuthTokenUser {
  id: string;
  email: string;
  roles: string[];
  permissions: string[];
}

export function decodeJwtPayload(token: string): JwtPayload {
  const parts = token.split('.');

  if (parts.length !== 3) {
    throw new Error('Invalid JWT format');
  }

  const payload = parts[1];

  if (!payload) {
    throw new Error('Invalid JWT payload');
  }

  const normalized = payload.replace(/-/g, '+').replace(/_/g, '/');
  const padded = normalized.padEnd(Math.ceil(normalized.length / 4) * 4, '=');

  try {
    return JSON.parse(atob(padded)) as JwtPayload;
  } catch {
    throw new Error('Invalid JWT payload');
  }
}

function toStringArray(value: string | string[] | undefined): string[] {
  if (value === undefined) {
    return [];
  }

  return Array.isArray(value) ? value : [value];
}

export function getUserFromAccessToken(token: string): AuthTokenUser {
  const payload = decodeJwtPayload(token);

  if (!payload.sub) {
    throw new Error('JWT is missing subject claim');
  }

  if (!payload.email) {
    throw new Error('JWT is missing email claim');
  }

  return {
    id: payload.sub,
    email: payload.email,
    roles: toStringArray(payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']),
    permissions: toStringArray(payload.permission),
  };
}
