import { useQueryClient } from '@tanstack/react-query';
import { Link } from '@tanstack/react-router';
import { useAddToCart } from '@/features/cart/queries';
import { WishlistToggle } from '@/features/wishlist/WishlistToggle';
import { cn } from '@/shared/lib/cn';
import { Button } from '@/shared/ui';
import { productDetailQueryOptions } from './queries';
import type { Product } from './types';

interface ProductCardProps {
  product: Product;
  className?: string;
}

export function ProductCard({ product, className }: ProductCardProps): React.JSX.Element {
  const queryClient = useQueryClient();
  const addToCart = useAddToCart();
  const listPrice = product.price / 0.8;

  return (
    <article
      className={cn(
        'group grid h-full gap-3 rounded-xl border p-3.5',
        'border-slate-200 bg-white shadow-sm',
        'transition duration-200 ease-out hover:-translate-y-1 hover:border-blue-200 hover:bg-blue-50/40 hover:shadow-lg hover:shadow-slate-900/10',
        className,
      )}
    >
      <Link
        to="/products/$slug"
        params={{ slug: product.slug }}
        data-testid="product-card"
        onMouseEnter={() => {
          void queryClient.prefetchQuery(productDetailQueryOptions(product.slug));
        }}
        className={cn(
          'grid gap-3 rounded-lg',
          'focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-brand-500',
        )}
      >
        <div className="relative grid aspect-square place-items-center overflow-hidden rounded-lg bg-slate-100 transition-colors duration-200 group-hover:bg-blue-100/70">
          <span className="text-sm font-medium text-slate-500">Product</span>
          <span className="absolute left-2.5 top-2.5 rounded-full bg-emerald-700 px-2.5 py-1 text-xs font-bold text-white shadow-sm">
            20% OFF
          </span>
        </div>

        <div className="grid gap-1">
          <h2
            data-testid="product-title"
            className="line-clamp-2 min-h-10 text-sm font-semibold text-slate-900 transition-colors group-hover:text-blue-800"
          >
            {product.title}
          </h2>

          <div className="flex flex-wrap items-baseline gap-x-2 gap-y-1">
            <p className="text-lg font-bold text-slate-950">
              {formatPrice(product.price, product.currency)}
            </p>
            <p className="text-sm text-slate-500">
              <span className="sr-only">Was </span>
              <s>{formatPrice(listPrice, product.currency)}</s>
            </p>
            <span className="text-xs font-semibold text-emerald-700">Save 20%</span>
          </div>
        </div>
      </Link>

      <div className="mt-auto flex items-center gap-2">
        <Button
          variant="primary"
          size="md"
          className="min-h-10 flex-1 rounded-lg bg-blue-700 font-semibold text-white shadow-sm hover:bg-blue-800 active:bg-blue-900"
          data-testid="product-add-to-cart"
          disabled={addToCart.isPending}
          onClick={() => {
            addToCart.mutate({
              productId: product.id,
              quantity: 1,
            });
          }}
        >
          {addToCart.isPending ? 'Adding…' : 'Add to cart'}
        </Button>
        <WishlistToggle
          productId={product.id}
          className="shrink-0 border border-slate-200 bg-white shadow-sm hover:border-pink-200 hover:bg-pink-50"
        />
      </div>
    </article>
  );
}

function formatPrice(amount: number, currency: string): string {
  return new Intl.NumberFormat(undefined, {
    style: 'currency',
    currency,
  }).format(amount);
}
