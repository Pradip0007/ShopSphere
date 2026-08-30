import { useQuery } from '@tanstack/react-query';
import { createFileRoute, Outlet } from '@tanstack/react-router';
import { z } from 'zod';
import { productsQueryOptions } from '@/features/products/queries';

const productsSearchSchema = z.object({
  category: z.string().optional(),
  sort: z.enum(['price-asc', 'price-desc']).default('price-asc').catch('price-asc'),
  page: z.number().int().positive().default(1).catch(1),
});

export const Route = createFileRoute('/products')({
  validateSearch: productsSearchSchema,
  component: ProductsPage,
});

function ProductsPage(): React.JSX.Element {
  const { category, sort, page } = Route.useSearch();

  const { data, isPending, isError, error } = useQuery(
    productsQueryOptions({
      ...(category !== undefined ? { category } : {}),
      sort,
      page,
      pageSize: 20,
    }),
  );

  if (isPending) return <p>Loading…</p>;
  if (isError) return <p role="alert">Failed to load: {error.message}</p>;

  return (
    <section>
      <h1>Products</h1>
      <p style={{ opacity: 0.6 }}>
        Page {data.page} of {data.totalPages} · {data.totalCount} total
      </p>
      <ul style={{ padding: 0, listStyle: 'none', display: 'grid', gap: '0.75rem' }}>
        {data.items.map((p) => (
          <li key={p.id} style={{ border: '1px solid #ddd', borderRadius: 8, padding: '0.75rem' }}>
            <strong>{p.title}</strong>
            <div style={{ opacity: 0.7 }}>
              {p.currency} {p.price.toFixed(2)}
            </div>
          </li>
        ))}
      </ul>
      <Outlet />
    </section>
  );
}
