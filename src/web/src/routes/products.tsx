import { useInfiniteQuery } from '@tanstack/react-query';
import { createFileRoute } from '@tanstack/react-router';
import { useMemo } from 'react';
import { z } from 'zod';
import { ProductGridSkeleton } from '@/features/products/ProductCardSkeleton';
import { ProductGrid } from '@/features/products/ProductGrid';
import { productsInfiniteQueryOptions } from '@/features/products/queries';
import { useIntersect } from '@/shared/lib/use-intersect';
import { Button } from '@/shared/ui';

const productsSearchSchema = z.object({
  category: z.string().optional(),
  sort: z.enum(['price-asc', 'price-desc']).default('price-asc').catch('price-asc'),
});

export const Route = createFileRoute('/products')({
  validateSearch: productsSearchSchema,
  component: ProductsPage,
});

function ProductsPage(): React.JSX.Element {
  const { category, sort } = Route.useSearch();

  const query = useInfiniteQuery(
    productsInfiniteQueryOptions({
      ...(category !== undefined ? { category } : {}),
      sort,
      pageSize: 24,
    }),
  );

  const items = useMemo(() => query.data?.pages.flatMap((page) => page.items) ?? [], [query.data]);

  const sentinelRef = useIntersect<HTMLDivElement>({
    onIntersect: () => {
      if (query.hasNextPage && !query.isFetchingNextPage) {
        void query.fetchNextPage();
      }
    },
    enabled: query.hasNextPage && !query.isPending,
  });

  if (query.isPending) {
    return (
      <section className="grid gap-6">
        <h1 className="text-2xl font-semibold">Products</h1>

        <ProductGridSkeleton />
      </section>
    );
  }

  if (query.isError) {
    return (
      <section className="grid max-w-md gap-4">
        <h1 className="text-2xl font-semibold">Products</h1>

        <p role="alert" className="text-[var(--color-danger)]">
          Could not load products: {query.error.message}
        </p>

        <Button onClick={() => void query.refetch()}>Try again</Button>
      </section>
    );
  }

  const totalLoaded = items.length;
  const totalAvailable = query.data.pages[0]?.totalCount ?? totalLoaded;

  return (
    <section className="grid gap-6">
      <header className="flex items-baseline justify-between">
        <h1 className="text-2xl font-semibold">Products</h1>

        <p className="text-sm text-[var(--color-text-muted)]">
          Showing {totalLoaded} of {totalAvailable}
        </p>
      </header>

      {items.length === 0 ? (
        <p className="text-[var(--color-text-muted)]">No products found.</p>
      ) : (
        <>
          <ProductGrid items={items} />

          <div ref={sentinelRef} aria-hidden="true" className="h-1" />

          {query.isFetchingNextPage && (
            <p className="text-center text-sm text-[var(--color-text-muted)]">Loading more…</p>
          )}

          {!query.hasNextPage && totalLoaded > 0 && (
            <p className="text-center text-sm text-[var(--color-text-muted)]">
              You've reached the end.
            </p>
          )}
        </>
      )}
    </section>
  );
}
