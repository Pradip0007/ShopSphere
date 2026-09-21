import { expect, test } from './fixtures/pages';
import { CheckoutPage } from './pages/checkout.page';

test.describe('Checkout', () => {
  test('registered customer can reach the payment step', async ({
    authenticatedPage,
    products,
  }) => {
    await authenticatedPage.goto('/products');

    await products.addProductToCart('Wireless Headphones');

    const checkout = new CheckoutPage(authenticatedPage);
    await checkout.goto();

    await expect(checkout.form()).toBeVisible();
    await checkout.fillShippingAddress();
    await checkout.continueFromAddress();

    await expect(
      authenticatedPage.getByRole('heading', { name: 'Payment', level: 2 }),
    ).toBeVisible();

    await checkout.fillStripeCard();

    await checkout.useCard();

    await expect(authenticatedPage.getByText('Card ready. Continue to review.')).toBeVisible();

    await checkout.continueFromPayment();

    await expect(
      authenticatedPage.getByRole('heading', { name: 'Review', level: 2 }),
    ).toBeVisible();

    await authenticatedPage
      .getByRole('checkbox', {
        name: 'I agree to the terms of sale.',
      })
      .check();

    await authenticatedPage.getByRole('button', { name: 'Place order' }).click();

    await authenticatedPage.waitForURL(/\/orders\/[^/]+$/);

    const orderNumber = authenticatedPage.getByTestId('order-number');

    await expect(orderNumber).toBeVisible();
    await expect(orderNumber).toHaveText(/^Order .+/);
  });
});
