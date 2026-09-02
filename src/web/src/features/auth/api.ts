import { apiFetch } from '@/shared/lib/api-fetch';

export interface AuthResponse {
  accessToken: string;
  expiresAt: string;
  refreshToken: string;
  refreshExpiresAt: string;
  tokenType: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
}

export interface RegisterResponse {
  userId: string;
}

export function login(req: LoginRequest): Promise<AuthResponse> {
  return apiFetch<AuthResponse>('/api/v1/auth/login', {
    method: 'POST',
    json: req,
    skipAuth: true,
  });
}

export function register(req: RegisterRequest): Promise<RegisterResponse> {
  return apiFetch<RegisterResponse>('/api/v1/auth/register', {
    method: 'POST',
    json: req,
    skipAuth: true,
  });
}

export function refresh(): Promise<AuthResponse> {
  return apiFetch<AuthResponse>('/api/v1/auth/refresh', {
    method: 'POST',
    skipAuth: true,
  });
}

export function logoutApi(): Promise<void> {
  return apiFetch<void>('/api/v1/auth/logout', {
    method: 'POST',
    skipAuth: true,
  });
}
