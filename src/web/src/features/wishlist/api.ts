import { apiFetch } from '@/shared/lib/api-fetch';
import type { WishlistResponse } from './types';

export function fetchWishlist(): Promise<WishlistResponse> {
  return apiFetch<WishlistResponse>('/api/v1/wishlist/');
}

export function addWishlistItem(productId: string): Promise<void> {
  return apiFetch<void>('/api/v1/wishlist/items', {
    method: 'POST',
    json: { productId },
  });
}

export function removeWishlistItem(productId: string): Promise<void> {
  return apiFetch<void>(`/api/v1/wishlist/items/${productId}`, {
    method: 'DELETE',
  });
}
