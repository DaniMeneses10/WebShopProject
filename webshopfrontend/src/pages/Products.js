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

// 🔹 Obtener Productos
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
      console.error("Error: el producto no tiene productID definido", product);
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
    <div className="flex gap-6 p-6 max-w-screen-xl mx-auto">
      {/* Tabla de productos */}
      <div className="w-3/4">
        <h1 className="text-2xl font-bold mb-4">Product Catalog</h1>
        {items.length === 0 ? (
          <p className="text-gray-500">No products available.</p>
        ) : (
          <table className="w-full border-collapse border border-gray-300">
            <thead>
              <tr className="bg-gray-200">
                <th className="border p-2">Product</th>
                <th className="border p-2">Code</th>
                <th className="border p-2">Price</th>
                <th className="border p-2">Stock</th>
                <th className="border p-2">Actions</th>
              </tr>
            </thead>
            <tbody>
              {items.map((product) => (
                <tr key={product.productID} className="text-center">
                  <td className="border p-2">{product.name}</td>
                  <td className="border p-2">{product.code}</td>
                  <td className="border p-2">
                    ${product.price ? product.price.toFixed(2) : "0.00"}
                  </td>
                  <td
                    className={`border p-2 ${
                      product.stock > 0 ? "text-green-600" : "text-red-500"
                    }`}
                  >
                    {product.stock || 0}
                  </td>
                  <td className="border p-2">
                    <button
                      className={`text-white px-4 py-2 rounded ${
                        product.stock > 0
                          ? "bg-blue-500 hover:bg-blue-600"
                          : "bg-gray-500 cursor-not-allowed"
                      }`}
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

      {/* Carrito de compras */}
      <div className="w-1/4 bg-gray-100 p-4 shadow-md rounded-md h-fit sticky top-6">
        <Cart />
      </div>
    </div>
  );
}
