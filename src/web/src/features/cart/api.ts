import { apiFetch } from '@/shared/lib/api-fetch';
import type { Cart } from './types';

export function fetchCart(): Promise<Cart> {
  return apiFetch<Cart>('/api/v1/cart/');
}

export function addCartItem(input: { productId: string; quantity: number }): Promise<Cart> {
  return apiFetch<Cart>('/api/v1/cart/items', {
    method: 'POST',
    json: input,
  });
}

export function updateCartItem(input: { productId: string; quantity: number }): Promise<Cart> {
  return apiFetch<Cart>(`/api/v1/cart/items/${input.productId}`, {
    method: 'PATCH',
    json: {
      quantity: input.quantity,
    },
  });
}

export function removeCartItem(productId: string): Promise<Cart> {
  return apiFetch<Cart>(`/api/v1/cart/items/${productId}`, {
    method: 'DELETE',
  });
}
