import { configureStore } from "@reduxjs/toolkit";
import { productsReducer } from "./pages/Products";
import { cartReducer } from "./pages/Cart"; 

const store = configureStore({
  reducer: {
    products: productsReducer,
    cart: cartReducer
  }
});

export default store;