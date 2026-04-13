export type Product = {
  id: string;
  name: string;
  quantity: number;
};

export type Hold = {
  holdId: string;
  productId: string;
  quantity: number;
  expiresAt: string; // ISO
};
