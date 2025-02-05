import { createSlice, createAsyncThunk } from "@reduxjs/toolkit";
import api from "../services/api"; // Axios preconfigurado con `withCredentials: true`

// 🔹 Obtener la lista de productos desde el backend
export const fetchProducts = createAsyncThunk("products/fetch", async (_, { rejectWithValue }) => {
  try {
    const response = await api.get("/Products"); // ✅ Asegurar que el backend usa `/api/products`
    return response.data;
  } catch (error) {
    return rejectWithValue(error.response ? error.response.data : "Failed to fetch products");
  }
});

// 📌 **Estado inicial**
const initialState = {
  items: [],
  status: "idle",
  error: null,
};

// 📌 **Slice de Redux**
const productSlice = createSlice({
  name: "products",
  initialState,
  reducers: {},
  extraReducers: (builder) => {
    builder
      .addCase(fetchProducts.pending, (state) => {
        state.status = "loading";
      })
      .addCase(fetchProducts.fulfilled, (state, action) => {
        state.status = "succeeded";
        state.items = action.payload; // ✅ Ahora toma los datos correctamente
      })
      .addCase(fetchProducts.rejected, (state, action) => {
        state.status = "failed";
        state.error = action.payload || "Failed to load products.";
      });
  },
});

// ✅ **Exportar acción y reducer**
// export { fetchProducts };
export default productSlice.reducer;
