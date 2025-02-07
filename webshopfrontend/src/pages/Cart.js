import React, { useEffect, useState } from "react";
import { useSelector, useDispatch } from "react-redux";
import { createSlice, createAsyncThunk } from "@reduxjs/toolkit";
import api from "../services/api";

// 🔹 Obtener Carrito
export const fetchCart = createAsyncThunk("cart/fetch", async () => {
  const response = await api.get("/ShoppingCart");
  return response.data.items || [];
});

// 🔹 Agregar al Carrito
export const addToCart = createAsyncThunk("cart/addToCart", async (product) => {
  await api.post("/ShoppingCart/add", product);
  return product;
});

// 🔹 Eliminar del Carrito
export const removeFromCart = createAsyncThunk("cart/removeFromCart", async (productId) => {
  await api.delete(`/ShoppingCart/remove/${productId}`);
  return productId;
});

// 🔹 Limpiar Carrito
export const clearCart = createAsyncThunk("cart/clearCart", async () => {
  await api.delete("/ShoppingCart/clear");
  return [];
});

// 🔹 Slice del Carrito
const cartSlice = createSlice({
  name: "cart",
  initialState: { items: [], totalAmount: 0 },
  reducers: {
    clearCart: (state) => {
      state.items = [];
      state.totalAmount = 0;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchCart.fulfilled, (state, action) => {
        state.items = action.payload;
      })
      .addCase(addToCart.fulfilled, (state, action) => {
        console.log("Producto agregado:", action.payload);

        const existingItem = state.items.find((item) => item.productID === action.payload.productID);

        if (existingItem) {
          existingItem.quantity += 1;
        } else {
          state.items.push({ ...action.payload, quantity: 1 });
        }
      })
      .addCase(removeFromCart.fulfilled, (state, action) => {
        console.log("Producto eliminado:", action.payload);
        state.items = state.items.filter((item) => item.productID !== action.payload);
      }); 
  },
});

export const { reducer: cartReducer, actions: cartActions } = cartSlice;

export default function Cart() {
  const dispatch = useDispatch();
  const { items: cartItems } = useSelector((state) => state.cart);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    dispatch(fetchCart());
  }, [dispatch]);

  const handleCheckout = async () => {
    if (cartItems.length === 0) {
      alert("Your cart is empty!");
      return;
    }

    setLoading(true);

    try {
      const response = await api.post(`/ShoppingCart/checkout/1`);
      console.log("Backend response:", response.data);
      if (!response.data.orderID) throw new Error("Invalid Order Response");

      alert(`Order placed successfully! Order ID: ${response.data.orderID}`);
      dispatch(clearCart());
    } catch (err) {
      console.error("Checkout error:", err);
    }

    setLoading(false);
  };

  return (
    <div className="p-4">
      <h1 className="text-xl font-bold">Shopping Cart</h1>
      {cartItems.length === 0 ? (
        <p>Your cart is empty.</p>
      ) : (
        <>
          <table className="w-full border-collapse border border-gray-300 mt-4">
            <thead>
              <tr className="bg-gray-200">
                <th className="border p-2">Product</th>
                <th className="border p-2">Quantity</th>
                <th className="border p-2">Total</th>
                <th className="border p-2">Actions</th>
              </tr>
            </thead>
            <tbody>
              {cartItems.map((item) => (
                <tr key={item.productID}>
                  <td className="border p-2">{item.name}</td>
                  <td className="border p-2">{item.quantity}</td>
                  <td className="border p-2">${(item.quantity * item.price).toFixed(2)}</td>
                  <td>
                    <button 
                      onClick={() => dispatch(removeFromCart(item.productID))} 
                      className="bg-red-500 text-white px-3 py-1 rounded"
                    >
                      Remove
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          <button onClick={handleCheckout} disabled={loading} className="bg-green-500 text-white px-4 py-2 mt-4 rounded">
            {loading ? "Processing..." : "Checkout"}
          </button>
        </>
      )}
    </div>
  );
}
