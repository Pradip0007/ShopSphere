import { createFileRoute, Outlet } from '@tanstack/react-router';
import { z } from 'zod';

const productsSearchSchema = z.object({
  category: z.string().optional(),
  sort: z.enum(['price-asc', 'price-desc', 'newest', 'popular']).default('newest').catch('newest'),
  page: z.number().int().positive().default(1).catch(1),
});

export const Route = createFileRoute('/products')({
  validateSearch: productsSearchSchema,
  component: ProductsPage,
});

function ProductsPage(): React.JSX.Element {
  const { category, sort, page } = Route.useSearch();

  return (
    <section>
      <h1>Products</h1>
      <p>category = {category ?? '(all)'}</p>
      <p>sort = {sort}</p>
      <p>page = {page}</p>
      <p style={{ opacity: 0.6 }}>Real listing lands Day 61. URL is already source of truth.</p>
      <Outlet />
    </section>
  );
}
