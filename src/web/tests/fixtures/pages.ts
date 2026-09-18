import { CartDrawer } from '../pages/cart-drawer.page';
import { ProductsPage } from '../pages/products.page';
import { test as base } from './base';

export const test = base.extend<{
  products: ProductsPage;
  cart: CartDrawer;
}>({
  products: async ({ page }, use) => {
    await use(new ProductsPage(page));
  },

  cart: async ({ page }, use) => {
    await use(new CartDrawer(page));
  },
});

export { expect } from './base';
