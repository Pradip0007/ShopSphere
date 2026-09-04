export interface Product {
  id: string;
  title: string;
  slug: string;
  sku: string;
  price: number;
  currency: string;
  categoryId: string;
}

export interface ProductAttribute {
  name: string;
  value: string;
}

export interface ProductDetail extends Product {
  category: string;
  images: string[];
  longDescription: string;
  stock: number;
  attributes: ProductAttribute[];
  shippingInfo: string;
  averageRating: number | null;
  ratingCount: number;
}

export interface Review {
  id: string;
  authorDisplayName: string;
  rating: number;
  title: string;
  body: string;
  createdUtc: string;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNext: boolean;
  hasPrevious: boolean;
}
