import React, { createContext, useContext, useCallback, useState, useEffect } from 'react';
import { Product, Hold } from '../types';
import * as api from '../services/api';

type InventoryContextType = {
  products: Product[];
  holds: Hold[];
  loading: boolean;
  refresh: () => Promise<void>;
  createHold: (productId: string, quantity: number) => Promise<Hold>;
  releaseHold: (holdId: string) => Promise<void>;
};

const InventoryContext = createContext<InventoryContextType | undefined>(undefined);

export const InventoryProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [products, setProducts] = useState<Product[]>([]);
  const [holds, setHolds] = useState<Hold[]>([]);
  const [loading, setLoading] = useState(false);

  const refresh = useCallback(async () => {
    setLoading(true);
    try {
      const p = await api.fetchInventory();
      setProducts(p);
      // active holds could be retrieved via API; here we keep local holds created during session
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    refresh();
  }, [refresh]);

  const createHold = useCallback(async (productId: string, quantity: number) => {
    const hold = await api.createHold(productId, quantity);
    setHolds(h => [hold, ...h]);
    // optimistic refresh inventory
    refresh();
    return hold;
  }, [refresh]);

  const releaseHold = useCallback(async (holdId: string) => {
    await api.releaseHold(holdId);
    setHolds(h => h.filter(x => x.holdId !== holdId));
    refresh();
  }, [refresh]);

  return (
    <InventoryContext.Provider value={{ products, holds, loading, refresh, createHold, releaseHold }}>
      {children}
    </InventoryContext.Provider>
  );
};

export const useInventory = () => {
  const ctx = useContext(InventoryContext);
  if (!ctx) throw new Error('useInventory must be used within InventoryProvider');
  return ctx;
};
