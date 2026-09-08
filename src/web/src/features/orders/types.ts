export type OrderStatus =
  | 'Pending'
  | 'Confirmed'
  | 'Paid'
  | 'Shipped'
  | 'Delivered'
  | 'Cancelled'
  | 'Refunded';

export interface OrderSummary {
  id: string;
  number: string;
  status: OrderStatus;
  placedUtc: string;
  totalAmount: number;
  currency: string;
  itemCount: number;
}

export interface OrderLine {
  productId: string;
  productSlug: string;
  productName: string;
  imageUrl: string | null;
  quantity: number;
  unitPriceAmount: number;
  lineTotalAmount: number;
  currency: string;
}

export interface OrderAddress {
  fullName: string;
  line1: string;
  line2?: string;
  city: string;
  postalCode: string;
  country: string;
  phone?: string;
}

export interface OrderDetail {
  id: string;
  number: string;
  status: OrderStatus;
  placedUtc: string;
  updatedUtc: string;
  lines: OrderLine[];
  subtotalAmount: number;
  shippingAmount: number | null;
  taxAmount: number | null;
  totalAmount: number;
  currency: string;
  shippingAddress: OrderAddress;
  billingAddress: OrderAddress | null;
  trackingNumber?: string;
}
