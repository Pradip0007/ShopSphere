export interface WishlistItem {
  productId: string;
  addedAtUtc: string;
}

export interface WishlistResponse {
  items: WishlistItem[];
}
