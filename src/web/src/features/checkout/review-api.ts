import { apiFetch } from '@/shared/lib/api-fetch';

export interface CheckoutReviewItem {
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  currency: string;
  lineTotal: number;
}

export interface CheckoutReview {
  items: CheckoutReviewItem[];
  subtotal: number;
  currency: string;
}

export function fetchCheckoutReview(): Promise<CheckoutReview> {
  return apiFetch<CheckoutReview>('/api/v1/checkout/review');
}
