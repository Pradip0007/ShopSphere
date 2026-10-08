import { useQuery } from '@tanstack/react-query';
import { cn } from '@/shared/lib/cn';
import { useAddToWishlist, useRemoveFromWishlist, wishlistQueryOptions } from './queries';

interface WishlistToggleProps {
  productId: string;
  className?: string;
}

export function WishlistToggle({ productId, className }: WishlistToggleProps): React.JSX.Element {
  const { data } = useQuery(wishlistQueryOptions());
  const addToWishlist = useAddToWishlist();
  const removeFromWishlist = useRemoveFromWishlist();

  const isInWishlist = data?.items.some((item) => item.productId === productId) ?? false;
  const isPending = addToWishlist.isPending || removeFromWishlist.isPending;

  function handleToggle(): void {
    if (isInWishlist) {
      removeFromWishlist.mutate(productId);
      return;
    }

    addToWishlist.mutate(productId);
  }

  return (
    <button
      type="button"
      aria-pressed={isInWishlist}
      aria-label={isInWishlist ? 'Remove from wishlist' : 'Add to wishlist'}
      disabled={isPending}
      onClick={handleToggle}
      className={cn(
        'grid size-10 place-items-center rounded-full',
        'transition-colors',
        'hover:bg-[var(--color-surface-muted)]',
        'focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-brand-500',
        'disabled:pointer-events-none disabled:opacity-60',
        className,
      )}
    >
      <svg
        viewBox="0 0 24 24"
        fill={isInWishlist ? 'currentColor' : 'none'}
        stroke="currentColor"
        strokeWidth="1.8"
        aria-hidden="true"
        className="size-5"
      >
        <path
          strokeLinecap="round"
          strokeLinejoin="round"
          d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78L12 21.23l8.84-8.84a5.5 5.5 0 0 0 0-7.78Z"
        />
      </svg>
    </button>
  );
}
