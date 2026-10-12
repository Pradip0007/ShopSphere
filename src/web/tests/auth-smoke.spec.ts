import { expect, test } from './fixtures/base';

test('authenticated user can open the home page', async ({ authenticatedPage }) => {
  await expect(authenticatedPage.getByTestId('nav-products')).toBeVisible();
  await expect(authenticatedPage.getByTestId('cart-page-link')).toBeVisible();
});

test('login and registration use the ShopSphere account layout', async ({ page }) => {
  await page.goto('/login');

  await expect(
    page.getByRole('banner').getByRole('link', { name: 'ShopSphere home' }),
  ).toBeVisible();
  await expect(page.locator('img[src="/shopsphere-logo.png"]')).toBeVisible();
  await expect(page.getByRole('heading', { name: 'Sign in or create an account' })).toBeVisible();
  await expect(page.getByRole('search')).toHaveCount(0);
  await expect(page.getByRole('contentinfo')).toContainText('About ShopSphere');
  await expect(page.getByRole('contentinfo')).toContainText('Get our newsletter');
  await expect(page.getByTestId('main-content').getByLabel('Email')).toBeVisible();
  await expect(page.getByTestId('main-content').getByLabel('Password')).toBeVisible();

  await page.getByRole('link', { name: 'Create your ShopSphere account' }).click();

  await expect(page).toHaveURL(/\/register$/);
  await expect(page.getByRole('heading', { name: 'Create account' })).toBeVisible();
  await expect(page.getByLabel('Display name')).toBeVisible();
  await expect(page.getByLabel('Confirm password')).toBeVisible();
  await expect(page.getByRole('contentinfo')).toContainText('Help & policies');
});
