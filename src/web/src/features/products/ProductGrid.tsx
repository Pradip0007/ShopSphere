import { ProductCard } from './ProductCard';
import type { Product } from './types';

interface ProductGridProps {
  items: Product[];
}

export function ProductGrid({ items }: ProductGridProps): React.JSX.Element {
  return (
    <ul
      className="grid list-none gap-4 p-0"
      style={{
        gridTemplateColumns: 'repeat(auto-fill, minmax(220px, 1fr))',
      }}
    >
      {items.map((product) => (
        <li key={product.id}>
          <ProductCard product={product} />
        </li>
      ))}
    </ul>
  );
}
