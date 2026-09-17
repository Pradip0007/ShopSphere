import { defineConfig, devices } from '@playwright/test';

const CI = !!process.env.CI;

export default defineConfig({
  testDir: './tests',
  timeout: 30_000,
  expect: { timeout: 5_000 },

  fullyParallel: true,

  forbidOnly: CI,
  retries: CI ? 1 : 0,
  workers: CI ? 2 : undefined,

  reporter: CI
    ? [['github'], ['html', { open: 'never' }]]
    : [['list'], ['html', { open: 'never' }]],

  use: {
    baseURL: process.env.E2E_BASE_URL ?? 'http://localhost:5173',
    trace: 'on-first-retry',
    video: 'retain-on-failure',
    screenshot: 'only-on-failure',
  },

  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],

  webServer: CI
    ? undefined
    : {
        command: 'pnpm dev',
        url: 'http://localhost:5173',
        reuseExistingServer: true,
        timeout: 60_000,
      },
});
