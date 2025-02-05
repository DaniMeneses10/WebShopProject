import { useEffect } from "react";
import { useSelector, useDispatch } from "react-redux";
import { fetchCart, removeFromCart, updateCartItem, clearCart } from "../store/cartSlice";
import { useState } from "react";
import api from "../services/api"; 

export default function Cart() {
  const dispatch = useDispatch();
  const { items: cartItems, totalAmount } = useSelector((state) => state.cart);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  // 🔹 Cargar el carrito al iniciar la página
  useEffect(() => {
    dispatch(fetchCart());
  }, [dispatch]);

  const handleQuantityChange = (productId, quantity) => {
    if (quantity < 1) return;
    dispatch(updateCartItem({ productId, quantity }));
  };

  const handleCheckout = async () => {
    if (cartItems.length === 0) {
      alert("Your cart is empty!");
      return;
    }

    setLoading(true);
    setError(null);
    const customerId = 1;

    try {
      const response = await api.post(`/shoppingCart/checkout/${customerId}`); // ✅ Ruta corregida
      console.log("Checkout response:", response.data);

      if (!response.data.OrderID) {
        throw new Error("Invalid Order Response");
      }

      alert(`Order placed successfully! Order ID: ${response.data.OrderID}`);
      dispatch(clearCart());
    } catch (err) {
      console.error("Checkout error:", err);
      setError("Failed to process order.");
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
                <th className="border p-2">Code</th>
                <th className="border p-2">Price</th>
                <th className="border p-2">Quantity</th>
                <th className="border p-2">Total</th>
                <th className="border p-2">Actions</th>
              </tr>
            </thead>
            <tbody>
              {cartItems.map((item) => (
                <tr key={item.productId} className="text-center">
                  <td className="border p-2">{item.name}</td>
                  <td className="border p-2">{item.code}</td>
                  <td className="border p-2">${item.price.toFixed(2)}</td>
                  <td className="border p-2">
                    <input
                      type="number"
                      value={item.quantity}
                      min="1"
                      onChange={(e) =>
                        handleQuantityChange(item.productId, parseInt(e.target.value))
                      }
                      className="w-16 p-1 border rounded"
                    />
                  </td>
                  <td className="border p-2">${(item.quantity * item.price).toFixed(2)}</td>
                  <td className="border p-2">
                    <button
                      onClick={() => dispatch(removeFromCart(item.productId))}
                      className="bg-red-500 text-white px-3 py-1 rounded"
                    >
                      Remove
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>

          <div className="mt-4 flex justify-between">
            <h2 className="text-xl font-bold">Total: ${totalAmount.toFixed(2)}</h2>
            <button
              onClick={handleCheckout}
              className={`px-4 py-2 text-white rounded ${loading ? "bg-gray-500" : "bg-green-500"}`}
              disabled={loading}
            >
              {loading ? "Processing..." : "Checkout"}
            </button>
          </div>

          {error && <p className="text-red-500 mt-2">{error}</p>}
        </>
      )}
    </div>
  );
}
