import type { Page } from '@playwright/test';

export class ProductsPage {
  constructor(private readonly page: Page) {}

  async goto(): Promise<void> {
    await this.page.goto('/products');
  }

  productCard(title: string) {
    return this.page
      .locator('article')
      .filter({ has: this.page.getByTestId('product-title').filter({ hasText: title }) });
  }

  async addProductToCart(title: string): Promise<void> {
    await this.productCard(title).getByTestId('product-add-to-cart').click();
  }
}
