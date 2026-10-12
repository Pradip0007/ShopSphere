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

  await expect(cart.content()).toContainText('Your cart');
  await expect(cart.content()).toContainText('Your cart is empty.');
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

test('cart page shows product information and order pricing', async ({ page }) => {
  await page.route('**/api/**', (route) => {
    const url = new URL(route.request().url());

    if (url.pathname === '/api/v1/cart/') {
      return route.fulfill({
        json: {
          key: 'cart-test',
          totalUnits: 2,
          lines: [{ productId: 'product-1', quantity: 2 }],
        },
      });
    }

    if (url.pathname === '/api/v1/products') {
      return route.fulfill({
        json: {
          items: [
            {
              id: 'product-1',
              title: 'Sample headphones',
              slug: 'sample-headphones',
              sku: 'SAMPLE-1',
              price: 25,
              currency: 'GBP',
              categoryId: 'electronics',
            },
          ],
          page: 1,
          pageSize: 100,
          totalCount: 1,
          totalPages: 1,
          hasNext: false,
          hasPrevious: false,
        },
      });
    }

    return route.fulfill({ status: 401 });
  });

  await page.goto('/products');
  await page.getByTestId('cart-page-link').click();

  await expect(page).toHaveURL(/\/cart$/);
  await expect(page.getByRole('heading', { name: 'Your cart' })).toBeVisible();
  await expect(page.getByRole('heading', { name: 'Sample headphones' })).toBeVisible();
  await expect(page.getByRole('heading', { name: 'Price Details' })).toBeVisible();
  await expect(page.getByText('£50.00').first()).toBeVisible();
  await expect(page.getByRole('button', { name: 'Place order' })).toBeVisible();
  await expect(page.getByTestId('cart-drawer')).toHaveCount(0);
});

test('guest can search products from the header', async ({ page }) => {
  await page.route('**/api/**', (route) => route.fulfill({ status: 401 }));
  await page.goto('/');

  await page.getByRole('searchbox', { name: 'Search ShopSphere' }).fill('wireless headphones');
  await page.getByRole('button', { name: 'Search' }).click();

  await expect(page).toHaveURL(/\/products\?q=wireless(\+|%20)headphones/);
  await expect(page.getByRole('heading', { name: 'Products' })).toBeVisible();
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

  await cart.content().getByRole('button', { name: 'Place order' }).click();
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

  await expect(cart.content()).toContainText('Your cart is empty.');
  await expect(cart.lines()).toHaveCount(0);
});
