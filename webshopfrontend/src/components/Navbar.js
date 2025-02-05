import { useSelector } from "react-redux";
import { Link } from "react-router-dom";

export default function Navbar() {
  const cartItems = useSelector((state) => state.cart.items || []); // ✅ Asegurar que siempre sea un array
  const cartCount = cartItems.length || 0; // ✅ Evitar errores si `cartItems` es undefined

  return (
    <nav className="bg-blue-500 p-4 text-white flex justify-between">
      <Link to="/" className="text-lg font-bold">WebShop</Link>
      <div>
        <Link to="/cart" className="mr-4">Cart ({cartCount})</Link>
      </div>
    </nav>
  );
}
