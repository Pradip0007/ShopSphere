import { expect, test } from './fixtures/base';

test.describe('Home page', () => {
  test.beforeEach(async ({ page, dismissCookieBanner }) => {
    await page.goto('/');
    await dismissCookieBanner(page);
  });

  test('renders the ShopSphere title', async ({ page }) => {
    await expect(page).toHaveTitle(/ShopSphere/i);
  });

  test('shows the primary nav', async ({ page }) => {
    await expect(page.getByTestId('nav-products')).toBeVisible();
  });

  test('shows the newsletter section', async ({ page }) => {
    await expect(page.getByRole('heading', { name: 'Get our newsletter', level: 2 })).toBeVisible();
  });
});
