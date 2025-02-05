import React from "react";

const CartItem = ({ item, onUpdateQuantity, onRemove }) => {
  return (
    <div className="cart-item">
      <h4>{item.name}</h4>
      <p>€ {item.price.toFixed(2)}</p>
      <div>
        <button
          onClick={() =>
            onUpdateQuantity(item.productId, Math.max(1, item.quantity - 1))
          }
        >
          -
        </button>
        <input
          type="number"
          value={item.quantity}
          onChange={(e) =>
            onUpdateQuantity(item.productId, Number(e.target.value))
          }
        />
        <button onClick={() => onUpdateQuantity(item.productId, item.quantity + 1)}>
          +
        </button>
      </div>
      <button onClick={() => onRemove(item.productId)}>Remove</button>
    </div>
  );
};

export default CartItem;
