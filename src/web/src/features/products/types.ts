export interface Product {
  id: string;
  title: string;
  slug: string;
  sku: string;
  price: number;
  currency: string;
  categoryId: string;
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
