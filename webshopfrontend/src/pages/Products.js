import React, { useEffect } from "react";
import { useDispatch, useSelector } from "react-redux";
import { createSlice, createAsyncThunk } from "@reduxjs/toolkit";
import Cart from "./Cart";
import api from "../services/api";
import { addToCart } from "./Cart";

import "../styles.css"; 

const productSlice = createSlice({
  name: "products",
  initialState: { items: [], status: "idle", error: null },
  reducers: {},
  extraReducers: (builder) => {
    builder
      .addCase(fetchProducts.pending, (state) => {
        state.status = "loading";
      })
      .addCase(fetchProducts.fulfilled, (state, action) => {
        state.status = "succeeded";
        state.items = action.payload;
      })
      .addCase(fetchProducts.rejected, (state, action) => {
        state.status = "failed";
        state.error = action.payload;
      });
  },
});

// Fetch Products
export const fetchProducts = createAsyncThunk(
  "products/fetch",
  async (_, { rejectWithValue }) => {
    try {
      const response = await api.get("/products");
      return response.data;
    } catch (error) {
      return rejectWithValue(
        error.response ? error.response.data : "Failed to fetch products"
      );
    }
  }
);

export const { reducer: productsReducer } = productSlice;

export default function Products() {
  const dispatch = useDispatch();
  const { items = [], status, error } = useSelector((state) => state.products);

  useEffect(() => {
    dispatch(fetchProducts());
  }, [dispatch]);

  const handleAddToCart = (product) => {
    if (!product.productID) {
      console.error("Error: The product does not have a productID defined", product);
      return;
    }

    if (!product.stock || product.stock < 1) {
      alert("Out of stock!");
      return;
    }

    dispatch(addToCart(product));
  };

  if (status === "loading")
    return <p className="p-4 text-lg">Loading products...</p>;
  if (status === "failed")
    return <p className="p-4 text-lg text-red-500">Error: {error}</p>;

  return (
    <div className="container">
      {/* Product Table */}
      <div className="product-table-container">
        <h1 className="title">Product Catalog</h1>
        {items.length === 0 ? (
          <p className="no-products">No products available.</p>
        ) : (
          <table className="product-table">
            <thead>
              <tr className="header-row">
                <th>Product</th>
                <th>Code</th>
                <th>Price</th>
                <th>Stock</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {items.map((product) => (
                <tr key={product.productID} className="text-center">
                  <td>{product.name}</td>
                  <td>{product.code}</td>
                  <td>${product.price ? product.price.toFixed(2) : "0.00"}</td>
                  <td className={product.stock > 0 ? "text-green" : "text-red"}>
                    {product.stock || 0}
                  </td>
                  <td>
                    <button
                      className={`btn-primary ${product.stock > 0 ? "" : "btn-disabled"}`}
                      onClick={() => handleAddToCart(product)}
                      disabled={!product.stock || product.stock < 1}
                    >
                      Add to Cart
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {/* Shopping Cart - Positioned to the right */}
      <div className="cart-container">
        <Cart />
      </div>
    </div>
  );
}
