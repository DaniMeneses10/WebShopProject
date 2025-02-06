import React, { useEffect } from "react";
import { useDispatch, useSelector } from "react-redux";
import { createSlice, createAsyncThunk } from "@reduxjs/toolkit";
import Cart from "./Cart";
import api from "../services/api";
import { addToCart } from "./Cart";

const productSlice = createSlice({
  name: "products",
  initialState: { items: [], status: "idle", error: null },
  reducers: {},
  extraReducers: (builder) => {
    builder
      .addCase(fetchProducts.pending, (state) => { state.status = "loading"; })
      .addCase(fetchProducts.fulfilled, (state, action) => { state.status = "succeeded"; state.items = action.payload; })
      .addCase(fetchProducts.rejected, (state, action) => { state.status = "failed"; state.error = action.payload; });
  },
});

// 🔹 Obtener Productos
export const fetchProducts = createAsyncThunk("products/fetch", async (_, { rejectWithValue }) => {
  try {
    const response = await api.get("/products");
    return response.data;
  } catch (error) {
    return rejectWithValue(error.response ? error.response.data : "Failed to fetch products");
  }
});

export const { reducer: productsReducer } = productSlice;

export default function Products() {
  const dispatch = useDispatch();
  const { items = [], status, error } = useSelector((state) => state.products);

  useEffect(() => {
    dispatch(fetchProducts());
  }, [dispatch]);

  const handleAddToCart = (product) => {
    console.log("Agregando producto:", product);
    if (!product.productID) {
      console.error("Error: el producto no tiene productID definido", product);
      return;
    }
  
    if (!product.stock || product.stock < 1) {
      alert("Out of stock!");
      return;
    }
  
    dispatch(addToCart(product));
  };
  

  if (status === "loading") return <p className="p-4 text-lg">Loading products...</p>;
  if (status === "failed") return <p className="p-4 text-lg text-red-500">Error: {error}</p>;

  return (
    <div className="flex">
      <div className="w-3/4 p-4">
        <h1 className="text-xl font-bold mb-4">Product Catalog</h1>
        {items.length === 0 ? (
          <p className="text-gray-500">No products available.</p>
        ) : (
          <div className="grid grid-cols-3 gap-4">
            {items.map((product) => {
              if (!product.productID) {
                console.warn("Producto sin ProductID detectado:", product);
              }
              return (
                <div key={product.productID} className="border p-4 shadow rounded">
                  <h2 className="text-lg font-bold">{product.name}</h2>
                  <p className="text-sm text-gray-500">Code: {product.code}</p>
                  <p className="text-lg font-semibold">
                    ${product.price ? product.price.toFixed(2) : "0.00"}
                  </p>
                  <p className="text-sm">Stock: {product.stock || 0}</p>
                  <button
                    className={`text-white px-4 py-2 mt-2 rounded ${
                      product.stock > 0 ? "bg-blue-500 hover:bg-blue-600" : "bg-gray-500 cursor-not-allowed"
                    }`}
                    onClick={() => handleAddToCart(product)}
                    disabled={!product.stock || product.stock < 1}
                  >
                    Add to Cart
                  </button>
                </div>
              );
            })}
          </div>
        )}
      </div>
      <div className="w-1/4 p-4 bg-gray-100">
        <Cart />
      </div>
    </div>
  );
}