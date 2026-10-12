import { expect, test } from './fixtures/base';

test('product card highlights on hover and shows its sale price and wishlist state', async ({
  page,
}) => {
  let wishlisted = false;
  await page.route('**/api/**', async (route) => {
    const url = new URL(route.request().url());

    if (url.pathname === '/api/v1/products') {
      return route.fulfill({
        json: {
          items: [
            {
              id: 'product-1',
              title: 'Sample headphones',
              slug: 'sample-headphones',
              sku: 'SAMPLE-1',
              price: 100,
              currency: 'GBP',
              categoryId: 'electronics',
            },
          ],
          page: 1,
          pageSize: 24,
          totalCount: 1,
          totalPages: 1,
          hasNext: false,
          hasPrevious: false,
        },
      });
    }

    if (url.pathname === '/api/v1/wishlist/' && route.request().method() === 'GET') {
      return route.fulfill({
        json: {
          items: wishlisted
            ? [{ productId: 'product-1', addedAtUtc: new Date().toISOString() }]
            : [],
        },
      });
    }

    if (url.pathname === '/api/v1/wishlist/items' && route.request().method() === 'POST') {
      wishlisted = true;
      return route.fulfill({ status: 204 });
    }

    return route.fulfill({ status: 401 });
  });

  await page.goto('/products');

  const card = page.getByRole('listitem').filter({ hasText: 'Sample headphones' });
  const wishlist = card.getByRole('button', { name: 'Add to wishlist' });

  await expect(card.getByText('£100.00')).toBeVisible();
  await expect(card.getByText('£125.00')).toBeVisible();
  await expect(card.getByText('20% OFF')).toBeVisible();
  await expect(wishlist).toHaveAttribute('aria-pressed', 'false');
  await expect(wishlist.locator('svg')).toHaveCSS('fill', 'rgb(255, 255, 255)');

  await card.hover();
  const productArticle = card.locator('article');
  await expect
    .poll(() => productArticle.evaluate((element) => getComputedStyle(element).backgroundColor))
    .not.toBe('rgb(255, 255, 255)');

  await wishlist.click();
  await expect(card.getByRole('button', { name: 'Remove from wishlist' })).toHaveAttribute(
    'aria-pressed',
    'true',
  );
});

test('product detail has a complete preview when product photos and specifications are missing', async ({
  page,
}) => {
  await page.route('**/api/**', async (route) => {
    const url = new URL(route.request().url());

    if (url.pathname === '/api/v1/products/sample-detail') {
      return route.fulfill({
        json: {
          id: 'product-detail-1',
          title: 'Sample Studio Headphones',
          slug: 'sample-detail',
          sku: 'AUDIO-100',
          price: 80,
          currency: 'GBP',
          categoryId: 'audio',
          category: 'Audio',
          images: [],
          longDescription: 'Comfortable headphones for work, travel, and everyday listening.',
          stock: 4,
          attributes: [],
          shippingInfo: 'Delivery details are confirmed at checkout.',
          averageRating: null,
          ratingCount: 0,
        },
      });
    }

    if (url.pathname === '/api/v1/cart/' && route.request().method() === 'GET') {
      return route.fulfill({ json: { key: 'empty', totalUnits: 0, lines: [] } });
    }

    if (url.pathname === '/api/v1/cart/items' && route.request().method() === 'POST') {
      return route.fulfill({
        json: {
          key: 'cart-1',
          totalUnits: 2,
          lines: [{ productId: 'product-detail-1', quantity: 2 }],
        },
      });
    }

    if (url.pathname === '/api/v1/wishlist/') {
      return route.fulfill({ json: { items: [] } });
    }

    return route.fulfill({ status: 401 });
  });

  await page.goto('/products/sample-detail');

  await expect(page.getByRole('heading', { name: 'Sample Studio Headphones' })).toBeVisible();
  await expect(page.getByText('Sample product preview')).toBeVisible();
  await expect(page.getByText('20% sample offer')).toBeVisible();
  await expect(page.getByText('£80.00')).toBeVisible();
  await expect(page.getByText('£100.00')).toBeVisible();
  await expect(page.getByRole('heading', { name: 'Sample product highlights' })).toBeVisible();

  await page.getByLabel('Quantity').selectOption('2');
  await page.getByTestId('product-detail-add-to-cart').click();
  await expect(page.getByRole('status')).toHaveText('Added to your cart.');
});
