import type { Page } from '@playwright/test';

export class CheckoutPage {
  constructor(private readonly page: Page) {}

  async goto(): Promise<void> {
    await this.page.goto('/checkout');
  }

  shippingField(testId: string) {
    return this.page.getByTestId(testId);
  }

  async fillShippingAddress(): Promise<void> {
    await this.shippingField('ship-full-name').fill('E2E Test Customer');
    await this.shippingField('ship-address').fill('123 Test Street');
    await this.shippingField('ship-city').fill('Kolkata');
    await this.shippingField('ship-postal').fill('700001');
    await this.shippingField('ship-country').fill('IN');
  }

  async continueFromAddress(): Promise<void> {
    await this.page.getByRole('button', { name: 'Continue' }).click();
  }

  async fillStripeCard(cardNumber = '4242 4242 4242 4242'): Promise<void> {
    const stripeFrame = this.page.getByTestId('stripe-card-element').frameLocator('iframe');

    await stripeFrame.getByPlaceholder('Card number').fill(cardNumber);
    await stripeFrame.getByPlaceholder('MM / YY').fill('12 / 34');
    await stripeFrame.getByPlaceholder('CVC').fill('123');
  }

  async useCard(): Promise<void> {
    await this.page.getByTestId('payment-use-card').click();
  }

  async continueFromPayment(): Promise<void> {
    await this.page.getByRole('button', { name: 'Continue' }).click();
  }

  form() {
    return this.page.getByTestId('checkout-form');
  }
}
