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
