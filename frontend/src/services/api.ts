import axios from 'axios';
import { Product, Hold } from '../types';

export async function fetchInventory(): Promise<Product[]> {
  const res = await axios.get('/api/inventory');
  return res.data.map((p: any) => ({ id: p.id ?? p._id, name: p.name, quantity: p.quantity }));
}

export async function createHold(productId: string, quantity: number): Promise<Hold> {
  const res = await axios.post('/api/holds', { productId, quantity });
  return res.data;
}

export async function getHold(holdId: string): Promise<Hold> {
  const res = await axios.get(`/api/holds/${holdId}`);
  return res.data;
}

export async function releaseHold(holdId: string): Promise<void> {
  await axios.delete(`/api/holds/${holdId}`);
}
