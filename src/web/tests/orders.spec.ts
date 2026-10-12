import { expect, test } from './fixtures/base';

test('orders page supports searching and filtering recent orders', async ({
  authenticatedPage,
}) => {
  await authenticatedPage.route('**/api/v1/orders?*', (route) =>
    route.fulfill({
      json: {
        items: [
          {
            id: 'delivered-order',
            number: 'ORD-1001',
            status: 'Delivered',
            placedUtc: new Date().toISOString(),
            totalAmount: 48,
            currency: 'GBP',
            itemCount: 1,
          },
          {
            id: 'shipped-order',
            number: 'ORD-1002',
            status: 'Shipped',
            placedUtc: new Date().toISOString(),
            totalAmount: 75,
            currency: 'GBP',
            itemCount: 2,
          },
        ],
        page: 1,
        pageSize: 20,
        totalCount: 2,
        totalPages: 1,
        hasNext: false,
        hasPrevious: false,
      },
    }),
  );

  await authenticatedPage.goto('/orders');

  await expect(authenticatedPage.getByRole('heading', { name: 'My Orders' })).toBeVisible();
  await expect(authenticatedPage.getByText('Order ORD-1001')).toBeVisible();
  await expect(authenticatedPage.getByText('Order ORD-1002')).toBeVisible();

  await authenticatedPage.getByLabel('Delivered', { exact: true }).check();
  await expect(authenticatedPage.getByText('Order ORD-1001')).toBeVisible();
  await expect(authenticatedPage.getByText('Order ORD-1002')).toHaveCount(0);

  await authenticatedPage.getByLabel('Delivered', { exact: true }).uncheck();
  await authenticatedPage.getByRole('searchbox', { name: 'Search your orders' }).fill('1002');
  await authenticatedPage.getByRole('button', { name: 'Search Orders' }).click();

  await expect(authenticatedPage.getByText('Order ORD-1001')).toHaveCount(0);
  await expect(authenticatedPage.getByText('Order ORD-1002')).toBeVisible();
});
