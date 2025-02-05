import { createSlice, createAsyncThunk } from "@reduxjs/toolkit";
import api from "../services/api"; // Configurado con `withCredentials: true`

// 🔹 Obtener carrito desde la API
export const fetchCart = createAsyncThunk("cart/fetchCart", async () => {
  const response = await api.get("/shoppingCart"); // ✅ Ruta corregida
  return response.data.items || []; // Asegurar que devuelve un array vacío si el carrito está vacío
});

// 🔹 Agregar producto al carrito
export const addToCart = createAsyncThunk("cart/addToCart", async (product) => {
  await api.post("/shoppingCart/add", product); // ✅ Ruta corregida
  return product;
});

// 🔹 Actualizar cantidad de un producto en el carrito
export const updateCartItem = createAsyncThunk("cart/updateCartItem", async ({ productId, quantity }) => {
  await api.put(`/shoppingCart/update/${productId}`, quantity); // ✅ Ruta corregida
  return { productId, quantity };
});

// 🔹 Eliminar producto del carrito
export const removeFromCart = createAsyncThunk("cart/removeFromCart", async (productId) => {
  await api.delete(`/shoppingCart/remove/${productId}`); // ✅ Ruta corregida
  return productId;
});

// 🔹 Vaciar el carrito
export const clearCart = createAsyncThunk("cart/clearCart", async () => {
  await api.delete("/shoppingCart/clear"); // ✅ Ruta corregida
  return [];
});

// 📌 **Estado inicial**
const initialState = {
  items: [],
  totalQuantity: 0,
  totalPrice: 0,
};

// 📌 **Slice de Redux**
const cartSlice = createSlice({
  name: "cart",
  initialState,
  reducers: {},
  extraReducers: (builder) => {
    builder
      .addCase(fetchCart.fulfilled, (state, action) => {
        state.items = action.payload;
        state.totalQuantity = action.payload.reduce((total, item) => total + item.quantity, 0);
        state.totalPrice = action.payload.reduce((total, item) => total + item.quantity * item.price, 0);
      })
      .addCase(addToCart.fulfilled, (state, action) => {
        state.items.push(action.payload);
      })
      .addCase(updateCartItem.fulfilled, (state, action) => {
        const item = state.items.find(i => i.productId === action.payload.productId);
        if (item) item.quantity = action.payload.quantity;
      })
      .addCase(removeFromCart.fulfilled, (state, action) => {
        state.items = state.items.filter(i => i.productId !== action.payload);
      })
      .addCase(clearCart.fulfilled, (state) => {
        state.items = [];
      });
  },
});

// ✅ **Exportar acciones**
// export { fetchCart, addToCart, updateCartItem, removeFromCart, clearCart };
export default cartSlice.reducer;
