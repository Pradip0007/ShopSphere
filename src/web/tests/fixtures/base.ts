import { test as base, expect, type Page } from '@playwright/test';

export const test = base.extend<{
  dismissCookieBanner: (page: Page) => Promise<void>;
}>({
  dismissCookieBanner: async (_fixtures, use) => {
    await use(async (page) => {
      const banner = page.getByTestId('cookie-banner');

      if (await banner.isVisible().catch(() => false)) {
        await page.getByRole('button', { name: /accept|agree|got it/i }).click();

        await expect(banner).toBeHidden();
      }
    });
  },
});

export { expect };
