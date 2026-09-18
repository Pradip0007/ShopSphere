import { test as base, expect, type Page } from '@playwright/test';
import { authenticateTestUser } from '../helpers/auth';

export const test = base.extend<{
  dismissCookieBanner: (page: Page) => Promise<void>;
  authenticatedPage: Page;
}>({
  dismissCookieBanner: async ({ page: _page }, use) => {
    await use(async (page) => {
      const banner = page.getByTestId('cookie-banner');

      if (await banner.isVisible().catch(() => false)) {
        await page.getByRole('button', { name: /accept|agree|got it/i }).click();

        await expect(banner).toBeHidden();
      }
    });
  },

  authenticatedPage: async ({ page, request }, use) => {
    const refreshToken = await authenticateTestUser(request);

    await page.context().addCookies([
      {
        name: 'shopsphere_refresh',
        value: refreshToken,
        domain: 'localhost',
        path: '/api/v1/auth',
        secure: true,
        httpOnly: true,
        sameSite: 'Strict',
      },
    ]);

    await page.goto('/');

    await use(page);
  },
});

export { expect };
