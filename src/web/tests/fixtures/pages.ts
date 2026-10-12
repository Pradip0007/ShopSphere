import { CartPage } from '../pages/cart.page';
import { ProductsPage } from '../pages/products.page';
import { test as base } from './base';

export const test = base.extend<{
  products: ProductsPage;
  cart: CartPage;
}>({
  products: async ({ page }, use) => {
    await use(new ProductsPage(page));
  },

  cart: async ({ page }, use) => {
    await use(new CartPage(page));
  },
});

export { expect } from './base';
