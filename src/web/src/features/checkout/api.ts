import { apiFetch } from '@/shared/lib/api-fetch';

export interface CheckoutRequest {
  shippingAddress: {
    line1: string;
    line2?: string;
    city: string;
    postalCode: string;
    country: string;
  };
}

export interface CheckoutResponse {
  orderId: string;
  total: number;
  currency: string;
  lineCount: number;
}

export function submitCheckout(req: CheckoutRequest): Promise<CheckoutResponse> {
  return apiFetch<CheckoutResponse>('/api/v1/checkout', {
    method: 'POST',
    json: req,
  });
}
