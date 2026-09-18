import type { APIRequestContext } from '@playwright/test';

const API_BASE_URL = 'https://localhost:7583';

const TEST_PASSWORD = 'E2eTestPassword123!';

interface LoginResponse {
  accessToken: string;
  expiresAt: string;
  refreshToken: string;
  refreshExpiresAt: string;
  tokenType: string;
}

export async function authenticateTestUser(request: APIRequestContext): Promise<string> {
  const email = `e2e-${crypto.randomUUID()}@shopsphere.local`;

  const registerResponse = await request.post(`${API_BASE_URL}/api/v1/auth/register`, {
    data: {
      email,
      password: TEST_PASSWORD,
    },
    ignoreHTTPSErrors: true,
  });

  if (!registerResponse.ok()) {
    throw new Error(
      `Test user registration failed: ${registerResponse.status()} ${await registerResponse.text()}`,
    );
  }

  const loginResponse = await request.post(`${API_BASE_URL}/api/v1/auth/login`, {
    data: {
      email,
      password: TEST_PASSWORD,
    },
    ignoreHTTPSErrors: true,
  });

  if (!loginResponse.ok()) {
    throw new Error(
      `Test user login failed: ${loginResponse.status()} ${await loginResponse.text()}`,
    );
  }

  const login = (await loginResponse.json()) as LoginResponse;

  if (!login.accessToken || !login.refreshToken) {
    throw new Error('Test user login did not return both authentication tokens.');
  }

  const cookies = await request.storageState();

  const refreshCookie = cookies.cookies.find((cookie) => cookie.name === 'shopsphere_refresh');

  if (!refreshCookie) {
    throw new Error('Test user login did not create a refresh cookie.');
  }

  return refreshCookie.value;
}
