import type { Page } from '@playwright/test';

export class CartDrawer {
  constructor(private readonly page: Page) {}

  async open(): Promise<void> {
    await this.page.getByTestId('cart-drawer-trigger').click();
  }

  count() {
    return this.page.getByTestId('cart-count');
  }

  lines() {
    return this.page.getByTestId('cart-line');
  }

  async removeFirstItem(): Promise<void> {
    await this.lines().first().getByTestId('cart-line-remove').click();
  }

  drawer() {
    return this.page.getByTestId('cart-drawer');
  }
}
