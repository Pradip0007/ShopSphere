import { expect, test } from './fixtures/pages';

test('user can add a product to the cart', async ({ authenticatedPage, products, cart }) => {
  await authenticatedPage.goto('/products');

  await products.addProductToCart('Wireless Headphones');

  await cart.open();

  await expect(cart.count()).toHaveText('1');
  await expect(cart.lines()).toHaveCount(1);
});

test('new user starts with an empty cart', async ({ authenticatedPage, cart }) => {
  await authenticatedPage.goto('/');

  await cart.open();

  await expect(cart.drawer()).toContainText('Your cart');
  await expect(cart.drawer()).toContainText('Your cart is empty.');
  await expect(cart.lines()).toHaveCount(0);
});

test('guest can browse products and open the cart without login', async ({ page }) => {
  await page.route('**/api/**', (route) => route.fulfill({ status: 401 }));

  await page.goto('/products');

  await expect(page).toHaveURL(/\/products$/);
  await expect(page.getByRole('heading', { name: 'Products' })).toBeVisible();

  await page.goto('/cart');

  await expect(page).toHaveURL(/\/cart$/);
  await expect(page.getByRole('heading', { name: 'Your cart' })).toBeVisible();

  await page.goto('/checkout');

  await expect(page).toHaveURL(/\/login\?redirect=%2Fcheckout/);
});

test('guest cart is preserved when checkout requires sign in', async ({
  page,
  request,
  products,
  cart,
}) => {
  const email = `guest-cart-${crypto.randomUUID()}@shopsphere.local`;
  const password = 'E2eTestPassword123!';
  const registerResponse = await request.post('https://localhost:7583/api/v1/auth/register', {
    data: { email, password },
    ignoreHTTPSErrors: true,
  });

  expect(registerResponse.ok()).toBeTruthy();

  await page.goto('/products');
  await products.addProductToCart('Wireless Headphones');
  await cart.open();

  await expect(cart.count()).toHaveText('1');
  await expect(cart.lines()).toHaveCount(1);

  await cart.drawer().getByRole('button', { name: 'Go to checkout' }).click();
  await expect(page).toHaveURL(/\/login\?redirect=%2Fcheckout/);

  await page.getByLabel('Email').fill(email);
  await page.getByLabel('Password').fill(password);
  await page.getByRole('button', { name: 'Sign in' }).click();

  await expect(page).toHaveURL(/\/checkout$/);
  await cart.open();

  await expect(cart.count()).toHaveText('1');
  await expect(cart.lines()).toHaveCount(1);
});

test('user can remove an item from the cart', async ({ authenticatedPage, products, cart }) => {
  await authenticatedPage.goto('/products');

  await products.addProductToCart('Wireless Headphones');

  await cart.open();

  await expect(cart.count()).toHaveText('1');
  await expect(cart.lines()).toHaveCount(1);

  await cart.removeFirstItem();

  await expect(cart.drawer()).toContainText('Your cart is empty.');
  await expect(cart.lines()).toHaveCount(0);
});
