import React, { useState } from 'react'
import { InventoryProvider, useInventory } from './context/InventoryContext'

function InventoryView() {
  const { products, loading, refresh } = useInventory();
  return (
    <div>
      <h2>Inventory</h2>
      {loading ? <div>Loading...</div> : (
        <ul>
          {products.map(p => (
            <li key={p.id}>{p.name}: {p.quantity}</li>
          ))}
        </ul>
      )}
      <button onClick={refresh}>Refresh</button>
    </div>
  )
}

function CreateHold() {
  const { products, createHold } = useInventory();
  const [productId, setProductId] = useState('');
  const [qty, setQty] = useState(1);

  return (
    <div>
      <h2>Create Hold</h2>
      <select value={productId} onChange={e => setProductId(e.target.value)}>
        <option value="">--select--</option>
        {products.map(p => <option key={p.id} value={p.id}>{p.name}</option>)}
      </select>
      <input type="number" value={qty} onChange={e => setQty(parseInt(e.target.value || '0'))} />
      <button onClick={async () => {
        if (!productId) return;
        try {
          await createHold(productId, qty);
        } catch (err: any) {
          alert(err?.response?.data?.error || err.message);
        }
      }}>Place Hold</button>
    </div>
  )
}

function HoldsView() {
  const { holds, releaseHold } = useInventory();

  return (
    <div>
      <h2>Active Holds</h2>
      <ul>
        {holds.map(h => (
          <li key={h.holdId}>{h.productId} x {h.quantity} — expires {new Date(h.expiresAt).toLocaleString()} <button onClick={() => releaseHold(h.holdId)}>Release</button></li>
        ))}
      </ul>
    </div>
  )
}

export default function App() {
  return (
    <InventoryProvider>
      <div style={{ padding: 20 }}>
        <h1>Inventory Hold Dashboard</h1>
        <InventoryView />
        <CreateHold />
        <HoldsView />
      </div>
    </InventoryProvider>
  )
}
