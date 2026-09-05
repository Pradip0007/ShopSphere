export interface CartLine {
  productId: string;
  quantity: number;
}

export interface Cart {
  key: string;
  totalUnits: number;
  lines: CartLine[];
}
