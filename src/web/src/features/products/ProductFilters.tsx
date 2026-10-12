import { useQuery } from '@tanstack/react-query';
import { useNavigate } from '@tanstack/react-router';
import { useEffect, useState } from 'react';
import { Route as ProductsIndexRoute } from '@/routes/products.index';
import { useDebouncedValue } from '@/shared/lib/use-debounced-value';
import { Button } from '@/shared/ui';
import { categoriesQueryOptions } from './queries';

const SORT_OPTIONS = [
  { value: 'price-asc', label: 'Price: low to high' },
  { value: 'price-desc', label: 'Price: high to low' },
] as const;

export function ProductFilters(): React.JSX.Element {
  const search = ProductsIndexRoute.useSearch();
  const navigate = useNavigate();

  const categoriesQuery = useQuery(categoriesQueryOptions());

  const [q, setQ] = useState(search.q ?? '');
  const [minPrice, setMinPrice] = useState(
    search.minPrice !== undefined ? String(search.minPrice) : '',
  );
  const [maxPrice, setMaxPrice] = useState(
    search.maxPrice !== undefined ? String(search.maxPrice) : '',
  );

  const debouncedQ = useDebouncedValue(q, 300);
  const debouncedMin = useDebouncedValue(minPrice, 400);
  const debouncedMax = useDebouncedValue(maxPrice, 400);

  useEffect(() => {
    void navigate({
      to: '.',
      search: (prev) => ({
        ...prev,
        q: debouncedQ.length > 0 ? debouncedQ : undefined,
        minPrice: parseOptionalNumber(debouncedMin),
        maxPrice: parseOptionalNumber(debouncedMax),
      }),
      replace: true,
    });
  }, [debouncedQ, debouncedMin, debouncedMax, navigate]);

  return (
    <aside className="grid grid-cols-1 content-start items-start gap-x-4 gap-y-4 self-start rounded-xl border border-slate-200 bg-white p-4 shadow-sm sm:grid-cols-2 lg:sticky lg:top-4 lg:grid-cols-1">
      <h2 className="text-base font-semibold text-slate-900 sm:col-span-2 lg:col-span-1">
        Refine products
      </h2>

      <div className="grid content-start gap-2 sm:col-span-2 lg:col-span-1">
        <label htmlFor="filter-q" className="text-sm font-medium">
          Search
        </label>

        <input
          id="filter-q"
          value={q}
          onChange={(event) => setQ(event.target.value)}
          placeholder="Search products…"
          className="h-10 min-w-0 rounded-lg border border-slate-300 bg-white px-3 text-sm text-slate-900 outline-none transition placeholder:text-slate-400 focus:border-blue-500 focus:ring-2 focus:ring-blue-100"
        />
      </div>

      <div className="grid content-start gap-2">
        <label htmlFor="filter-category" className="text-sm font-medium">
          Category
        </label>

        <select
          id="filter-category"
          value={search.categoryId ?? ''}
          onChange={(event) => {
            const next = event.target.value === '' ? undefined : event.target.value;

            void navigate({
              to: '.',
              search: (prev) => ({
                ...prev,
                categoryId: next,
              }),
            });
          }}
          className="h-10 rounded-md border border-[var(--color-border)] bg-[var(--color-surface)] px-3"
        >
          <option value="">All categories</option>

          {categoriesQuery.data?.map((category) => (
            <option key={category.id} value={category.id}>
              {category.name}
            </option>
          ))}
        </select>
      </div>

      <div className="grid content-start gap-2">
        <label htmlFor="filter-sort" className="text-sm font-medium">
          Sort
        </label>

        <select
          id="filter-sort"
          value={search.sort}
          onChange={(event) => {
            const sort = event.target.value as (typeof SORT_OPTIONS)[number]['value'];

            void navigate({
              to: '.',
              search: (prev) => ({
                ...prev,
                sort,
              }),
            });
          }}
          className="h-10 rounded-md border border-[var(--color-border)] bg-[var(--color-surface)] px-3"
        >
          {SORT_OPTIONS.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>
      </div>

      <fieldset className="grid content-start gap-2 sm:col-span-2 lg:col-span-1">
        <legend className="mb-2 text-sm font-medium text-slate-800">Price range</legend>

        <div className="flex items-center gap-2">
          <input
            aria-label="Minimum price"
            type="number"
            min={0}
            value={minPrice}
            onChange={(event) => setMinPrice(event.target.value)}
            placeholder="Min"
            className="h-10 w-full min-w-0 rounded-lg border border-slate-300 bg-white px-3 text-sm text-slate-900 outline-none transition placeholder:text-slate-400 focus:border-blue-500 focus:ring-2 focus:ring-blue-100"
          />

          <span aria-hidden className="text-slate-400">
            –
          </span>

          <input
            aria-label="Maximum price"
            type="number"
            min={0}
            value={maxPrice}
            onChange={(event) => setMaxPrice(event.target.value)}
            placeholder="Max"
            className="h-10 w-full min-w-0 rounded-lg border border-slate-300 bg-white px-3 text-sm text-slate-900 outline-none transition placeholder:text-slate-400 focus:border-blue-500 focus:ring-2 focus:ring-blue-100"
          />
        </div>
      </fieldset>

      <Button
        variant="outline"
        size="sm"
        className="w-full border-slate-300 text-slate-700 hover:border-blue-300 hover:bg-blue-50 sm:col-span-2 lg:col-span-1"
        onClick={() => {
          setQ('');
          setMinPrice('');
          setMaxPrice('');

          void navigate({
            to: '.',
            search: () => ({
              sort: 'price-asc' as const,
            }),
          });
        }}
      >
        Reset filters
      </Button>
    </aside>
  );
}

function parseOptionalNumber(raw: string): number | undefined {
  if (raw.trim() === '') {
    return undefined;
  }

  const value = Number(raw);

  return Number.isFinite(value) && value >= 0 ? value : undefined;
}
