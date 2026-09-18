import { expect, test } from './fixtures/base';

test('authenticated user can open the home page', async ({ authenticatedPage }) => {
  await expect(authenticatedPage.getByTestId('nav-products')).toBeVisible();
  await expect(authenticatedPage.getByTestId('cart-drawer-trigger')).toBeVisible();
});
