import { useQueryClient } from '@tanstack/react-query';
import { Link } from '@tanstack/react-router';
import { cn } from '@/shared/lib/cn';
import { productDetailQueryOptions } from './queries';
import type { Product } from './types';

interface ProductCardProps {
  product: Product;
  className?: string;
}

export function ProductCard({ product, className }: ProductCardProps): React.JSX.Element {
  const queryClient = useQueryClient();

  return (
    <Link
      to="/products/$slug"
      params={{ slug: product.slug }}
      onMouseEnter={() => {
        void queryClient.prefetchQuery(productDetailQueryOptions(product.slug));
      }}
      className={cn(
        'group grid gap-3 rounded-lg border p-3',
        'border-[var(--color-border)] bg-[var(--color-surface)]',
        'transition-shadow hover:shadow-md',
        'focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-brand-500',
        className,
      )}
    >
      <div className="grid aspect-square place-items-center rounded-md bg-[var(--color-surface-muted)]">
        <span className="text-sm text-[var(--color-text-muted)]">Product</span>
      </div>

      <div className="grid gap-1">
        <h3 className="line-clamp-2 text-sm font-medium">{product.title}</h3>

        <p className="text-base font-semibold">{formatPrice(product.price, product.currency)}</p>
      </div>
    </Link>
  );
}

function formatPrice(amount: number, currency: string): string {
  return new Intl.NumberFormat(undefined, {
    style: 'currency',
    currency,
  }).format(amount);
}
