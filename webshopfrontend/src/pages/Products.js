import { useEffect } from "react";
import { useDispatch, useSelector } from "react-redux";
import { fetchProducts } from "../store/productSlice";
import { addToCart } from "../store/cartSlice";

export default function Products() {
  const dispatch = useDispatch();
  const { items = [], status, error } = useSelector((state) => state.products); // ✅ Asegurar que items sea un array

  // 🔹 Cargar los productos al montar el componente
  useEffect(() => {
    dispatch(fetchProducts());
  }, [dispatch]);

  const handleAddToCart = (product) => {
    if (!product.Stock || product.Stock < 1) {
      alert("Out of stock!");
      return;
    }
    dispatch(addToCart({ ...product, quantity: 1 }));
  };

  // 🔹 Mostrar mensajes de carga y error
  if (status === "loading") return <p className="p-4 text-lg">Loading products...</p>;
  if (status === "failed") return <p className="p-4 text-lg text-red-500">Error: {error}</p>;

  return (
    <div className="p-4">
      <h1 className="text-xl font-bold mb-4">Product Catalog</h1>

      {items.length === 0 ? (
        <p className="text-gray-500">No products available.</p>
      ) : (
        <div className="grid grid-cols-3 gap-4">
          {items.map((product) => (
            <div key={product.ProductID} className="border p-4 shadow rounded">
              <h2 className="text-lg font-bold">{product.Name}</h2>
              <p className="text-sm text-gray-500">{product.Code}</p>
              <p className="text-lg font-semibold">
                ${product.Price ? product.Price.toFixed(2) : "0.00"} {/* ✅ Evitar error con `toFixed()` */}
              </p>
              <p className="text-sm">Stock: {product.Stock || 0}</p>
              <button
                className={`text-white px-4 py-2 mt-2 rounded ${
                  product.Stock > 0 ? "bg-blue-500 hover:bg-blue-600" : "bg-gray-500 cursor-not-allowed"
                }`}
                onClick={() => handleAddToCart(product)}
                disabled={!product.Stock || product.Stock < 1}
              >
                Add to Cart
              </button>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
