import React, { useState } from "react";

const ProductCard = ({ product, onAddToCart }) => {
  const [quantity, setQuantity] = useState(1);

  const handleAddToCart = () => {
    onAddToCart({
      productId: product.productId,
      name: product.name,
      price: product.price,
      quantity,
    });
  };

  return (
    <div className="product-card">
      <h3>{product.name}</h3>
      <p>€ {product.price.toFixed(2)}</p>
      <p>{product.stock} in stock</p>
      <div>
        <button onClick={() => setQuantity(Math.max(1, quantity - 1))}>-</button>
        <input
          type="number"
          value={quantity}
          onChange={(e) => setQuantity(Number(e.target.value))}
          min="1"
        />
        <button onClick={() => setQuantity(quantity + 1)}>+</button>
      </div>
      <button onClick={handleAddToCart}>Add to Cart</button>
    </div>
  );
};

export default ProductCard;
