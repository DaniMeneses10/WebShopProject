import React, { useEffect, useState } from "react";
import { useSelector, useDispatch } from "react-redux";
import { createSlice, createAsyncThunk } from "@reduxjs/toolkit";
import api from "../services/api";
import { fetchProducts } from "./Products"; // Import fetchProducts to refresh stock

import "../styles.css"; 

// Fetch Cart Items
export const fetchCart = createAsyncThunk("cart/fetch", async () => {
  const response = await api.get("/ShoppingCart");
  return response.data.items || [];
});

// Add Item to Cart
export const addToCart = createAsyncThunk("cart/addToCart", async (product) => {
  await api.post("/ShoppingCart/add", product);
  return product;
});

// Remove Item from Cart
export const removeFromCart = createAsyncThunk("cart/removeFromCart", async (productId) => {
  await api.delete(`/ShoppingCart/remove/${productId}`);
  return productId;
});

// Clear Cart
export const clearCart = createAsyncThunk("cart/clearCart", async () => {
  await api.delete("/ShoppingCart/clear");
  return [];
});

// Cart Slice
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
        console.log("Item added:", action.payload);
        const existingItem = state.items.find((item) => item.productID === action.payload.productID);

        if (existingItem) {
          existingItem.quantity += 1;
        } else {
          state.items.push({ ...action.payload, quantity: 1 });
        }
      })
      .addCase(removeFromCart.fulfilled, (state, action) => {
        console.log("Item removed:", action.payload);
        state.items = state.items.filter((item) => item.productID !== action.payload);
      })
      .addCase(clearCart.fulfilled, (state) => {
        state.items = [];
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
      //Supose customer ID is 1
      const customerID = 1;
      const response = await api.post(`/ShoppingCart/checkout/${customerID}`);
      console.log("Backend response:", response.data);

      if (!response.data.orderID) throw new Error("Invalid Order Response");

      // Show alert with order details
      alert(`✅ Order placed successfully!\n🛒 Order ID: ${response.data.orderID}\n💰 Amount: $${response.data.amount}`);

      // Clear cart after checkout
      await dispatch(clearCart());
      await dispatch(fetchCart());
      dispatch(fetchProducts());  // 🔄 Reload products to update stock

    } catch (err) {
      console.error("Checkout error:", err);
      alert("❌ Error processing order. Please try again.");
    }

    setLoading(false);
  };

  return (
    <div>
      <h1 className="cart-title">Shopping Cart</h1>
      {cartItems.length === 0 ? (
        <p className="cart-empty">Your cart is empty.</p>
      ) : (
        <>
          <table className="cart-table">
            <thead>
              <tr>
                <th>Product</th>
                <th>Quantity</th>
                <th>Total</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {cartItems.map((item) => (
                <tr key={item.productID}>
                  <td>{item.name}</td>
                  <td>{item.quantity}</td>
                  <td>${(item.quantity * item.price).toFixed(2)}</td>
                  <td>
                    <button 
                      onClick={() => dispatch(removeFromCart(item.productID))} 
                      className="btn-remove"
                    >
                      Remove
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          <button 
            onClick={handleCheckout} 
            disabled={loading} 
            className="btn-checkout"
          >
            {loading ? "Processing..." : "Checkout"}
          </button>
        </>
      )}
    </div>
  );
}
